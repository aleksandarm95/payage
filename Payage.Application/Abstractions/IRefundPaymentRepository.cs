using Payage.Application.Features.Payments.Refund.Models;
using System.Data;

namespace Payage.Application.Abstractions
{
    public interface IRefundPaymentRepository
    {
        Task<RefundData?> TryRefundAsync(IDbConnection dbConnection, IDbTransaction dbTransaction, Guid id, decimal refundAmount, DateTimeOffset now);
        Task InsertRefundEventAsync(IDbConnection dbConnection, IDbTransaction dbTransaction, Guid id, decimal amount, string reason, DateTimeOffset now);
    }
}
