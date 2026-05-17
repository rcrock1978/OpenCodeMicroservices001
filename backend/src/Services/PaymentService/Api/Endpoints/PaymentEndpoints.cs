using MediatR;
using PaymentService.Application.Commands;
using PaymentService.Application.Queries;
using PaymentService.Domain.Entities;

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

        group.MapGet("/intents", async (IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(new GetPaymentIntentsQuery(), cancellationToken)));

        group.MapGet("/intents/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
            await mediator.Send(new GetPaymentIntentByIdQuery(id), cancellationToken) is PaymentIntent intent
                ? Results.Ok(intent)
                : Results.NotFound());

        group.MapPost("/intents", async (CreatePaymentIntentRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CreatePaymentIntentCommand(
                request.TenantId,
                request.OrderId,
                request.CustomerId,
                request.Amount,
                request.Currency,
                request.IdempotencyKey,
                request.PaymentMethod,
                request.TestFailure);
            var intent = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/payments/intents/{intent.Id}", intent);
        });

        group.MapPost("/intents/{id:guid}/refund", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            try
            {
                var intent = await mediator.Send(new RefundPaymentIntentCommand(id), cancellationToken);
                return intent is null ? Results.NotFound() : Results.Ok(intent);
            }
            catch (InvalidOperationException ex) when (ex.Message == "Only succeeded payments can be refunded")
            {
                return Results.BadRequest(ex.Message);
            }
        });

        group.MapGet("/methods", async (IMediator mediator, CancellationToken cancellationToken) =>
            Results.Ok(await mediator.Send(new GetPaymentMethodsQuery(), cancellationToken)));

        group.MapPost("/methods", async (CreatePaymentMethodRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CreatePaymentMethodCommand(
                request.TenantId,
                request.CustomerId,
                request.Type,
                request.LastFour,
                request.Brand,
                request.ExpMonth,
                request.ExpYear,
                request.IsDefault);
            var method = await mediator.Send(command, cancellationToken);
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
