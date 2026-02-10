using Payage.Application.Features.Payments.Shared.Models;
using System.Data;

namespace Payage.Application.Abstractions
{
    public interface IPaymentRepository
    {
        Task<PaymentData?> GetPaymentAsync(IDbConnection dbConnection, IDbTransaction dbTransaction, Guid paymentId);
    }
}
