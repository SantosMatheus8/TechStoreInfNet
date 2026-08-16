using Microsoft.EntityFrameworkCore;
using TechStoreCloud.Api.Models;

namespace TechStoreCloud.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("produtos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(2000);
            entity.Property(e => e.Preco).HasPrecision(18, 2);
            entity.Property(e => e.Categoria).IsRequired().HasMaxLength(100);
            entity.Property(e => e.QuantidadeEstoque).HasDefaultValue(0);
            entity.Property(e => e.ImagemUrl).HasMaxLength(500);
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.CriadoEm).HasDefaultValueSql("NOW()");
            entity.Property(e => e.AtualizadoEm).HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Categoria);
            entity.HasIndex(e => e.Ativo);
        });
    }
}
