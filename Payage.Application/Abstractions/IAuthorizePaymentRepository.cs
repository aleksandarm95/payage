using System.Data;

namespace Payage.Application.Abstractions
{
    public interface IAuthorizePaymentRepository
    {
        Task InsertTransactionAsync(IDbConnection connection, IDbTransaction dbTransaction, Guid id, string orderReference, decimal amount,
            string currency, string maskedCard, string cardholder, DateTimeOffset now);
        Task InsertAuthorizedEventAsync(IDbConnection conn, IDbTransaction dbTransaction, Guid transactionId, DateTimeOffset now);
    }
}
