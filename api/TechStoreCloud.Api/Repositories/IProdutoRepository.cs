using TechStoreCloud.Api.Models;

namespace TechStoreCloud.Api.Repositories;

public interface IProdutoRepository
{
    Task<IEnumerable<Produto>> ObterTodosAsync();
    Task<Produto?> ObterPorIdAsync(Guid id);
    Task<Produto> CriarAsync(Produto produto);
    Task<Produto> AtualizarAsync(Produto produto);
    Task<bool> ExcluirAsync(Guid id);
}
