using Payage.Api.Common.Exceptions;
using Payage.Api.Features.Payments.Shared.Models;
using Payage.Api.Infrastructure.Db;

namespace Payage.Api.Features.Payments.Shared
{
    public class PaymentHandler
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly PaymentRepository _paymentRepository;

        public PaymentHandler(IDbConnectionFactory dbConnectionFactory, PaymentRepository paymentRepository)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _paymentRepository = paymentRepository;
        }

        internal async Task<PaymentData> HandleAsync(Guid paymentId, CancellationToken cancellationToken)
        {
            using var dbConnection = _dbConnectionFactory.Create();
            await ((dynamic)dbConnection).OpenAsync(cancellationToken);

            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var transaction = await _paymentRepository.GetPaymentAsync(dbConnection, dbTransaction, paymentId);
                if (transaction is null)
                    throw new TransactionNotFoundException(paymentId);

                return new PaymentData
                {
                    Id = transaction.Id,
                    Status = transaction.Status,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    CapturedAmount = transaction.CapturedAmount,
                    RefundedAmount = transaction.RefundedAmount,
                    MaskedCardNumber = transaction.MaskedCardNumber,
                    CardholderName = transaction.CardholderName,
                    CreatedAt = transaction.CreatedAt,
                    UpdatedAt = transaction.UpdatedAt
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
