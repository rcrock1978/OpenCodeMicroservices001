using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace PaymentService.Api.Endpoints;

/// <summary>
/// API endpoints for payment management.
/// </summary>
public static class PaymentEndpoints
{
    /// <summary>
    /// Maps payment-related API endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments").WithTags("Payments").WithOpenApi();

        group.MapGet("/intents", async (PaymentDbContext db) =>
            Results.Ok(await db.PaymentIntents.AsNoTracking().ToListAsync()));

        group.MapGet("/intents/{id:guid}", async (Guid id, PaymentDbContext db) =>
            await db.PaymentIntents.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id) is PaymentIntent intent
                ? Results.Ok(intent)
                : Results.NotFound());

        group.MapPost("/intents", async (CreatePaymentIntentRequest request, PaymentDbContext db) =>
        {
            // Simulate payment processing
            var status = request.Amount > 0 && !request.TestFailure
                ? PaymentStatus.Succeeded
                : PaymentStatus.Failed;

            var intent = new PaymentIntent
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                OrderId = request.OrderId,
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Currency = request.Currency,
                IdempotencyKey = request.IdempotencyKey,
                Status = status,
                PaymentMethod = request.PaymentMethod,
                FailureReason = status == PaymentStatus.Failed ? "Test failure or zero amount" : null,
                CapturedAt = status == PaymentStatus.Succeeded ? DateTime.UtcNow : null
            };
            db.PaymentIntents.Add(intent);
            await db.SaveChangesAsync();
            return Results.Created($"/api/payments/intents/{intent.Id}", intent);
        });

        group.MapPost("/intents/{id:guid}/refund", async (Guid id, PaymentDbContext db) =>
        {
            var intent = await db.PaymentIntents.FindAsync(id);
            if (intent is null) return Results.NotFound();
            if (intent.Status != PaymentStatus.Succeeded)
                return Results.BadRequest("Only succeeded payments can be refunded");
            intent.Status = PaymentStatus.Refunded;
            await db.SaveChangesAsync();
            return Results.Ok(intent);
        });

        group.MapGet("/methods", async (PaymentDbContext db) =>
            Results.Ok(await db.PaymentMethods.AsNoTracking().ToListAsync()));

        group.MapPost("/methods", async (CreatePaymentMethodRequest request, PaymentDbContext db) =>
        {
            var method = new PaymentMethod
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                CustomerId = request.CustomerId,
                Type = request.Type,
                LastFour = request.LastFour,
                Brand = request.Brand,
                ExpMonth = request.ExpMonth,
                ExpYear = request.ExpYear,
                IsDefault = request.IsDefault
            };
            db.PaymentMethods.Add(method);
            await db.SaveChangesAsync();
            return Results.Created($"/api/payments/methods/{method.Id}", method);
        });

        return app;
    }
}

/// <summary>
/// Request model for creating a payment intent.
/// </summary>
public record CreatePaymentIntentRequest(Guid TenantId, Guid OrderId, Guid CustomerId, decimal Amount, string Currency, string IdempotencyKey, string? PaymentMethod, bool TestFailure);

/// <summary>
/// Request model for creating a payment method.
/// </summary>
public record CreatePaymentMethodRequest(Guid TenantId, Guid CustomerId, PaymentMethodType Type, string? LastFour, string? Brand, int? ExpMonth, int? ExpYear, bool IsDefault);
