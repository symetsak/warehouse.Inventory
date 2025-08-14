using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db, IConfiguration cfg)
        {
            await db.Database.MigrateAsync();

            if (!await db.Users.AnyAsync())
            {
                var adminPass = cfg["Seed:AdminPassword"] ?? "Admin123!";
                var employeePass = cfg["Seed:EmployeePassword"] ?? "Employee123!";

                var admin = new Core.Entities.User
                {
                    Username = "admin",
                    Email = "admin@company.com",
                    FullName = "Διαχειριστής",
                    Mobile = "6900000000",
                    Role = "Admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPass, workFactor: 11)
                };
                var clerk = new Core.Entities.User
                {
                    Username = "clerk",
                    Email = "clerk@company.com",
                    FullName = "Υπάλληλος",
                    Mobile = "6999999999",
                    Role = "Employee",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(employeePass, workFactor: 11)
                };

                db.Users.AddRange(admin, clerk);
                await db.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("[Seeder] Users exist, skip");
            }

            if (!await db.Inventories.AnyAsync())
            {
                var adminId = await db.Users.Where(u => u.Username == "admin")
                                            .Select(u => u.Id).SingleAsync();
                var clerkId = await db.Users.Where(u => u.Username == "clerk")
                                            .Select(u => u.Id).SingleAsync();

                db.Inventories.AddRange(
                    new Core.Entities.Inventory
                    {
                        ScanCode = "SCN1001",
                        Code = "PRD001",
                        Action = "Input",
                        Quantity = 0,
                        Timestamp = DateTime.UtcNow,
                        WarehouseId = 1,
                        UserId = adminId
                    },
                    new Core.Entities.Inventory
                    {
                        ScanCode = "SCN1002",
                        Code = "PRD002",
                        Action = "Input",
                        Quantity = 0,
                        Timestamp = DateTime.UtcNow,
                        WarehouseId = 1,
                        UserId = clerkId
                    }
                );
                await db.SaveChangesAsync();
                Console.WriteLine("[Seeder] Inventory seeded");
            }

            Console.WriteLine("[Seeder] Done");
        }
    }
}
