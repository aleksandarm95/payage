using FluentValidation;
using Payage.Api.Common;
using Payage.Api.Common.Exceptions;
using Payage.Api.Features.Payments.Capture.Models;
using Payage.Api.Features.Payments.Shared;
using Payage.Api.Infrastructure.Db;

namespace Payage.Api.Features.Payments.Capture
{
    public class CapturePaymentHandler
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly CapturePaymentRepository _captureRepository;
        private readonly PaymentRepository _paymentRepository;
        private IValidator<CapturePaymentRequest> _validator;
        private readonly ILogger<CapturePaymentHandler> _logger;

        public CapturePaymentHandler(IDbConnectionFactory dbConnectionFactory, CapturePaymentRepository repository, PaymentRepository paymentRepository, 
            IValidator<CapturePaymentRequest> validator, ILogger<CapturePaymentHandler> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _captureRepository = repository;
            _validator = validator;
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        internal async Task<CapturePaymentResponse> HandleAsync(Guid paymentId, CapturePaymentRequest capturePaymentRequest, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(capturePaymentRequest, cancellationToken);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);
            _logger.LogInformation("Validation passed. Preparing to capture payment: {PaymentId} RequestedAmount: {RequestedAmount}", paymentId, capturePaymentRequest.Amount);

            var timeNow = DateTimeOffset.UtcNow;

            using var dbConnection = _dbConnectionFactory.Create();
            await ((dynamic)dbConnection).OpenAsync(cancellationToken);

            using var dbTransaction = dbConnection.BeginTransaction();

            try
            {
                var currentPayment = await _paymentRepository.GetPaymentAsync(dbConnection, dbTransaction, paymentId);
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

                var updated = await _captureRepository.TryCaptureAsync(dbConnection, dbTransaction, paymentId, captureAmount, timeNow);
                if (updated == null)
                {
                    // If the update failed => concurrency issue
                    // Recheck the payment to provide accurate error information
                    _logger.LogWarning("Capture update returned null for payment {PaymentId}, possible concurrency issue. Rechecking payment.", paymentId);

                    var recheckPayment = await _paymentRepository.GetPaymentAsync(dbConnection, dbTransaction, paymentId);
                    if (recheckPayment == null)
                        throw new TransactionNotFoundException(paymentId);

                    if (recheckPayment.Status != Constants.AUTHORIZE_STATUS)
                        throw new InvalidTransactionStateException(paymentId, recheckPayment.Status, Constants.CAPTURE_STATUS);

                    var reRemainingAmount = recheckPayment.Amount - recheckPayment.CapturedAmount;
                    if (captureAmount > reRemainingAmount)
                        throw new CaptureAmountExceedsAuthorizedException(paymentId, captureAmount, reRemainingAmount);

                    throw new InvalidOperationException("Capture failed for an unexpected reason.");
                }

                await _captureRepository.InsertCaptureEventAsync(dbConnection, dbTransaction, paymentId, captureAmount, timeNow);

                dbTransaction.Commit();
                _logger.LogInformation("Payment {PaymentId} captured successfully. CapturedAmount: {CapturedAmount} at {CreatedAt}", paymentId, captureAmount, timeNow);
                return new CapturePaymentResponse(updated.Id, updated.Status, updated.Amount, updated.Currency, updated.CapturedAmount, updated.UpdatedAt);
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError(ex, "Unhandled error while capturing payment {PaymentId}", paymentId);
                throw;
            }
        }
    }
}
