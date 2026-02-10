using Payage.Application.Features.Payments.Capture.Models;
using System.Data;

namespace Payage.Application.Abstractions
{
    public interface ICapturePaymentRepository
    {
        public Task<CapturePaymentData?> TryCaptureAsync(IDbConnection dbConnection, IDbTransaction dbTransaction, Guid id, decimal captureAmount, DateTimeOffset now);

        public Task InsertCaptureEventAsync(IDbConnection dbConnection, IDbTransaction dbTransaction, Guid id, decimal amount, DateTimeOffset now);
    }
}
