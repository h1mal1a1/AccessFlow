namespace AccessFlow.Api.Options;

public static class PaginationHelper
{
    public static (int Page, int PageSize) Normalize(int page, int? pageSize, PaginationOptions options)
    {
        if (page < 1)
            page = 1;

        var actualPageSize = pageSize ?? options.DefaultPageSize;

        if (actualPageSize < 1)
            actualPageSize = options.DefaultPageSize;

        if (actualPageSize > options.MaxPageSize)
            actualPageSize = options.MaxPageSize;

        return (page, actualPageSize);
    }
}