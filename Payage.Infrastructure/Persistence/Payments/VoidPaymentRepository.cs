using Dapper;
using Payage.Application.Abstractions;
using Payage.Application.Features.Payments.Void.Models;
using System.Data;

namespace Payage.Application.Features.Payments
{
    public class VoidPaymentRepository : IVoidPaymentRepository
    {
        private const string VOID_SQL = @"
        UPDATE transactions
        SET
          status = 'VOIDED',
          updated_at = @Now,
          row_version = row_version + 1
        WHERE
          id = @Id
          AND status = 'AUTHORIZED'
        RETURNING
          id AS Id,
          status AS Status,
          amount AS Amount,
          currency AS Currency,
          updated_at AS UpdatedAt;
        ";

        private const string INSERT_EVENT_SQL = @"
        INSERT INTO transaction_events(transaction_id, event_type, amount, reason, created_at)
        VALUES (@Id, 'VOIDED', NULL, NULL, @Now);
        ";

        public Task<VoidPaymentData?> TryVoidAsync(IDbConnection dbConnection, IDbTransaction dbTransaction, Guid id, DateTimeOffset now)
        => dbConnection.QuerySingleOrDefaultAsync<VoidPaymentData>(
            VOID_SQL,
            new { Id = id, Now = now },
            transaction: dbTransaction);

        public Task InsertVoidedEventAsync(IDbConnection conn, IDbTransaction tx, Guid id, DateTimeOffset now)
        => conn.ExecuteAsync(
            INSERT_EVENT_SQL,
            new { Id = id, Now = now },
            transaction: tx);
    }
}
