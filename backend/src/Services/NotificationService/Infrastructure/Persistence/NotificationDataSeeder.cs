using NotificationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Infrastructure.Persistence;

/// <summary>
/// Seeds the NotificationService database with 3000 sample notification records.
/// </summary>
public static class NotificationDataSeeder
{
    private static readonly Guid[] TenantIds =
    [
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Guid.Parse("33333333-3333-3333-3333-333333333333"),
        Guid.Parse("44444444-4444-4444-4444-444444444444")
    ];

    private static readonly string[] FirstNames =
    [
        "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda",
        "William", "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica",
        "Thomas", "Sarah", "Charles", "Karen", "Daniel", "Nancy", "Matthew", "Lisa",
        "Anthony", "Betty", "Mark", "Margaret", "Donald", "Sandra", "Steven", "Ashley",
        "Paul", "Kimberly", "Andrew", "Emily", "Joshua", "Donna", "Kenneth", "Michelle",
        "Kevin", "Dorothy", "Brian", "Carol", "George", "Amanda", "Timothy", "Melissa",
        "Ronald", "Deborah", "Edward", "Stephanie", "Jason", "Rebecca", "Jeffrey", "Sharon",
        "Ryan", "Laura", "Jacob", "Cynthia", "Gary", "Kathleen", "Nicholas", "Amy",
        "Eric", "Shirley", "Jonathan", "Angela", "Stephen", "Anna", "Larry", "Brenda",
        "Justin", "Pamela", "Scott", "Emma", "Brandon", "Nicole", "Benjamin", "Helen",
        "Samuel", "Samantha", "Gregory", "Katherine", "Frank", "Christine", "Alexander", "Debra",
        "Patrick", "Rachel", "Raymond", "Catherine", "Jack", "Carolyn", "Dennis", "Janet",
        "Jerry", "Ruth", "Tyler", "Maria"
    ];

    private static readonly string[] LastNames =
    [
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas",
        "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White",
        "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young",
        "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
        "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell",
        "Carter", "Roberts", "Gomez", "Phillips", "Evans", "Turner", "Diaz", "Parker",
        "Cruz", "Edwards", "Collins", "Reyes", "Stewart", "Morris", "Morales", "Murphy",
        "Cook", "Rogers", "Gutierrez", "Ortiz", "Morgan", "Cooper", "Peterson", "Bailey",
        "Reed", "Kelly", "Howard", "Ramos", "Kim", "Cox", "Ward", "Richardson",
        "Watson", "Brooks", "Chavez", "Wood", "James", "Bennett", "Gray", "Mendoza",
        "Ruiz", "Hughes", "Price", "Alvarez", "Castillo", "Sanders", "Patel", "Myers",
        "Long", "Ross", "Foster", "Jimenez"
    ];

    private static readonly string[] NotificationSubjects =
    [
        "Welcome to our store!", "Your order has been placed", "Order shipped", "Order delivered",
        "Payment received", "Payment failed", "Your refund is processing", "Refund completed",
        "Account created successfully", "Password reset requested", "Password changed", "New login detected",
        "Your cart is waiting", "Item back in stock", "Price drop alert", "Special offer just for you",
        "Shipping address updated", "Order cancelled", "Return initiated", "Return received",
        "Review your purchase", "Subscription renewed", "Invoice ready", "Delivery attempt failed",
        "Verify your email address", "Two-factor authentication enabled", "Profile updated", "New message received",
        "Appointment confirmed", "Appointment reminder", "Appointment cancelled", "Feedback request",
        "Product recommendation", "Birthday discount!", "Loyalty points earned", "Loyalty reward available",
        "Gift card received", "Gift card balance low", "Wishlist item on sale", "Order delayed",
        "Out for delivery", "Delivery confirmed", "Pickup ready", "Store pickup reminder",
        "Membership upgraded", "Membership expiring soon", "Payment method expired", "Auto-payment setup",
        "Abandoned cart reminder", "Flash sale starting now!", "Exclusive early access", "Restock notification"
    ];

    private static readonly string[] TemplateKeys =
    [
        "welcome-email", "order-confirmation", "order-shipped", "order-delivered",
        "payment-success", "payment-failed", "refund-processing", "refund-completed",
        "account-created", "password-reset", "password-changed", "security-alert",
        "cart-abandoned", "back-in-stock", "price-drop", "promotional-offer",
        "address-updated", "order-cancelled", "return-initiated", "return-received",
        "review-request", "subscription-renewed", "invoice-ready", "delivery-failed",
        "email-verification", "2fa-enabled", "profile-updated", "new-message",
        "appointment-confirmed", "appointment-reminder", "appointment-cancelled", "feedback-request",
        "product-recommendation", "birthday-discount", "loyalty-earned", "loyalty-reward",
        "gift-card-received", "gift-card-low", "wishlist-sale", "order-delayed"
    ];

    private static readonly string[] ErrorMessages =
    [
        "SMTP connection timeout", "Invalid recipient address", "Mailbox full", "Domain not found",
        "Message rejected by server", "Rate limit exceeded", "DNS lookup failed", "Network unreachable",
        "Authentication failed", "TLS handshake failed", "Message too large", "Temporary failure"
    ];

    /// <summary>
    /// Seeds the database with 3000 notification records (Templates + Notifications).
    /// </summary>
    public static async Task SeedAsync(NotificationDbContext dbContext, ILogger logger)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Notifications.AnyAsync())
        {
            logger.LogInformation("Database already contains notification data. Skipping seed.");
            return;
        }

        var rng = new Random(42);

        // Distribution: 500 Templates + 2500 Notifications = 3000 total
        const int templateCount = 500;
        const int notificationCount = 2500;

        var templates = new List<Template>();
        var notifications = new List<Notification>();

        // Generate Templates
        for (int i = 0; i < templateCount; i++)
        {
            var tenantId = TenantIds[rng.Next(TenantIds.Length)];
            var key = TemplateKeys[rng.Next(TemplateKeys.Length)];
            var channel = PickNotificationChannel(rng);
            var subject = NotificationSubjects[rng.Next(NotificationSubjects.Length)];

            templates.Add(new Template
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Key = $"{key}-{i:D4}",
                Subject = subject,
                BodyHtml = $"<html><body><h1>{subject}</h1><p>This is the HTML version of the {key} template.</p></body></html>",
                BodyText = $"{subject}\n\nThis is the text version of the {key} template.",
                Channel = channel,
                CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(0, 730)).AddHours(-rng.Next(0, 24))
            });
        }

        await dbContext.Templates.AddRangeAsync(templates);
        await dbContext.SaveChangesAsync();

        // Generate Notifications
        for (int i = 0; i < notificationCount; i++)
        {
            var tenantId = TenantIds[rng.Next(TenantIds.Length)];
            var firstName = FirstNames[rng.Next(FirstNames.Length)];
            var lastName = LastNames[rng.Next(LastNames.Length)];
            var email = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}.{rng.Next(1, 9999)}@example.com";
            var type = PickNotificationType(rng);
            var status = PickNotificationStatus(rng);
            var subject = NotificationSubjects[rng.Next(NotificationSubjects.Length)];
            var createdAt = DateTime.UtcNow.AddDays(-rng.Next(0, 365)).AddHours(-rng.Next(0, 24));
            DateTime? sentAt = null;
            string? errorMessage = null;

            if (status == NotificationStatus.Sent)
            {
                sentAt = createdAt.AddMinutes(rng.Next(1, 60));
            }
            else if (status == NotificationStatus.Failed)
            {
                sentAt = createdAt.AddMinutes(rng.Next(1, 60));
                errorMessage = ErrorMessages[rng.Next(ErrorMessages.Length)];
            }
            else if (status == NotificationStatus.RetryScheduled)
            {
                errorMessage = ErrorMessages[rng.Next(ErrorMessages.Length)];
            }

            // Body based on type
            var body = type switch
            {
                NotificationType.Sms => $"Hi {firstName}, {subject}. Reply STOP to opt out.",
                NotificationType.Push => $"🔔 {subject} - Tap to view details.",
                NotificationType.Webhook => $"{{\"event\":\"{subject.Replace(" ", "_").ToLowerInvariant()}\",\"recipient\":\"{email}\",\"timestamp\":\"{createdAt:O}\"}}",
                _ => $"Hello {firstName} {lastName},\n\n{subject}\n\nThank you for choosing us!\n\nBest regards,\nThe Team"
            };

            notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RecipientEmail = email,
                Subject = subject,
                Body = body,
                Type = type,
                Status = status,
                CreatedAt = createdAt,
                SentAt = sentAt,
                ErrorMessage = errorMessage
            });
        }

        await dbContext.Notifications.AddRangeAsync(notifications);
        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Seeded NotificationService with {TemplateCount} templates and {NotificationCount} notifications.",
            templates.Count, notifications.Count);
    }

    private static NotificationChannel PickNotificationChannel(Random rng)
    {
        var roll = rng.NextDouble();
        if (roll < 0.70) return NotificationChannel.Email;
        if (roll < 0.90) return NotificationChannel.Sms;
        return NotificationChannel.Push;
    }

    private static NotificationType PickNotificationType(Random rng)
    {
        var roll = rng.NextDouble();
        if (roll < 0.60) return NotificationType.Email;
        if (roll < 0.80) return NotificationType.Sms;
        if (roll < 0.95) return NotificationType.Push;
        return NotificationType.Webhook;
    }

    private static NotificationStatus PickNotificationStatus(Random rng)
    {
        var roll = rng.NextDouble();
        if (roll < 0.55) return NotificationStatus.Sent;
        if (roll < 0.75) return NotificationStatus.Pending;
        if (roll < 0.90) return NotificationStatus.Failed;
        return NotificationStatus.RetryScheduled;
    }
}
