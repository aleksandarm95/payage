using Dapper;
using FluentValidation;
using Npgsql;
using Payage.Api.Common;
using Payage.Api.Features.Payments.Authorize.Models;
using Payage.Api.Infrastructure.Db;

namespace Payage.Api.Features.Payments.Authorize
{
    public class AuthorizePaymentHandler
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IValidator<AuthorizePaymentRequest> _validator;
        private readonly AuthorizePaymentRepository _repository;

        public AuthorizePaymentHandler(IDbConnectionFactory dbConnectionFactory, AuthorizePaymentRepository repository, IValidator<AuthorizePaymentRequest> validator)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _repository = repository;
            _validator = validator;
        }

        public async Task<AuthorizePaymentResponse> HandleAsync(AuthorizePaymentRequest request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var time = DateTimeOffset.UtcNow;
            var id = Guid.NewGuid();
            var masked = MaskCard(request.CardNumber);

            using var dbConnection = _dbConnectionFactory.Create();
            await ((dynamic)dbConnection).OpenAsync(cancellationToken);

            using var dbTransaction = dbConnection.BeginTransaction();

            try
            {
                await _repository.InsertTransactionAsync(dbConnection, dbTransaction, 
                    id, request.OrderReference, request.Amount, request.Currency,
                    masked, request.CardholderName, time);

                await _repository.InsertAuthorizedEventAsync(dbConnection, dbTransaction, id, time);

                dbTransaction.Commit();

                return new AuthorizePaymentResponse(
                    Id: id,
                    Status: Constants.AUTHORIZE_STATUS,
                    Amount: request.Amount,
                    Currency: request.Currency,
                    MaskedCardNumber: masked,
                    CreatedAt: time
                );
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                dbTransaction.Rollback();
                // We'll map this to 409 in controller for now
                throw new OrderReferenceConflictException(request.OrderReference, ex);
            }
            catch
            {
                dbTransaction.Rollback();
                throw;
            }
        }

        private static string MaskCard(string cardNumber)
        {
            var digits = new string(cardNumber.Where(char.IsDigit).ToArray());
            var first6 = digits.Substring(0, 6);
            var last4 = digits.Substring(digits.Length - 4);
            var stars = new string('*', digits.Length - 10);

            return $"{first6}{stars}{last4}";
        }
    }

    public sealed class OrderReferenceConflictException : Exception
    {
        public string OrderReference { get; }

        public OrderReferenceConflictException(string orderReference, Exception inner)
            : base($"Order reference '{orderReference}' already exists.", inner)
        {
            OrderReference = orderReference;
        }
    }
}
