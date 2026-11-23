using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PediatriTakip.Models;

namespace PediatriTakip.Data;

public partial class PediatriContext : DbContext
{
    public PediatriContext(DbContextOptions<PediatriContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AsiKayitlari> AsiKayitlaris { get; set; }

    public virtual DbSet<Asilar> Asilars { get; set; }

    public virtual DbSet<Hastalar> Hastalars { get; set; }

    public virtual DbSet<Kullanicilar> Kullanicilars { get; set; }

    public virtual DbSet<Muayeneler> Muayenelers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AsiKayitlari>(entity =>
        {
            entity.ToTable("AsiKayitlari");

            entity.HasIndex(e => e.HastaId, "IX_AsiKayitlari_HastaId");

            entity.HasOne(d => d.Asi).WithMany(p => p.AsiKayitlaris)
                .HasForeignKey(d => d.AsiId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Hasta).WithMany(p => p.AsiKayitlaris).HasForeignKey(d => d.HastaId);
        });

        modelBuilder.Entity<Asilar>(entity =>
        {
            entity.ToTable("Asilar");
        });

        modelBuilder.Entity<Hastalar>(entity =>
        {
            entity.ToTable("Hastalar");

            entity.HasIndex(e => e.TcKimlikNo, "IX_Hastalar_TcKimlikNo").IsUnique();

            entity.HasIndex(e => e.DoktorId, "IX_Hastalar_DoktorId");

            entity.HasIndex(e => e.VeliId, "IX_Hastalar_VeliId");

            entity.Property(e => e.KayitTarihi).HasDefaultValueSql("datetime('now','localtime')");

            entity.HasOne(d => d.Doktor).WithMany(p => p.HastalarDoktors)
                .HasForeignKey(d => d.DoktorId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Veli).WithMany(p => p.HastalarVelis)
                .HasForeignKey(d => d.VeliId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Kullanicilar>(entity =>
        {
            entity.ToTable("Kullanicilar");

            entity.HasIndex(e => e.Email, "IX_Kullanicilar_Email").IsUnique();

            entity.Property(e => e.OlusturmaTarihi).HasDefaultValueSql("datetime('now','localtime')");
        });

        modelBuilder.Entity<Muayeneler>(entity =>
        {
            entity.ToTable("Muayeneler");

            entity.HasIndex(e => e.HastaId, "IX_Muayeneler_HastaId");

            entity.HasOne(d => d.Doktor).WithMany(p => p.Muayenelers)
                .HasForeignKey(d => d.DoktorId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Hasta).WithMany(p => p.Muayenelers).HasForeignKey(d => d.HastaId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
