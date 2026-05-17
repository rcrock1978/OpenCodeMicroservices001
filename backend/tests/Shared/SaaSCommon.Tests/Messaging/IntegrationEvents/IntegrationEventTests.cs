using SaaSCommon.Messaging.IntegrationEvents;
using Xunit;

namespace SaaSCommon.Tests.Messaging.IntegrationEvents;

/// <summary>
/// Unit tests for all integration events in the shared messaging layer.
/// </summary>
public class IntegrationEventTests
{
    #region Base IntegrationEvent

    [Fact]
    public void IntegrationEvent_HasTenantId()
    {
        var evt = new ProductCreatedIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Name = "Test",
            Sku = "SKU",
            Price = 10.00m
        };

        Assert.NotEqual(Guid.Empty, evt.TenantId);
    }

    #endregion

    #region Catalog Events

    [Fact]
    public void ProductCreatedIntegrationEvent_HasCorrectEventType()
    {
        var evt = new ProductCreatedIntegrationEvent();
        Assert.Equal(nameof(ProductCreatedIntegrationEvent), evt.EventType);
    }

    [Fact]
    public void ProductCreatedIntegrationEvent_CanBeInstantiated()
    {
        var evt = new ProductCreatedIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Name = "Widget",
            Sku = "WID-001",
            Price = 29.99m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Assert.Equal("Widget", evt.Name);
        Assert.Equal("WID-001", evt.Sku);
        Assert.Equal(29.99m, evt.Price);
    }

    [Fact]
    public void ProductUpdatedIntegrationEvent_HasCorrectEventType()
    {
        var evt = new ProductUpdatedIntegrationEvent();
        Assert.Equal(nameof(ProductUpdatedIntegrationEvent), evt.EventType);
    }

    [Fact]
    public void ProductDeletedIntegrationEvent_HasCorrectEventType()
    {
        var evt = new ProductDeletedIntegrationEvent { ProductId = Guid.NewGuid() };
        Assert.Equal(nameof(ProductDeletedIntegrationEvent), evt.EventType);
    }

    [Fact]
    public void CategoryCreatedIntegrationEvent_CanHaveParentCategory()
    {
        var evt = new CategoryCreatedIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Subcategory",
            ParentCategoryId = Guid.NewGuid()
        };

        Assert.NotNull(evt.ParentCategoryId);
    }

    #endregion

    #region Customer Events

    [Fact]
    public void CustomerCreatedIntegrationEvent_HasCorrectEventType()
    {
        var evt = new CustomerCreatedIntegrationEvent();
        Assert.Equal(nameof(CustomerCreatedIntegrationEvent), evt.EventType);
    }

    [Fact]
    public void CustomerCreatedIntegrationEvent_CanBeInstantiated()
    {
        var evt = new CustomerCreatedIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User"
        };

        Assert.Equal("test@example.com", evt.Email);
        Assert.Equal("Test User", evt.FullName);
    }

    [Fact]
    public void CustomerUpdatedIntegrationEvent_HasCorrectEventType()
    {
        var evt = new CustomerUpdatedIntegrationEvent();
        Assert.Equal(nameof(CustomerUpdatedIntegrationEvent), evt.EventType);
    }

    #endregion

    #region Identity Events

    [Fact]
    public void TenantCreatedIntegrationEvent_HasCorrectEventType()
    {
        var evt = new TenantCreatedIntegrationEvent();
        Assert.Equal(nameof(TenantCreatedIntegrationEvent), evt.EventType);
    }

    [Fact]
    public void TenantCreatedIntegrationEvent_CanBeInstantiated()
    {
        var evt = new TenantCreatedIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            Name = "Acme Corp",
            Slug = "acme",
            CreatedAt = DateTimeOffset.UtcNow
        };

        Assert.Equal("Acme Corp", evt.Name);
        Assert.Equal("acme", evt.Slug);
    }

    [Fact]
    public void UserRegisteredIntegrationEvent_HasCorrectEventType()
    {
        var evt = new UserRegisteredIntegrationEvent();
        Assert.Equal(nameof(UserRegisteredIntegrationEvent), evt.EventType);
    }

    #endregion

    #region Inventory Events

    [Fact]
    public void InventoryReservedIntegrationEvent_HasReservedQuantities()
    {
        var evt = new InventoryReservedIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            ReservedQuantities = new Dictionary<Guid, int> { { Guid.NewGuid(), 5 } }
        };

        Assert.Single(evt.ReservedQuantities);
    }

    [Fact]
    public void InventoryReservationFailedIntegrationEvent_HasReason()
    {
        var evt = new InventoryReservationFailedIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Reason = "Out of stock"
        };

        Assert.Equal("Out of stock", evt.Reason);
    }

    [Fact]
    public void StockReleasedIntegrationEvent_HasReleasedQuantities()
    {
        var evt = new StockReleasedIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            ReleasedQuantities = new Dictionary<Guid, int>()
        };

        Assert.Empty(evt.ReleasedQuantities);
    }

    #endregion

    #region Notification Events

    [Fact]
    public void NotificationSentIntegrationEvent_HasCorrectProperties()
    {
        var evt = new NotificationSentIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            NotificationId = Guid.NewGuid(),
            Recipient = "user@example.com",
            Channel = "Email",
            TemplateKey = "welcome"
        };

        Assert.Equal("user@example.com", evt.Recipient);
        Assert.Equal("Email", evt.Channel);
    }

    #endregion

    #region Order Events

    [Fact]
    public void OrderPlacedIntegrationEvent_HasItems()
    {
        var evt = new OrderPlacedIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.00m,
            Items = new List<OrderItemDto>
            {
                new(Guid.NewGuid(), null, "Product A", "A-001", 25.00m, 2)
            }
        };

        Assert.Single(evt.Items);
        Assert.Equal(100.00m, evt.TotalAmount);
    }

    [Fact]
    public void OrderItemDto_CanBeInstantiated()
    {
        var item = new OrderItemDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Product",
            "SKU",
            10.00m,
            3);

        Assert.Equal("Product", item.ProductName);
        Assert.Equal(30.00m, item.UnitPrice * item.Quantity);
    }

    [Fact]
    public void OrderPaidIntegrationEvent_HasPaymentIntentId()
    {
        var evt = new OrderPaidIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            PaymentIntentId = Guid.NewGuid()
        };

        Assert.NotEqual(Guid.Empty, evt.PaymentIntentId);
    }

    [Fact]
    public void OrderCancelledIntegrationEvent_HasReason()
    {
        var evt = new OrderCancelledIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Reason = "Customer request"
        };

        Assert.Equal("Customer request", evt.Reason);
    }

    [Fact]
    public void OrderShippedIntegrationEvent_HasTrackingNumber()
    {
        var evt = new OrderShippedIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            TrackingNumber = "TRACK123"
        };

        Assert.Equal("TRACK123", evt.TrackingNumber);
    }

    #endregion

    #region Payment Events

    [Fact]
    public void PaymentProcessedIntegrationEvent_HasCorrectEventType()
    {
        var evt = new PaymentProcessedIntegrationEvent();
        Assert.Equal(nameof(PaymentProcessedIntegrationEvent), evt.EventType);
    }

    [Fact]
    public void PaymentProcessedIntegrationEvent_CanBeInstantiated()
    {
        var evt = new PaymentProcessedIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            PaymentIntentId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Amount = 99.99m,
            ProcessedAt = DateTimeOffset.UtcNow
        };

        Assert.Equal(99.99m, evt.Amount);
    }

    [Fact]
    public void PaymentRefundedIntegrationEvent_HasCorrectEventType()
    {
        var evt = new PaymentRefundedIntegrationEvent();
        Assert.Equal(nameof(PaymentRefundedIntegrationEvent), evt.EventType);
    }

    [Fact]
    public void PaymentFailedIntegrationEvent_HasFailureReason()
    {
        var evt = new PaymentFailedIntegrationEvent
        {
            TenantId = Guid.NewGuid(),
            PaymentIntentId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            FailureReason = "Card declined"
        };

        Assert.Equal("Card declined", evt.FailureReason);
    }

    #endregion
}
