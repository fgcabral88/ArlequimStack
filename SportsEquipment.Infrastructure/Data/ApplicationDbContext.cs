using Microsoft.EntityFrameworkCore;
using SportsEquipment.Domain.Entities;

namespace SportsEquipment.Infrastructure.Data
{
    /// <summary>
    /// DbContext da aplicação: contém mapeamentos Fluent API para as entidades do domínio.
    /// Ajustado para mapear corretamente coleções com backing fields e tipos owned (Money).
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductStock> ProductStocks { get; set; } = null!;
        public DbSet<StockEntry> StockEntries { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasKey(u => u.Id);
                b.Property(u => u.Name).IsRequired().HasMaxLength(200);
                b.Property(u => u.Email).IsRequired().HasMaxLength(200);
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(200);
                b.Property<DateTime>("CreatedAt").HasColumnName("CreatedAt");
                b.Property<DateTime>("UpdatedAt").HasColumnName("UpdatedAt");
            });

            // Products
            modelBuilder.Entity<Product>(b =>
            {
                b.ToTable("Products");
                b.HasKey(p => p.Id);
                b.Property(p => p.Name).IsRequired().HasMaxLength(250);
                b.Property(p => p.Description).HasMaxLength(2000);
                b.Property(p => p.IsActive).IsRequired();
                b.Property<DateTime>("CreatedAt").HasColumnName("CreatedAt");
                b.Property<DateTime>("UpdatedAt").HasColumnName("UpdatedAt");

                // Money as owned type
                b.OwnsOne(p => p.Price, m =>
                {
                    m.Property(md => md.Amount).HasColumnName("PriceAmount").HasColumnType("decimal(18,2)").IsRequired();
                    m.Property(md => md.Currency).HasColumnName("PriceCurrency").HasMaxLength(10).IsRequired();
                });
            });

            // ProductStock
            modelBuilder.Entity<ProductStock>(b =>
            {
                b.ToTable("ProductStocks");
                b.HasKey(ps => ps.Id);
                b.Property(ps => ps.ProductId).IsRequired();
                b.HasIndex(ps => ps.ProductId).IsUnique(); // 1:1 Product -> ProductStock by ProductId
                b.Property<int>("CurrentQuantity").HasColumnName("CurrentQuantity");
                b.Property<DateTime>("CreatedAt").HasColumnName("CreatedAt");
                b.Property<DateTime>("UpdatedAt").HasColumnName("UpdatedAt");

                // relation to Product (optional): ensure Product exists
                b.HasOne<Product>().WithOne().HasForeignKey<ProductStock>("ProductId").HasPrincipalKey<Product>(p => p.Id);
            });

            // StockEntry
            modelBuilder.Entity<StockEntry>(b =>
            {
                b.ToTable("StockEntries");
                b.HasKey(se => se.Id);
                b.Property(se => se.ProductId).IsRequired();
                b.Property(se => se.Quantity).IsRequired();
                b.Property(se => se.FiscalNoteNumber).IsRequired().HasMaxLength(200);
                b.Property(se => se.RegisteredAt).IsRequired();

                // FK: StockEntry.ProductId -> ProductStock.ProductId (principal key)
                b.HasOne<ProductStock>()
                    .WithMany(nameof(ProductStock.Entries))
                    .HasForeignKey(se => se.ProductId)
                    .HasPrincipalKey(ps => ps.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Orders
            modelBuilder.Entity<Order>(b =>
            {
                b.ToTable("Orders");
                b.HasKey(o => o.Id);
                b.Property(o => o.ClientDocument).IsRequired().HasMaxLength(200);
                b.Property(o => o.SellerName).IsRequired().HasMaxLength(200);
                b.Property(o => o.Status).IsRequired();
                b.Property<DateTime>("CreatedAt").HasColumnName("CreatedAt");
                b.Property<DateTime>("UpdatedAt").HasColumnName("UpdatedAt");

                // Use the private backing field for the Items collection
                b.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

                // Map items as a separate table (OrderItems)
                b.HasMany(o => o.Items)
                    .WithOne()
                    .HasForeignKey("OrderId")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // OrderItem mapping (entity type)
            modelBuilder.Entity<OrderItem>(b =>
            {
                b.ToTable("OrderItems");
                // Composite key: OrderId (shadow FK) + ProductId
                b.HasKey("OrderId", nameof(OrderItem.ProductId));
                b.Property<Guid>("OrderId").IsRequired();
                b.Property(oi => oi.ProductId).IsRequired();
                b.Property(oi => oi.Quantity).IsRequired();

                // Map Money fields for UnitPrice (owned)
                b.OwnsOne(oi => oi.UnitPrice, oiOwned =>
                {
                    oiOwned.Property(p => p.Amount).HasColumnName("UnitPriceAmount").HasColumnType("decimal(18,2)").IsRequired();
                    oiOwned.Property(p => p.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(10).IsRequired();
                });
            });
        }
    }
}
