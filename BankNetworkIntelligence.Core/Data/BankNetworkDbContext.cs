using BankNetworkIntelligence.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BankNetworkIntelligence.Core.Data;

public class BankNetworkDbContext : DbContext
{
    public BankNetworkDbContext(
        DbContextOptions<BankNetworkDbContext> options)
        : base(options)
    {
    }

    public DbSet<Bank> Banks => Set<Bank>();

    public DbSet<Municipality> Municipalities => Set<Municipality>();

    public DbSet<HebicImport> HebicImports => Set<HebicImport>();

    public DbSet<BankLocation> BankLocations => Set<BankLocation>();

    public DbSet<LocationSnapshot> LocationSnapshots => Set<LocationSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureBanks(modelBuilder);
        ConfigureMunicipalities(modelBuilder);
        ConfigureImports(modelBuilder);
        ConfigureBankLocations(modelBuilder);
        ConfigureLocationSnapshots(modelBuilder);
    }

    private static void ConfigureBanks(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Bank>();

        entity.ToTable("banks");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        entity.HasIndex(x => x.Name)
            .IsUnique();
    }

    private static void ConfigureMunicipalities(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Municipality>();

        entity.ToTable("municipalities");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.Prefecture)
            .IsRequired()
            .HasMaxLength(200);

        entity.HasIndex(x => new
        {
            x.Name,
            x.Prefecture
        })
        .IsUnique();
    }

    private static void ConfigureImports(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<HebicImport>();

        entity.ToTable("hebic_imports");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Period)
            .IsRequired()
            .HasMaxLength(20);

        entity.Property(x => x.SourceFile)
            .IsRequired()
            .HasMaxLength(500);

        entity.Property(x => x.ImportedAt)
            .IsRequired();

        entity.Property(x => x.RecordCount)
            .IsRequired();

        entity.HasIndex(x => x.Period)
            .IsUnique();
    }

    private static void ConfigureBankLocations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankLocation>(entity =>
        {
            entity.ToTable("bank_locations");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.HebicCode)
                .HasMaxLength(20);

            entity.Property(x => x.LocationKey)
                .HasMaxLength(1000)
                .IsRequired();

            entity.HasIndex(x => x.LocationKey)
                .IsUnique();

            entity.HasOne(x => x.Bank)
                .WithMany(x => x.Locations)
                .HasForeignKey(x => x.BankId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureLocationSnapshots(
        ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<LocationSnapshot>();

        entity.ToTable("location_snapshots");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(300);

        entity.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(500);

        entity.Property(x => x.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        entity.Property(x => x.Phone)
            .HasMaxLength(100);

        entity.Property(x => x.Fax)
            .HasMaxLength(100);

        entity.HasIndex(x => new
        {
            x.LocationId,
            x.ImportId
        })
        .IsUnique();

        entity.HasIndex(x => x.MunicipalityId);

        entity.HasIndex(x => x.ImportId);

        entity.HasOne(x => x.Location)
            .WithMany(x => x.Snapshots)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Import)
            .WithMany(x => x.LocationSnapshots)
            .HasForeignKey(x => x.ImportId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Municipality)
            .WithMany(x => x.LocationSnapshots)
            .HasForeignKey(x => x.MunicipalityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}