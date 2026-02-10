using Dapper;
using Payage.Application.Abstractions;
using Payage.Application.Features.Payments.Refund.Models;
using System.Data;

namespace Payage.Application.Features.Payments
{
    public class RefundPaymentRepository : IRefundPaymentRepository
    {
        private const string REFUND_SQL = @"
        UPDATE transactions
        SET
          refunded_amount = refunded_amount + @RefundAmount,
          status = CASE
                      WHEN (refunded_amount + @RefundAmount) = captured_amount THEN 'REFUNDED'
                      ELSE status
                   END,
          updated_at = @Now,
          row_version = row_version + 1
        WHERE
          id = @Id
          AND status = 'CAPTURED'
          AND refunded_amount + @RefundAmount <= captured_amount
        RETURNING
          id,
          status,
          amount,
          currency,
          captured_amount  AS CapturedAmount,
          refunded_amount  AS RefundedAmount,
          updated_at       AS UpdatedAt;
        ";

        private const string INSERT_EVENT_SQL = @"
        INSERT INTO transaction_events(transaction_id, event_type, amount, reason, created_at)
        VALUES (@Id, 'REFUNDED', @Amount, @Reason, @Now);
        ";

        public Task<RefundData?> TryRefundAsync(IDbConnection dbConnection, IDbTransaction dbTransaction, Guid id, decimal refundAmount, DateTimeOffset now)
            => dbConnection.QuerySingleOrDefaultAsync<RefundData>(
             REFUND_SQL,
             new 
             { 
                 Id = id, 
                 RefundAmount = refundAmount, 
                 Now = now 
             },
             transaction: dbTransaction);

        public Task InsertRefundEventAsync(IDbConnection dbConnection, IDbTransaction dbTransaction, Guid id, decimal amount, string reason, DateTimeOffset now)
            => dbConnection.ExecuteAsync(
                INSERT_EVENT_SQL,
                new 
                { 
                    Id = id, 
                    Amount = amount, 
                    Reason = reason, 
                    Now = now 
                },
                transaction: dbTransaction);


    }
}
