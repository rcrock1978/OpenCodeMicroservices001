using MediatR;
using CatalogService.Api.Endpoints;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Queries;

/// <summary>
/// Query to retrieve all categories.
/// </summary>
public record GetCategoriesQuery : IRequest<List<CategoryResponse>>;

/// <summary>
/// Handler for <see cref="GetCategoriesQuery"/>.
/// </summary>
public class GetCategoriesQueryHandler(CatalogDbContext db) : IRequestHandler<GetCategoriesQuery, List<CategoryResponse>>
{
    /// <inheritdoc />
    public async Task<List<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await db.Categories.AsNoTracking()
            .ToListAsync(cancellationToken);

        return categories.Select(c => new CategoryResponse(
            c.Id, c.TenantId, c.Name, c.ParentCategoryId, c.IsActive)).ToList();
    }
}
