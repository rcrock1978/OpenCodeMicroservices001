using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Persistence;

/// <summary>
/// Seeds the CustomerService database with initial demo data.
/// </summary>
public static class CustomerDataSeeder
{
    private static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

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

    private static readonly string[] StreetNames =
    [
        "Main St", "Oak Ave", "Maple Dr", "Cedar Ln", "Pine Rd", "Elm St", "Washington Ave",
        "Lakeview Dr", "Riverside Rd", "Highland Ave", "Sunset Blvd", "Forest Ln", "Broadway",
        "Park Ave", "Center St", "Mill Rd", "Church St", "King Ave", "Hilltop Dr", "Valley Rd",
        "Spring St", "Union Ave", "Jefferson Blvd", "Madison Ln", "Adams Rd", "Monroe St",
        "Jackson Ave", "Wilson Dr", "Clinton Rd", "Harrison St", "Lincoln Ave", "Grant Blvd",
        "Franklin Ln", "Hamilton Rd", "Garfield St", "Arthur Ave", "Cleveland Dr", "Harrison Rd",
        "Taylor St", "Tyler Ave", "Polk Ln", "Van Buren Rd", "Buchanan St", "Pierce Ave",
        "Fillmore Dr", "Johnson Rd", "Kennedy Blvd", "Reagan St", "Carter Ave", "Bush Ln",
        "Clinton Rd", "Obama Dr", "Trump Ave", "Biden St", "Roosevelt Rd", "Truman Ave",
        "Eisenhower Ln", "Nixon Rd", "Ford St", "Carter Ave", "Reagan Blvd", "Bush Dr",
        "Obama Ln", "Washington Rd", "Adams St", "Jefferson Ave", "Madison Dr", "Monroe Ln",
        "Jackson Rd", "Van Buren St", "Harrison Ave", "Tyler Dr", "Polk Ln", "Taylor Rd",
        "Fillmore St", "Pierce Ave", "Buchanan Dr", "Lincoln Rd", "Johnson St", "Grant Ave",
        "Hayes Dr", "Garfield Ln", "Arthur Rd", "Cleveland St", "McKinley Ave", "Roosevelt Dr",
        "Taft Ln", "Wilson Rd", "Harding St", "Coolidge Ave", "Hoover Dr", "Truman Ln",
        "Eisenhower Rd", "Kennedy St", "Johnson Ave", "Nixon Dr", "Ford Ln", "Carter Rd",
        "Reagan St", "Bush Ave", "Clinton Dr", "Obama Ln", "Trump Rd", "Biden St"
    ];

    private static readonly string[] Cities =
    [
        "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio",
        "San Diego", "Dallas", "San Jose", "Austin", "Jacksonville", "Fort Worth", "Columbus",
        "Charlotte", "San Francisco", "Indianapolis", "Seattle", "Denver", "Washington",
        "Boston", "El Paso", "Nashville", "Detroit", "Oklahoma City", "Portland", "Las Vegas",
        "Louisville", "Baltimore", "Milwaukee", "Albuquerque", "Tucson", "Fresno", "Sacramento",
        "Mesa", "Kansas City", "Atlanta", "Long Beach", "Colorado Springs", "Raleigh",
        "Omaha", "Miami", "Oakland", "Minneapolis", "Tulsa", "Cleveland", "Wichita",
        "Arlington", "New Orleans", "Bakersfield", "Tampa", "Aurora", "Honolulu", "Anaheim",
        "Santa Ana", "Corpus Christi", "Riverside", "Lexington", "Stockton", "Henderson",
        "Saint Paul", "St. Louis", "Cincinnati", "Pittsburgh", "Greensboro", "Anchorage",
        "Plano", "Lincoln", "Orlando", "Irvine", "Newark", "Toledo", "Durham", "Chula Vista",
        "Fort Wayne", "Jersey City", "St. Petersburg", "Laredo", "Madison", "Chandler",
        "Buffalo", "Lubbock", "Scottsdale", "Reno", "Glendale", "Gilbert", "Winston-Salem",
        "North Las Vegas", "Norfolk", "Chesapeake", "Irving", "Fremont", "Hialeah", "Garland",
        "Richmond", "Boise", "Baton Rouge", "Des Moines", "Spokane", "San Bernardino", "Modesto",
        "Tacoma", "Fontana", "Santa Clarita", "Birmingham", "Oxnard", "Fayetteville", "Rochester",
        "Moreno Valley", "Glendale", "Yonkers", "Huntington Beach", "Aurora", "Montgomery"
    ];

    private static readonly string[] States =
    [
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
        "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
        "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
        "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
        "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY"
    ];

    private static readonly string[] OrderStatuses = ["Pending", "Processing", "Shipped", "Delivered", "Cancelled", "Returned"];

    /// <summary>
    /// Seeds the database with 1000 customers, addresses, and order summaries.
    /// </summary>
    public static async Task SeedAsync(CustomerDbContext dbContext, ILogger logger)
    {
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Customers.AnyAsync())
        {
            logger.LogInformation("Database already contains customers. Skipping seed.");
            return;
        }

        var rng = new Random(42); // deterministic seed

        const int customerCount = 1000;
        var customers = new List<Customer>();
        var addresses = new List<Address>();
        var orderSummaries = new List<OrderSummary>();

        for (int i = 0; i < customerCount; i++)
        {
            var firstName = FirstNames[rng.Next(FirstNames.Length)];
            var lastName = LastNames[rng.Next(LastNames.Length)];
            var email = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}.{i + 1:0000}@example.com";
            var userId = rng.NextDouble() < 0.8 ? Guid.NewGuid() : (Guid?)null;
            var phoneNumber = $"{rng.Next(100, 999)}-{rng.Next(100, 999)}-{rng.Next(1000, 9999)}";

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                UserId = userId,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(0, 730))
            };
            customers.Add(customer);

            // Addresses (1-2 per customer)
            var addressCount = rng.Next(1, 3);
            for (int a = 0; a < addressCount; a++)
            {
                var streetNumber = rng.Next(100, 9999);
                var street = StreetNames[rng.Next(StreetNames.Length)];
                var city = Cities[rng.Next(Cities.Length)];
                var state = States[rng.Next(States.Length)];
                var postalCode = rng.Next(10000, 99999).ToString("D5");
                var country = "US";
                var type = a == 0 ? AddressType.Shipping : AddressType.Billing;
                var isDefault = a == 0;

                addresses.Add(new Address
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    Type = type,
                    Street = $"{streetNumber} {street}",
                    City = city,
                    State = state,
                    PostalCode = postalCode,
                    Country = country,
                    IsDefault = isDefault
                });
            }

            // Order summaries (0-3 per customer)
            var orderCount = rng.Next(0, 4);
            for (int o = 0; o < orderCount; o++)
            {
                var totalAmount = Math.Round((decimal)(rng.NextDouble() * 490 + 10), 2);
                var status = OrderStatuses[rng.Next(OrderStatuses.Length)];

                orderSummaries.Add(new OrderSummary
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    OrderId = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    TotalAmount = totalAmount,
                    Status = status,
                    CreatedAt = customer.CreatedAt.AddDays(rng.Next(1, 365))
                });
            }
        }

        await dbContext.Customers.AddRangeAsync(customers);
        await dbContext.Addresses.AddRangeAsync(addresses);
        await dbContext.OrderSummaries.AddRangeAsync(orderSummaries);
        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Seeded CustomerService with {CustomerCount} customers, {AddressCount} addresses, and {OrderCount} order summaries.",
            customers.Count, addresses.Count, orderSummaries.Count);
    }
}
