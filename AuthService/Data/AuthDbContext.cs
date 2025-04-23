using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options) { }

    public DbSet<Auth> Auth { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auth>(entity =>
        {
            entity.ToTable("Usuario");
            entity.HasKey(a => a.Correo);                
            entity.Property(a => a.Correo)
                .HasColumnName("usuario_correo");
            entity.Property(a => a.Contrasena)
                .HasColumnName("usuario_contrasena");
        });
    }
}
