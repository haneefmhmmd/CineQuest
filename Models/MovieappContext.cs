using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace _301301555_301287005_Laylay_Muhammad__Lab3.Models;

public partial class MovieappContext : DbContext
{
    public MovieappContext()
    {
    }

    public MovieappContext(DbContextOptions<MovieappContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=movie-app.c5ueo6sqo8m2.us-east-1.rds.amazonaws.com,1433;Database=movieapp;TrustServerCertificate=true;User ID=admin;Password=KlEmvY4EuPoaq5DDZ22l;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC2E4C0F7B");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4A2A802BC").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.FullName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Unknown");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
