using Ocow.Inventory.Migrations.Factories;

var factory = new InventoryDbContextFactory();
using var dbContext = factory.CreateDbContext(args);

Console.WriteLine($"Inventory DbContext ready: {dbContext.GetType().Name}");
