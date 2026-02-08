using FluentValidation;
using Payage.Api.Common;
using Payage.Api.Common.Exceptions;
using Payage.Api.Features.Payments.Capture.Model;
using Payage.Api.Infrastructure.Db;

namespace Payage.Api.Features.Payments.Capture
{
    public class CapturePaymentHandler
    {
        private IDbConnectionFactory _dbConnectionFactory;
        private CapturePaymentRepository _repository;
        private IValidator<CapturePaymentRequest> _validator;

        public CapturePaymentHandler(IDbConnectionFactory dbConnectionFactory, CapturePaymentRepository repository, IValidator<CapturePaymentRequest> validator)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _repository = repository;
            _validator = validator;
        }

        internal async Task<CapturePaymentResponse> HandleAsync(Guid paymentId, CapturePaymentRequest capturePaymentRequest, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(capturePaymentRequest, cancellationToken);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var timeNow = DateTimeOffset.UtcNow;

            using var dbConnection = _dbConnectionFactory.Create();
            await ((dynamic)dbConnection).OpenAsync(cancellationToken);

            using var dbTransaction = dbConnection.BeginTransaction();

            try
            {
                var currentPayment = await _repository.GetPaymentAsync(dbConnection, dbTransaction, paymentId, cancellationToken);
                if(currentPayment == null)
                    throw new TransactionNotFoundException(paymentId);

                if (currentPayment.Status != Constants.AUTHORIZE_STATUS)
                    throw new InvalidTransactionStateException(paymentId, currentPayment.Status, Constants.CAPTURE_STATUS);

                var remainingAmount = currentPayment.Amount - currentPayment.CapturedAmount;
                if(remainingAmount <= 0)
                    throw new CaptureAmountExceedsAuthorizedException(paymentId, capturePaymentRequest.Amount ?? remainingAmount, remainingAmount);
                
                var captureAmount = capturePaymentRequest.Amount ?? remainingAmount;
                if (captureAmount > remainingAmount)
                    throw new CaptureAmountExceedsAuthorizedException(paymentId, captureAmount, remainingAmount);

                var updated = await _repository.TryCaptureAsync(dbConnection, dbTransaction, paymentId, captureAmount, timeNow);
                if (updated == null)
                {
                    // If the update failed => concurrency issue
                    // Recheck the payment to provide accurate error information
                    var recheckPayment = await _repository.GetPaymentAsync(dbConnection, dbTransaction, paymentId, cancellationToken);
                    if (recheckPayment == null)
                        throw new TransactionNotFoundException(paymentId);

                    if (recheckPayment.Status != Constants.AUTHORIZE_STATUS)
                        throw new InvalidTransactionStateException(paymentId, recheckPayment.Status, Constants.CAPTURE_STATUS);

                    var reRemainingAmount = recheckPayment.Amount - recheckPayment.CapturedAmount;
                    throw new CaptureAmountExceedsAuthorizedException(paymentId, reRemainingAmount, reRemainingAmount);
                }

                await _repository.InsertCaptureEventAsync(dbConnection, dbTransaction, paymentId, captureAmount, timeNow);

                dbTransaction.Commit();
                return new CapturePaymentResponse(updated.Id, updated.Status, updated.Amount, updated.Currency, updated.CapturedAmount, updated.UpdatedAt);
            }
            catch (Exception)
            {
                dbTransaction.Rollback();
                throw;
            }
        }
    }
}
