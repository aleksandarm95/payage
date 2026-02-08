using Dapper;
using Payage.Api.Features.Payments.Shared.Models;
using System.Data;

namespace Payage.Api.Features.Payments.Shared
{
    public class PaymentRepository
    {
        private const string GET_PAYMENT_SQL = @"
        SELECT 
            id AS Id, 
            status AS Status, 
            amount AS Amount, 
            currency AS Currency, 
            captured_amount AS CapturedAmount,
            refunded_amount AS RefundedAmount,
            masked_card_number AS MaskedCardNumber,
            cardholder_name AS CardholderName,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt
        FROM transactions
            WHERE id = @PaymentId;
        ";

        public async Task<PaymentData?> GetPaymentAsync(IDbConnection dbConnection, IDbTransaction dbTransaction, Guid paymentId)
           => await dbConnection.QuerySingleOrDefaultAsync<PaymentData>(
               GET_PAYMENT_SQL,
               new
               {
                   PaymentId = paymentId
               },
               transaction: dbTransaction);
    }
}
