using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Domain.Entities;
using QuickBasket.Domain.Enums;

namespace QuickBasket.Infrastructure.Persistence;

/// <summary>
/// Development bootstrap. Creates the schema (until EF migrations exist), then seeds an admin, a delivery partner and a
/// small sample catalogue. Credentials come from configuration (Seed:*) - nothing is hard-coded here.
/// </summary>
public static class DbSeeder
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<QuickBasketDbContext>();
        var log = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        if (config.GetValue<bool>("Database:ApplyMigrations")) await db.Database.MigrateAsync(ct);
        else if (config.GetValue<bool>("Database:EnsureCreated")) await db.Database.EnsureCreatedAsync(ct);

        var hasher = sp.GetRequiredService<IPasswordHasher>();
        await SeedUserAsync(db, hasher, config["Seed:AdminEmail"], config["Seed:AdminPassword"], "QuickBasket Admin", UserRole.Admin, ct);
        await SeedUserAsync(db, hasher, config["Seed:PartnerEmail"], config["Seed:PartnerPassword"], "Demo Delivery Partner", UserRole.DeliveryPartner, ct);

        if (config.GetValue<bool>("Seed:SampleCatalog") && !await db.Categories.AnyAsync(ct))
        {
            SeedCatalog(db);
            await db.SaveChangesAsync(ct);
            log.LogInformation("Seeded sample catalogue");
        }
    }

    private static async Task SeedUserAsync(QuickBasketDbContext db, IPasswordHasher hasher, string? email, string? password,
        string name, UserRole role, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        email = email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email, ct)) return;
        db.Users.Add(new User { Email = email, PasswordHash = hasher.Hash(password), FullName = name, Role = role, IsAvailable = role == UserRole.DeliveryPartner });
        await db.SaveChangesAsync(ct);
    }

    private static void SeedCatalog(QuickBasketDbContext db)
    {
        (string Name, (string Name, decimal Price, decimal Mrp, string Unit)[] Items)[] data =
        [
            ("Fruits & Vegetables", [("Bananas", 49m, 55m, "1 dozen"), ("Tomatoes", 32m, 40m, "500 g"), ("Onions", 38m, 45m, "1 kg"), ("Spinach", 25m, 30m, "250 g")]),
            ("Dairy & Eggs", [("Toned Milk", 28m, 30m, "500 ml"), ("Curd", 35m, 38m, "400 g"), ("Eggs", 84m, 90m, "12 pcs"), ("Paneer", 95m, 105m, "200 g")]),
            ("Staples", [("Basmati Rice", 180m, 210m, "1 kg"), ("Whole Wheat Atta", 255m, 280m, "5 kg"), ("Toor Dal", 160m, 175m, "1 kg"), ("Sunflower Oil", 145m, 160m, "1 L")]),
            ("Snacks & Beverages", [("Masala Chips", 20m, 20m, "52 g"), ("Instant Coffee", 175m, 190m, "100 g"), ("Green Tea", 140m, 160m, "25 bags"), ("Orange Juice", 99m, 110m, "1 L")]),
            ("Household Essentials", [("Dish Wash Liquid", 109m, 125m, "500 ml"), ("Laundry Detergent", 215m, 240m, "1 kg"), ("Toilet Paper", 120m, 140m, "4 rolls"), ("Hand Wash", 89m, 99m, "250 ml")])
        ];

        var order = 0;
        foreach (var (catName, items) in data)
        {
            var category = new Category { Name = catName, Slug = Slug(catName), SortOrder = order++ };
            db.Categories.Add(category);
            foreach (var (name, price, mrp, unit) in items)
                db.Products.Add(new Product { Category = category, Name = name, Slug = Slug(name), Price = price, MrpPrice = mrp, Unit = unit, StockQuantity = 100 });
        }
    }

    private static string Slug(string s) => string.Join('-', new string(s.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray())
        .Split('-', StringSplitOptions.RemoveEmptyEntries));
}
