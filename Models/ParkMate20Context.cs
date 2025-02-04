using Microsoft.EntityFrameworkCore;

namespace ParkMate2._0.Models;

public partial class ParkMate20Context : DbContext
{
    public ParkMate20Context()
    {
    }

    public ParkMate20Context(DbContextOptions<ParkMate20Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Car> Cars { get; set; }

    public virtual DbSet<Parking> Parkings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserCar> UserCars { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=ParkMate2.0;Trusted_Connection=True;TrustServerCertificate=True;");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.CarId).HasName("PK__Cars__68A0340E0152E66D");

            entity.HasIndex(e => e.LicensePlate, "UQ__Cars__026BC15CD797100B").IsUnique();

            entity.Property(e => e.CarId).HasColumnName("CarID");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Model)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Cars)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Cars__UserID__3B75D760");
        });

        modelBuilder.Entity<Parking>(entity =>
        {
            entity.HasKey(e => e.ParkingId).HasName("PK__Parking__43E7B6314A13BDFA");

            entity.ToTable("Parking");

            entity.Property(e => e.ParkingId).HasColumnName("ParkingID");
            entity.Property(e => e.CarId).HasColumnName("CarID");
            entity.Property(e => e.Duration).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PayMethod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Car).WithMany(p => p.Parkings)
                .HasForeignKey(d => d.CarId)
                .HasConstraintName("FK__Parking__CarID__46E78A0C");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACF63CCDE8");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534DDEF515F").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserCar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserCar__3214EC2744EA247E");

            entity.ToTable("UserCar");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CarId).HasColumnName("CarID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Car).WithMany(p => p.UserCars)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserCar__CarID__4316F928");

            entity.HasOne(d => d.User).WithMany(p => p.UserCars)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserCar__UserID__4222D4EF");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
