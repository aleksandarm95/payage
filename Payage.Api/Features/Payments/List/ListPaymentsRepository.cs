using Dapper;
using Payage.Api.Features.Payments.Shared.Models;
using System.Data;

namespace Payage.Api.Features.Payments.List
{
    public class ListPaymentsRepository
    {
        public async Task<(IReadOnlyList<PaymentData> Items, long TotalCount)> GetListAsync(IDbConnection dbConnection, string? status, string? orderReference, int limit, int offset)
        {
            var whereParameters = new List<string>();
            var dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("Limit", limit);
            dynamicParameters.Add("Offset", offset);

            if(!string.IsNullOrEmpty(status))
            {
                whereParameters.Add("Status = @Status");
                dynamicParameters.Add("Status", status);
            }

            if(!string.IsNullOrEmpty(orderReference))
            {
                whereParameters.Add("OrderReference = @OrderReference");
                dynamicParameters.Add("OrderReference", orderReference);
            }

            var whereSql = whereParameters.Count > 0 ? $"WHERE {string.Join(" AND ", whereParameters)}" : string.Empty;
            var countSql = $@"
                SELECT COUNT(*) 
                FROM transactions
                {whereSql};";
                
                var listSql = $@"
                SELECT
                  id,
                  status,
                  amount,
                  currency,
                  order_reference AS OrderReference,
                  captured_amount AS CapturedAmount,
                  refunded_amount AS RefundedAmount,
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
