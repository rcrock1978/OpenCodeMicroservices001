using MediatR;
using CatalogService.Api.Endpoints;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Application.Commands;

/// <summary>
/// Command to create a new category.
/// </summary>
public record CreateCategoryCommand(
    Guid TenantId,
    string Name,
    Guid? ParentCategoryId
) : IRequest<CategoryResponse>;

/// <summary>
/// Handler for <see cref="CreateCategoryCommand"/>.
/// </summary>
public class CreateCategoryCommandHandler(CatalogDbContext db) : IRequestHandler<CreateCategoryCommand, CategoryResponse>
{
    /// <inheritdoc />
    public async Task<CategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Name = request.Name,
            ParentCategoryId = request.ParentCategoryId
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        return new CategoryResponse(
            category.Id, category.TenantId, category.Name, category.ParentCategoryId, category.IsActive);
    }
}
