using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MonopolyDatabaseContext.DatabaseContext;

public partial class VpmContext : DbContext
{
    private static string? connectionString = "";
    public VpmContext()
    {
        var config = new ConfigurationBuilder()
             .AddJsonFile("appsettings.json")
             .Build();
        var appSettings = config.GetSection("ConnectionStrings")["DefaultConnection"];
        connectionString = appSettings;
    }

    public VpmContext(DbContextOptions<VpmContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Boxesdb> Boxesdbs { get; set; }

    public virtual DbSet<Palletsdb> Palletsdbs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(connectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Boxesdb>(entity =>
        {
            entity.HasKey(e => e.BoxId).HasName("boxes_pkey");

            entity.ToTable("boxesdb", "stock");

            entity.Property(e => e.BoxId).HasColumnName("box_id");
            entity.Property(e => e.BoxName)
                .HasMaxLength(100)
                .HasColumnName("box_name");
            entity.Property(e => e.Depth).HasColumnName("depth");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.PalletId).HasColumnName("pallet_id");
            entity.Property(e => e.ProductionDate).HasColumnName("production_date");
            entity.Property(e => e.Weight).HasColumnName("weight");
            entity.Property(e => e.Width).HasColumnName("width");
			entity.Property(e => e.Volume).HasColumnName("volume");

			entity.HasOne(d => d.Pallet).WithMany(p => p.Boxesdbs)
                .HasForeignKey(d => d.PalletId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("boxes_pallet_id_fkey");
        });

        modelBuilder.Entity<Palletsdb>(entity =>
        {
            entity.HasKey(e => e.PalletId).HasName("pallets_pkey");

            entity.ToTable("palletsdb", "stock");

            entity.Property(e => e.PalletId)
                .HasDefaultValueSql("nextval('stock.pallets_pallet_id_seq'::regclass)")
                .HasColumnName("pallet_id");
            entity.Property(e => e.Depth).HasColumnName("depth");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.PalletName).HasColumnName("pallet_name");
            entity.Property(e => e.Weight).HasColumnName("weight");
            entity.Property(e => e.Width).HasColumnName("width");
			entity.Property(e => e.Volume).HasColumnName("volume");
			entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
		});

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
