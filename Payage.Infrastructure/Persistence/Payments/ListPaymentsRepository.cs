using Dapper;
using Payage.Application.Abstractions;
using Payage.Application.Features.Payments.Shared.Models;
using System.Data;

namespace Payage.Application.Features.Payments
{
    public class ListPaymentsRepository : IListPaymentsRepository
    {
        public async Task<(IReadOnlyList<PaymentData> Items, long TotalCount)> GetListAsync(IDbConnection dbConnection, string? status, string? orderReference, int limit, int offset)
        {
            var whereParameters = new List<string>();
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("Limit", limit);
            dynamicParameters.Add("Offset", offset);

            if(!string.IsNullOrEmpty(status))
            {
                whereParameters.Add("status = @Status");
                dynamicParameters.Add("Status", status);
            }

            if(!string.IsNullOrEmpty(orderReference))
            {
                whereParameters.Add("order_reference = @OrderReference");
                dynamicParameters.Add("OrderReference", orderReference);
            }

            var whereSql = whereParameters.Count > 0 ? $"WHERE {string.Join(" AND ", whereParameters)}" : string.Empty;
            var countSql = $@"
                SELECT COUNT(*) 
                FROM transactions
                {whereSql};";
                
                var listSql = $@"
                SELECT
                  id AS Id,
                  status AS Status,
                  amount AS Amount,
                  currency AS Currency,
                  order_reference AS OrderReference,
                  captured_amount AS CapturedAmount,
                  refunded_amount AS RefundedAmount,
                  masked_card_number AS MaskedCardNumber,
                  cardholder_name AS CardholderName,
                  created_at AS CreatedAt,
                  updated_at AS UpdatedAt
                FROM transactions
                {whereSql}
                ORDER BY created_at DESC
                LIMIT @Limit OFFSET @Offset;
                ";

            var total = await dbConnection.ExecuteScalarAsync<long>(countSql, dynamicParameters);
            var items = (await dbConnection.QueryAsync<PaymentData>(listSql, dynamicParameters)).ToList();

            return (items, total);
        }
    }
}
