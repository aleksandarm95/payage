using Dapper;
using Payage.Api.Features.Payments.Capture.Model;
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

        private const string GET_PAYMENT_SQL = @"
        SELECT 
            id AS Id, 
            status AS Status, 
            amount AS Amount, 
            currency AS Currency, 
            captured_amount AS CapturedAmount,
            masked_card_number AS MaskedCardNumber,
            cardholder_name AS CardholderName,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM transactions
         WHERE id = @PaymentId;
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

        public async Task<PaymentData?> GetPaymentAsync(IDbConnection dbConnection, IDbTransaction dbTransaction, Guid paymentId, CancellationToken cancellationToken) 
            => await dbConnection.QuerySingleOrDefaultAsync<PaymentData>(
                GET_PAYMENT_SQL,
                new
                {
                    PaymentId = paymentId
                },
                transaction: dbTransaction);
    }

    public class PaymentData
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = default!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public decimal CapturedAmount { get; set; }
        public string MaskedCardNumber { get; set; } = default!;
        public string CardholderName { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
