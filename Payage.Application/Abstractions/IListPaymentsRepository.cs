using Payage.Application.Features.Payments.Shared.Models;
using System.Data;

namespace Payage.Application.Abstractions
{
    public interface IListPaymentsRepository
    {
        public Task<(IReadOnlyList<PaymentData> Items, long TotalCount)> GetListAsync(IDbConnection dbConnection, string? status, string? orderReference, int limit, int offset);
    }
}
