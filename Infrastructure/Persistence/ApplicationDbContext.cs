using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ProductBarcode> ProductBarcodes => Set<ProductBarcode>();
        public DbSet<Announcement> Announcements => Set<Announcement>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Announcement ---
            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.ToTable("Announcements");
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(a => a.Body)
                      .IsRequired();

                entity.Property(a => a.Date)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(a => a.PublisherFullName)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(a => a.IsPinned)
                      .HasDefaultValue(false);

                entity.Property(a => a.PinnedAt);

                entity.HasIndex(a => a.IsPinned);
                entity.HasIndex(a => a.Date);
                entity.HasIndex(a => a.Title);
            });

            // --- Product ---
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Code).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Description).HasMaxLength(500);
                entity.Property(p => p.Unit).IsRequired().HasMaxLength(20);
                entity.Property(p => p.Quantity).IsRequired();
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(p => p.TotalValue).HasColumnType("decimal(18,2)").IsRequired();

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
                entity.Property(w => w.Name).IsRequired().HasMaxLength(100);
                entity.Property(w => w.Address).IsRequired().HasMaxLength(200);
            });

            // --- Inventory ---
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable("Inventory");
                entity.HasKey(i => i.Id);

                entity.Property(i => i.ScanCode).HasMaxLength(100);
                entity.Property(i => i.Code).IsRequired().HasMaxLength(50);
                entity.Property(i => i.Action).IsRequired().HasMaxLength(10);
                entity.Property(i => i.Quantity).IsRequired();
                entity.Property(i => i.Timestamp).HasDefaultValueSql("GETUTCDATE()");

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

                entity.Property(u => u.FullName).IsRequired().HasMaxLength(150);
                entity.Property(u => u.Mobile).IsRequired().HasMaxLength(20);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(200);
                entity.Property(u => u.Role).IsRequired().HasMaxLength(50);

                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // --- Seed Warehouses ---
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { Id = 1, Name = "Κεντρική Αποθήκη", Address = "Λεωφ. Αθηνών 123" },
                new Warehouse { Id = 2, Name = "Υποκατάστημα Πειραιά", Address = "Οδός Θησέως 45" },
                new Warehouse { Id = 3, Name = "Υποκατάστημα Θεσσαλονίκης", Address = "Λεωφ. Νίκης 78" },
                new Warehouse { Id = 4, Name = "Υποκατάστημα Πατρών", Address = "Οδός Κορίνθου 12" },
                new Warehouse { Id = 5, Name = "Υποκατάστημα Λάρισας", Address = "Οδός Βόλου 56" }
            );

            // --- Seed Products ---
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Code = "PRD001", Name = "Laptop Dell Latitude 5540", Description = "15.6 i5 / 16GB RAM", Unit = "pcs", Quantity = 10, Price = 950.00m, TotalValue = 9500.00m, WarehouseId = 1 },
                new Product { Id = 2, Code = "PRD002", Name = "Οθόνη Samsung 24", Description = "IPS Full HD", Unit = "pcs", Quantity = 15, Price = 180.00m, TotalValue = 2700.00m, WarehouseId = 3 },
                new Product { Id = 3, Code = "PRD003", Name = "Πληκτρολόγιο Logitech K120", Description = "USB Ελληνικό", Unit = "pcs", Quantity = 50, Price = 12.00m, TotalValue = 600.00m, WarehouseId = 2 },
                new Product { Id = 4, Code = "PRD004", Name = "Ποντίκι Logitech B100", Description = "USB Οπτικό", Unit = "pcs", Quantity = 60, Price = 8.50m, TotalValue = 510.00m, WarehouseId = 5 },
                new Product { Id = 5, Code = "PRD005", Name = "SSD Samsung 1TB", Description = "NVMe M.2", Unit = "pcs", Quantity = 12, Price = 95.00m, TotalValue = 1140.00m, WarehouseId = 4 }
            );

            // --- RefreshToken ---
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Token).IsRequired().HasMaxLength(200);
                entity.HasIndex(r => r.Token).IsUnique();
                entity.Property(r => r.Created).IsRequired();
                entity.Property(r => r.Expires).IsRequired();

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- ProductBarcode ---
            modelBuilder.Entity<ProductBarcode>()
                .HasIndex(x => x.Code).IsUnique();

            modelBuilder.Entity<ProductBarcode>()
                .HasOne(x => x.Product)
                .WithMany(p => p.Barcodes)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Seed Announcement ---
            modelBuilder.Entity<Announcement>().HasData(
                new Announcement
                {
                    Id = 1,
                    Title = "Καλωσήρθατε στην πλατφόρμα",
                    Body = "Από σήμερα οι ενημερώσεις θα εμφανίζονται εδώ.",
                    Date = new DateTime(2025, 9, 1, 8, 0, 0, DateTimeKind.Utc),
                    PublisherFullName = "Admin User"
                });
        }
    }
}
