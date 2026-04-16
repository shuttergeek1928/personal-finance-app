namespace PersonalFinance.Services.Transactions.Application.DataTransferObjects.Response
{
    public class PaginatedApiResponse<T> : ApiResponse<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public static PaginatedApiResponse<T> SuccessPaginatedResult(T data, int page, int pageSize, int totalCount, string? message = null)
        {
            return new PaginatedApiResponse<T>
            {
                Success = true,
                Data = data,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Message = message
            };
        }
    }
}
