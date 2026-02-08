using Payage.Api.Common;
using Payage.Api.Common.Exceptions;
using Payage.Api.Features.Payments.Shared;
using Payage.Api.Features.Payments.Void.Models;
using Payage.Api.Infrastructure.Db;

namespace Payage.Api.Features.Payments.Void
{
    public class VoidPaymentHandler
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly VoidPaymentRepository _voidRepository;
        private readonly PaymentRepository _paymentRepository;

        public VoidPaymentHandler(IDbConnectionFactory db, VoidPaymentRepository repository, PaymentRepository paymentRepository)
        {
            _dbConnectionFactory = db;
            _voidRepository = repository;
            _paymentRepository = paymentRepository;
        }

        public async Task<VoidPaymentResponse> HandleAsync(Guid paymentId, CancellationToken cancellationToken)
        {
            var timeNow = DateTimeOffset.UtcNow;

            using var dbConnection = _dbConnectionFactory.Create();
            await ((dynamic)dbConnection).OpenAsync(cancellationToken);

            using var dbTransaction = dbConnection.BeginTransaction();

            try
            {
                var currentPayment = await _paymentRepository.GetPaymentAsync(dbConnection, dbTransaction, paymentId, cancellationToken);
                if (currentPayment == null)
                    throw new TransactionNotFoundException(paymentId);

                if (currentPayment.Status != Constants.AUTHORIZE_STATUS)
                    throw new InvalidTransactionStateException(paymentId, currentPayment.Status, Constants.VOID_STATUS);

                var updated = await _voidRepository.TryVoidAsync(dbConnection, dbTransaction, paymentId, timeNow);

                if (updated is null)
                {
                    var recheckPayment = await _paymentRepository.GetPaymentAsync(dbConnection, dbTransaction, paymentId, cancellationToken);

                    if (recheckPayment is null)
                        throw new TransactionNotFoundException(paymentId);

                    if (recheckPayment.Status != Constants.AUTHORIZE_STATUS)
                        throw new InvalidTransactionStateException(paymentId, recheckPayment.Status, Constants.VOID_STATUS);

                    throw new InvalidOperationException("Changing status to void failed for an unexpected reason.");
                }

                await _voidRepository.InsertVoidedEventAsync(dbConnection, dbTransaction, paymentId, timeNow);
                dbTransaction.Commit();

                return new VoidPaymentResponse(updated.Id, updated.Status, updated.Amount, updated.Currency, updated.UpdatedAt);
            }
            catch
            {
                dbTransaction.Rollback();
                throw;
            }
        }
    }
}
