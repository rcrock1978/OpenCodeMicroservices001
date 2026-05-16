using BillingService.Domain.Entities;
using BillingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Api.Endpoints;

/// <summary>
/// API endpoints for invoice management.
/// </summary>
public static class InvoiceEndpoints
{
    /// <summary>
    /// Maps invoice-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices").WithTags("Invoices").WithOpenApi();

        group.MapGet("/", async (BillingDbContext db) =>
            Results.Ok(await db.Invoices.ToListAsync()));

        group.MapGet("/tenant/{tenantId:guid}", async (Guid tenantId, BillingDbContext db) =>
            Results.Ok(await db.Invoices.Where(i => i.TenantId == tenantId).ToListAsync()));

        group.MapPost("/", async (CreateInvoiceRequest request, BillingDbContext db) =>
        {
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                SubscriptionId = request.SubscriptionId,
                Amount = request.Amount,
                Currency = request.Currency,
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd
            };
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();
            return Results.Created($"/api/invoices/{invoice.Id}", invoice);
        });

        group.MapPost("/{id:guid}/pay", async (Guid id, BillingDbContext db) =>
        {
            var invoice = await db.Invoices.FindAsync(id);
            if (invoice is null) return Results.NotFound();
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        return app;
    }
}

/// <summary>
/// Request model for creating an invoice.
/// </summary>
public record CreateInvoiceRequest(Guid TenantId, Guid SubscriptionId, decimal Amount, string Currency, DateTime PeriodStart, DateTime PeriodEnd);
