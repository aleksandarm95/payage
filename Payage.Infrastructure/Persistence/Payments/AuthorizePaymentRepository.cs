using Dapper;
using Payage.Application.Abstractions;
using Payage.Domain;
using System.Data;

namespace Payage.Application.Features.Payments
{
    public class AuthorizePaymentRepository : IAuthorizePaymentRepository
    {
        const string INSERT_TRANSACTION_SQL = @"
                    INSERT INTO transactions
                    (id, order_reference, status, amount, currency, masked_card_number, cardholder_name,
                     captured_amount, refunded_amount, created_at, updated_at, row_version)
                    VALUES
                    (@Id, @OrderReference, @Status, @Amount, @Currency, @MaskedCardNumber, @CardholderName,
                     0, 0, @CreatedAt, @UpdatedAt, 1);
                ";

        const string INSERT_TRANSACTION_EVENT_SQL = @"
                    INSERT INTO transaction_events (transaction_id, event_type, amount, reason, created_at)
                    VALUES (@TransactionId, @EventType, NULL, NULL, @CreatedAt);
                ";

        public Task InsertTransactionAsync(IDbConnection connection, IDbTransaction dbTransaction, Guid id, string orderReference, decimal amount, 
            string currency, string maskedCard, string cardholder, DateTimeOffset now)
            => connection.ExecuteAsync(INSERT_TRANSACTION_SQL, new
            {
                Id = id,
                OrderReference = orderReference,
                Status = Constants.AUTHORIZE_STATUS,
                Amount = amount,
                Currency = currency,
                MaskedCardNumber = maskedCard,
                CardholderName = cardholder,
                CreatedAt = now,
                UpdatedAt = now
            }, dbTransaction);

        public Task InsertAuthorizedEventAsync(IDbConnection conn, IDbTransaction dbTransaction, Guid transactionId, DateTimeOffset now) 
            => conn.ExecuteAsync(INSERT_TRANSACTION_EVENT_SQL, new 
            { 
                TransactionId = transactionId,
                EventType = Constants.AUTHORIZE_STATUS,
                CreatedAt = now 
            }, dbTransaction);   
    }
}