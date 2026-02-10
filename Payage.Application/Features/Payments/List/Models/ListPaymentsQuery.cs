namespace Payage.Application.Features.Payments.List.Models
{
    public record ListPaymentsQuery(
        int Page = 1,
        int PageSize = 20,
        string? Status = null,
        string? OrderReference = null
    );
}
