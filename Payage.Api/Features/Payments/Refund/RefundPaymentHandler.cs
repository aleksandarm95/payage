using FluentValidation;
using Payage.Api.Common;
using Payage.Api.Common.Exceptions;
using Payage.Api.Features.Payments.Capture.Models;
using Payage.Api.Features.Payments.Refund.Models;
using Payage.Api.Features.Payments.Shared;
using Payage.Api.Infrastructure.Db;
using System.Data.Common;
using System.Threading;
using System.Transactions;

namespace Payage.Api.Features.Payments.Refund
{
    public class RefundPaymentHandler
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly RefundPaymentRepository _refundRepository;
        private readonly PaymentRepository _paymentRepository;
        private readonly IValidator<RefundPaymentRequest> _validator;

        public RefundPaymentHandler(IDbConnectionFactory db, RefundPaymentRepository repo, PaymentRepository payments, IValidator<RefundPaymentRequest> validator)
        {
            _dbConnectionFactory = db;
            _paymentRepository = payments;
            _refundRepository = repo;
            _validator = validator;
        }

        public async Task<RefundPaymentResponse> HandleAsync(Guid paymentId, RefundPaymentRequest refundPaymentRequest, CancellationToken cancellationToken)
        {
            var validation = await _validator.ValidateAsync(refundPaymentRequest, cancellationToken);
            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var timeNow = DateTimeOffset.UtcNow;

            using var dbConnection = _dbConnectionFactory.Create();
            await ((dynamic)dbConnection).OpenAsync(cancellationToken);
            using var dbTransaction = dbConnection.BeginTransaction();

            try
            {
                var currentPayment = await _paymentRepository.GetPaymentAsync(dbConnection, dbTransaction, paymentId);
                if (currentPayment == null)
                    throw new TransactionNotFoundException(paymentId);

                if (currentPayment.Status != Constants.CAPTURE_STATUS)
                    throw new InvalidTransactionStateException(paymentId, currentPayment.Status, Constants.REFUND_STATUS);

                var remainingAmount = currentPayment.CapturedAmount - currentPayment.RefundedAmount;
                if (remainingAmount <= 0)
                    throw new RefundAmountExceedsCapturedException(paymentId, currentPayment.RefundedAmount, currentPayment.CapturedAmount);

                var refundAmount = refundPaymentRequest.Amount ?? remainingAmount;
                if(refundAmount > remainingAmount)
                    throw new RefundAmountExceedsCapturedException(paymentId, refundAmount, remainingAmount);

                var updated = await _refundRepository.TryRefundAsync(dbConnection, dbTransaction, paymentId, refundAmount, timeNow);
                if(updated == null)
                {
                    var recheckPayment = await _paymentRepository.GetPaymentAsync(dbConnection, dbTransaction, paymentId);
                    if (recheckPayment == null)
                        throw new TransactionNotFoundException(paymentId);

                    if (recheckPayment.Status != Constants.CAPTURE_STATUS)
                        throw new InvalidTransactionStateException(paymentId, recheckPayment.Status, Constants.REFUND_STATUS);

                    var reRemainingAmount = recheckPayment.CapturedAmount - recheckPayment.RefundedAmount;
                    if(refundAmount > reRemainingAmount)
                        throw new RefundAmountExceedsCapturedException(paymentId, refundAmount, reRemainingAmount);
                    
                    throw new InvalidOperationException("Refund failed for an unexpected reason.");
                }

                await _refundRepository.InsertRefundEventAsync(dbConnection, dbTransaction, paymentId, refundAmount, refundPaymentRequest.Reason ?? string.Empty, timeNow);

                dbTransaction.Commit();

                return new RefundPaymentResponse(updated.Id, updated.Status, updated.Amount, updated.Currency, updated.CapturedAmount, updated.RefundedAmount, updated.UpdatedAt);
            }
            catch
            {
                dbTransaction.Rollback();
                throw;
            }
        }
    }
}
