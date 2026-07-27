using Ardalis.Specification;
using Transportation.Mediator.Helper.Common.Models;

namespace Transportation.Mediator.Helper.Common.Extensions;

public static class PaginationExtensions
{
    public static ISpecificationBuilder<T> WithPagination<T>(
        this ISpecificationBuilder<T> builder,
        PaginationInfo paginationInfo)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(paginationInfo);

        return builder
            .Skip(paginationInfo.Size * paginationInfo.Index)
            .Take(paginationInfo.Size);
    }
}