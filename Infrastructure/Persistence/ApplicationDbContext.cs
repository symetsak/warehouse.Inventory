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
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(i => i.Code)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(i => i.Action)
                      .IsRequired()
                      .HasMaxLength(10); // “Input” / “Output”
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
        }
    }
}
