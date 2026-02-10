using Payage.Application.Features.Payments.Void.Models;
using System.Data;

namespace Payage.Application.Abstractions
{
    public interface IVoidPaymentRepository
    {
        public Task<VoidPaymentData?> TryVoidAsync(IDbConnection dbConnection, IDbTransaction dbTransaction, Guid id, DateTimeOffset now);
        public Task InsertVoidedEventAsync(IDbConnection conn, IDbTransaction tx, Guid id, DateTimeOffset now);
    }
}
