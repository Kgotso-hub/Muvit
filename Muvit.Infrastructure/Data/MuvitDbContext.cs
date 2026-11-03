using Microsoft.EntityFrameworkCore;
using Muvit.Domain.Entities;

namespace Muvit.Infrastructure.Data;

public class MuvitDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Payout> Payouts => Set<Payout>();
    public MuvitDbContext(DbContextOptions<MuvitDbContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasKey(p => p.Id);

            entity.ComplexProperty(p => p.Amount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("Amount").HasColumnType("decimal(18,2)");
                money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
            });
        });

        modelBuilder.Entity<Payout>(entity =>
        {
            entity.UsePropertyAccessMode(PropertyAccessMode.Field);
            
            entity.HasKey(p => p.Id);
            entity.Property(p => p.CommissionRate).HasColumnType("decimal(5,2)");

            entity.ComplexProperty(p => p.GrossAmount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("GrossAmount").HasColumnType("decimal(18,2)");
                money.Property(m => m.Currency).HasColumnName("GrossCurrency").HasMaxLength(3);
            });

            entity.ComplexProperty(p => p.CommissionAmount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("CommissionAmount").HasColumnType("decimal(18,2)");
                money.Property(m => m.Currency).HasColumnName("CommissionCurrency").HasMaxLength(3);
            });

            entity.ComplexProperty(p => p.NetPayoutAmount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("NetAmount").HasColumnType("decimal(18,2)");
                money.Property(m => m.Currency).HasColumnName("NetCurrency").HasMaxLength(3);
            });
        });

        modelBuilder.Entity<Booking>(entity =>
{
    entity.UsePropertyAccessMode(PropertyAccessMode.Field);
    entity.HasKey(b => b.Id);

    entity.ComplexProperty(b => b.PickupAddress, address =>
    {
        address.UsePropertyAccessMode(PropertyAccessMode.Field);
        address.Property(a => a.Line1).HasColumnName("PickupLine1").HasMaxLength(200);
        address.Property(a => a.Line2).HasColumnName("PickupLine2").HasMaxLength(200);
        address.Property(a => a.City).HasColumnName("PickupCity").HasMaxLength(100);
        address.Property(a => a.StateProvince).HasColumnName("PickupStateProvince").HasMaxLength(100);
        address.Property(a => a.PostalCode).HasColumnName("PickupPostalCode").HasMaxLength(20);
        address.Property(a => a.Country).HasColumnName("PickupCountry").HasMaxLength(100);

        address.ComplexProperty(a => a.Coordinate, coord =>
        {
            coord.Property(c => c.Latitude).HasColumnName("PickupLatitude").HasColumnType("decimal(9,6)");
            coord.Property(c => c.Longitude).HasColumnName("PickupLongitude").HasColumnType("decimal(9,6)");
        });
    });

    entity.ComplexProperty(b => b.DropoffAddress, address =>
    {
        address.UsePropertyAccessMode(PropertyAccessMode.Field);
        address.Property(a => a.Line1).HasColumnName("DropoffLine1").HasMaxLength(200);
        address.Property(a => a.Line2).HasColumnName("DropoffLine2").HasMaxLength(200);
        address.Property(a => a.City).HasColumnName("DropoffCity").HasMaxLength(100);
        address.Property(a => a.StateProvince).HasColumnName("DropoffStateProvince").HasMaxLength(100);
        address.Property(a => a.PostalCode).HasColumnName("DropoffPostalCode").HasMaxLength(20);
        address.Property(a => a.Country).HasColumnName("DropoffCountry").HasMaxLength(100);

        address.ComplexProperty(a => a.Coordinate, coord =>
        {
            coord.Property(c => c.Latitude).HasColumnName("DropoffLatitude").HasColumnType("decimal(9,6)");
            coord.Property(c => c.Longitude).HasColumnName("DropoffLongitude").HasColumnType("decimal(9,6)");
        });
    });

    entity.ComplexProperty(b => b.AgreedPrice, money =>
    {
        money.Property(m => m.Amount).HasColumnName("AgreedPriceAmount").HasColumnType("decimal(18,2)");
        money.Property(m => m.Currency).HasColumnName("AgreedPriceCurrency").HasMaxLength(3);
    });

    entity.OwnsMany(b => b.StatusHistory, history =>
    {
        history.UsePropertyAccessMode(PropertyAccessMode.Field);
        history.WithOwner().HasForeignKey("BookingId");
        history.HasKey(h => h.Id);
        history.Property(h => h.Note).HasMaxLength(500);
        history.ToTable("BookingStatusEvents");
    });

    entity.OwnsMany(b => b.PriceOffers, offers =>
    {
        offers.UsePropertyAccessMode(PropertyAccessMode.Field);
        offers.WithOwner().HasForeignKey("BookingId");
        offers.HasKey(o => o.Id);

        offers.OwnsOne(o => o.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount").HasColumnType("decimal(18,2)");
            money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
        });

        offers.ToTable("PriceOffers");
    });
});

        modelBuilder.Entity<User>(entity =>
        {
            entity.UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasKey(u => u.Id);
            entity.HasDiscriminator<string>("UserType")
                .HasValue<Customer>("Customer")
                .HasValue<Driver>("Driver")
                .HasValue<Admin>("Admin");
        });

        modelBuilder.Entity<Customer>();

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.ComplexProperty(d => d.Profile, profile =>
            {
                profile.UsePropertyAccessMode(PropertyAccessMode.Field);

                profile.ComplexProperty(p => p.Vehicle, vehicle =>
                {
                    vehicle.Property(v => v.VehicleType).HasColumnName("VehicleType").HasMaxLength(50);
                    vehicle.Property(v => v.PlateNumber).HasColumnName("PlateNumber").HasMaxLength(20);
                });
            });
        });
        modelBuilder.Entity<Admin>();

        base.OnModelCreating(modelBuilder);
    }
}