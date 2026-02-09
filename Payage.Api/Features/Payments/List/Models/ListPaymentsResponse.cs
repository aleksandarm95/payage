using Payage.Api.Features.Payments.Shared.Models;

namespace Payage.Api.Features.Payments.List.Models
{
    public record ListPaymentsResponse(
        IReadOnlyList<PaymentData> Items,
        int Page,
        int PageSize,
        long TotalCount,
        bool HasNext
    );
}
