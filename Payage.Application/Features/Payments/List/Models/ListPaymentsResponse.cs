using Payage.Application.Features.Payments.Shared.Models;

namespace Payage.Application.Features.Payments.List.Models
{
    public record ListPaymentsResponse(
        IReadOnlyList<PaymentData> Items,
        int Page,
        int PageSize,
        long TotalCount,
        bool HasNext
    );
}
