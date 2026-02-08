using Dapper;
using Payage.Api.Features.Payments.Capture.Models;
using System.Data;

namespace Payage.Api.Features.Payments.Capture
{
    public class CapturePaymentRepository
    {
        private const string CAPTURE_SQL = @"
        UPDATE transactions
        SET
          captured_amount = captured_amount + @CaptureAmount,
          status = 'CAPTURED',
          updated_at = @Now,
          row_version = row_version + 1
        WHERE
          id = @Id
          AND status = 'AUTHORIZED'
          AND captured_amount + @CaptureAmount <= amount
        RETURNING
          id AS Id,
          status AS Status,
          amount AS Amount,
          currency AS Currency,
          captured_amount AS CapturedAmount,
          updated_at AS UpdatedAt;
        ";

        private const string INSERT_EVENT_SQL = @"
        INSERT INTO transaction_events(transaction_id, event_type, amount, reason, created_at)
        VALUES (@Id, 'CAPTURED', @Amount, NULL, @Now);
        ";

        public Task<CapturePaymentData?> TryCaptureAsync(IDbConnection dbConnection, IDbTransaction dbTransaction, Guid id, decimal captureAmount, DateTimeOffset now)
            => dbConnection.QuerySingleOrDefaultAsync<CapturePaymentData>(
                CAPTURE_SQL,
                new 
                { 
                    Id = id, 
                    CaptureAmount = captureAmount, 
                    Now = now 
                },
                transaction: dbTransaction);

       
        public Task InsertCaptureEventAsync(IDbConnection dbConnection, IDbTransaction dbTransaction, Guid id, decimal amount, DateTimeOffset now)
            => dbConnection.ExecuteAsync(
                INSERT_EVENT_SQL,
                new 
                { 
                    Id = id, 
                    Amount = amount, 
                    Now = now 
                },
                transaction: dbTransaction);
    }
}
