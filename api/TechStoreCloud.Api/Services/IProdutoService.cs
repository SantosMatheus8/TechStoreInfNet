using TechStoreCloud.Api.DTOs;

namespace TechStoreCloud.Api.Services;

public interface IProdutoService
{
    Task<IEnumerable<ProdutoDto>> ObterTodosAsync();
    Task<ProdutoDto?> ObterPorIdAsync(Guid id);
    Task<ProdutoDto> CriarAsync(CriarProdutoDto dto);
    Task<ProdutoDto?> AtualizarAsync(Guid id, AtualizarProdutoDto dto);
    Task<bool> ExcluirAsync(Guid id);
}
