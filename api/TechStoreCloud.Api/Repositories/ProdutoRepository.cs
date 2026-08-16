using Microsoft.EntityFrameworkCore;
using TechStoreCloud.Api.Data;
using TechStoreCloud.Api.Models;

namespace TechStoreCloud.Api.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly AppDbContext _context;

    public ProdutoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Produto>> ObterTodosAsync()
    {
        return await _context.Produtos
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();
    }

    public async Task<Produto?> ObterPorIdAsync(Guid id)
    {
        return await _context.Produtos.FindAsync(id);
    }

    public async Task<Produto> CriarAsync(Produto produto)
    {
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();
        return produto;
    }

    public async Task<Produto> AtualizarAsync(Produto produto)
    {
        _context.Produtos.Update(produto);
        await _context.SaveChangesAsync();
        return produto;
    }

    public async Task<bool> ExcluirAsync(Guid id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto is null) return false;

        _context.Produtos.Remove(produto);
        await _context.SaveChangesAsync();
        return true;
    }
}
