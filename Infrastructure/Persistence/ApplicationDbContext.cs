using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets για κάθε entity
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Product ---
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Code)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(p => p.Description)
                      .HasMaxLength(500);
                entity.Property(p => p.Unit)
                      .IsRequired()
                      .HasMaxLength(20);
                entity.Property(p => p.Quantity)
                      .IsRequired();
                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();
                entity.Property(p => p.TotalValue)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                // Σχέση 1:Ν με Warehouse
                entity.HasOne(p => p.Warehouse)
                      .WithMany(w => w.Products)
                      .HasForeignKey(p => p.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Warehouse ---
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.ToTable("Warehouses");
                entity.HasKey(w => w.Id);

                entity.Property(w => w.Name)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(w => w.Address)
                      .IsRequired()
                      .HasMaxLength(200);
            });

            // --- Inventory ---
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable("Inventory");
                entity.HasKey(i => i.Id);

                entity.Property(i => i.ScanCode)
                      .HasMaxLength(100);
                entity.Property(i => i.Code)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(i => i.Action)
                      .IsRequired()
                      .HasMaxLength(10); // “Input” / “Output”
                entity.Property(i => i.Quantity)
                      .IsRequired();
                entity.Property(i => i.Timestamp)
                      .HasDefaultValueSql("GETUTCDATE()");

                // Σχέσεις με Warehouse & User
                entity.HasOne(i => i.Warehouse)
                      .WithMany(w => w.Inventories)
                      .HasForeignKey(i => i.WarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.User)
                      .WithMany(u => u.Inventories)
                      .HasForeignKey(i => i.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- User ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);

                entity.Property(u => u.FullName)
                      .IsRequired()
                      .HasMaxLength(150);
                entity.Property(u => u.Mobile)
                      .IsRequired()
                      .HasMaxLength(20);
                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(u => u.Username)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(u => u.Password)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(u => u.Role)
                      .IsRequired()
                      .HasMaxLength(50);
            });

            // --- Seed Warehouses ---
            modelBuilder.Entity<Warehouse>().HasData(
            new Warehouse { Id = 1, Name = "Κεντρική Αποθήκη", Address = "Λεωφ. Αθηνών 123" },
            new Warehouse { Id = 2, Name = "Υποκατάστημα Πειραιά", Address = "Οδός Θησέως 45" }
            );

            // --- Seed Products ---
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Code = "PRD001", Name = "Μπουκάλι Νερό", Description = "500ml", Unit = "pcs", Quantity = 100, Price = 0.50m, TotalValue = 50m, WarehouseId = 1 },
                new Product { Id = 2, Code = "PRD002", Name = "Χαρτί Α4", Description = "Pack 500", Unit = "pcs", Quantity = 20, Price = 5.00m, TotalValue = 100m, WarehouseId = 1 },
                new Product { Id = 3, Code = "PRD003", Name = "Μολύβι HB", Description = "Ξύλινο", Unit = "pcs", Quantity = 200, Price = 0.25m, TotalValue = 50m, WarehouseId = 2 }
            );

            // --- Seed Users ---
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FullName = "Διαχειριστής", Mobile = "6900000000", Email = "admin@company.com", Username = "admin", Password = "admin123", Role = "Admin" },
                new User { Id = 2, FullName = "Υπάλληλος", Mobile = "6999999999", Email = "clerk@company.com", Username = "clerk", Password = "clerk123", Role = "Clerk" }
            );

            // --- (προαιρετικά) Seed Inventory Records ---
            modelBuilder.Entity<Inventory>().HasData(
                new Inventory{ Id = 1,ScanCode = "SCN1001",Code = "PRD001", Action = "Input", WarehouseId = 1, UserId = 1,Timestamp = new DateTime(2025, 8, 1, 9, 0, 0, DateTimeKind.Utc) },
                new Inventory{ Id = 2, ScanCode = "SCN1002",Code = "PRD002", Action = "Input", WarehouseId = 1, UserId = 2,Timestamp = new DateTime(2025, 8, 1, 10, 0, 0, DateTimeKind.Utc) }
            );
 
        }
    }
}
