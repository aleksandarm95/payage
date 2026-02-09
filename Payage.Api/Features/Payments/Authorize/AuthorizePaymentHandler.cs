using Dapper;
using FluentValidation;
using Npgsql;
using Payage.Api.Common;
using Payage.Api.Common.Exceptions;
using Payage.Api.Features.Payments.Authorize.Models;
using Payage.Api.Infrastructure.Db;

namespace Payage.Api.Features.Payments.Authorize
{
    public class AuthorizePaymentHandler
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IValidator<AuthorizePaymentRequest> _validator;
        private readonly AuthorizePaymentRepository _repository;
        private readonly ILogger<AuthorizePaymentHandler> _logger;

        public AuthorizePaymentHandler(IDbConnectionFactory dbConnectionFactory, AuthorizePaymentRepository repository, IValidator<AuthorizePaymentRequest> validator, ILogger<AuthorizePaymentHandler> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _repository = repository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<AuthorizePaymentResponse> HandleAsync(AuthorizePaymentRequest request, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(request, cancellationToken);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var timeNow = DateTimeOffset.UtcNow;
            var id = Guid.NewGuid();
            var masked = MaskCard(request.CardNumber);

            _logger.LogInformation("Validation passed. Preparing to authorize payment: {PaymentId} for OrderReference: {OrderReference}, Amount: {Amount} {Currency}",
                id, request.OrderReference, request.Amount, request.Currency);

            using var dbConnection = _dbConnectionFactory.Create();
            await ((dynamic)dbConnection).OpenAsync(cancellationToken);

            using var dbTransaction = dbConnection.BeginTransaction();

            try
            {
                await _repository.InsertTransactionAsync(dbConnection, dbTransaction, 
                    id, request.OrderReference, request.Amount, request.Currency,
                    masked, request.CardholderName, timeNow);

                await _repository.InsertAuthorizedEventAsync(dbConnection, dbTransaction, id, timeNow);

                dbTransaction.Commit();

                _logger.LogInformation("Payment {PaymentId} authorized successfully for OrderReference {OrderReference} at {CreatedAt}", id, request.OrderReference, timeNow);

                return new AuthorizePaymentResponse(
                    Id: id,
                    Status: Constants.AUTHORIZE_STATUS,
                    Amount: request.Amount,
                    Currency: request.Currency,
                    MaskedCardNumber: masked,
                    CreatedAt: timeNow
                );
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                dbTransaction.Rollback();
                _logger.LogWarning(ex,"Order reference conflict while authorizing payment {PaymentId} for OrderReference {OrderReference}",
                   id, request.OrderReference);
                throw new OrderReferenceConflictException(request.OrderReference, ex);
            }
            catch(Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError(ex, "Unhandled error while authorizing payment {PaymentId} for OrderReference {OrderReference}", id, request.OrderReference);
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
}
