using MediatR;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Application.Commands;

/// <summary>
/// Command to create a new notification template.
/// </summary>
public record CreateTemplateCommand(
    Guid TenantId,
    string Key,
    string Subject,
    string? BodyHtml,
    string? BodyText,
    NotificationChannel Channel) : IRequest<Template>;

/// <summary>
/// Handles the <see cref="CreateTemplateCommand"/>.
/// </summary>
public class CreateTemplateHandler(NotificationDbContext db) : IRequestHandler<CreateTemplateCommand, Template>
{
    /// <inheritdoc />
    public async Task<Template> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new Template
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Key = request.Key,
            Subject = request.Subject,
            BodyHtml = request.BodyHtml,
            BodyText = request.BodyText,
            Channel = request.Channel
        };

        db.Templates.Add(template);
        await db.SaveChangesAsync(cancellationToken);
        return template;
    }
}
