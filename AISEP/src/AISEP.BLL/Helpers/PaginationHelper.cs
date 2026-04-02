using AISEP.BLL.DTOs.Responses;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Helpers
{
    public static class PaginationHelper
    {
        public static async Task<PagedResult<TResponse>> PaginateAsync<TEntity, TResponse>(
            IQueryable<TEntity> query,
            SieveModel model,
            ISieveProcessor sieveProcessor,
            Func<TEntity, TResponse> selector)
        {
            model ??= new SieveModel();
            if (!model.Page.HasValue || model.Page.Value <= 0)
            {
                model.Page = 1;
            }
            if (!model.PageSize.HasValue || model.PageSize.Value <= 0)
            {
                model.PageSize = 10;
            }

            var totalCount = await sieveProcessor
                .Apply(model, query, applyPagination: false, applySorting: false)
                .CountAsync();

            var items = await sieveProcessor
                .Apply(model, query)
                .ToListAsync();

            var page = model.Page.Value;
            var pageSize = model.PageSize.Value;

            return new PagedResult<TResponse>
            {
                Page       = page,
                PageSize   = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items      = items.Select(selector)
            };
        }
    }
}
