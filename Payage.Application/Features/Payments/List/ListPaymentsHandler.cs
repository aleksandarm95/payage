using Payage.Application.Features.Payments.List.Models;
using Payage.Application.Abstractions;

namespace Payage.Application.Features.Payments.List
{
    public class ListPaymentsHandler
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IListPaymentsRepository _listPaymentsRepository;

        public ListPaymentsHandler(IDbConnectionFactory dbConnectionFactory, IListPaymentsRepository listPaymentsRepository)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _listPaymentsRepository = listPaymentsRepository;
        }
        public async Task<ListPaymentsResponse> HandleAsync(ListPaymentsQuery query, CancellationToken cancellationToken)
        {
            // Ensure that query numbers are valid, or take default values
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
            if (pageSize > 100) 
                pageSize = 100;

            var offset = (page - 1) * pageSize;

            using var dbConnection = _dbConnectionFactory.Create();
            await ((dynamic)dbConnection).OpenAsync(cancellationToken);
            var (items, totalCount) = await _listPaymentsRepository.GetListAsync(dbConnection, query.Status, query.OrderReference, pageSize, offset);

            var hasNextPage = offset + pageSize < totalCount;
            return new ListPaymentsResponse(items, page, pageSize, totalCount, hasNextPage);
        }
    }
}
