using TechStoreCloud.Api.DTOs;
using TechStoreCloud.Api.Models;
using TechStoreCloud.Api.Repositories;

namespace TechStoreCloud.Api.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _repository;
    private readonly ILogger<ProdutoService> _logger;

    public ProdutoService(IProdutoRepository repository, ILogger<ProdutoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<ProdutoDto>> ObterTodosAsync()
    {
        var produtos = await _repository.ObterTodosAsync();
        _logger.LogInformation("Listando {Count} produtos", produtos.Count());
        return produtos.Select(MapToDto);
    }

    public async Task<ProdutoDto?> ObterPorIdAsync(Guid id)
    {
        var produto = await _repository.ObterPorIdAsync(id);
        if (produto is null)
        {
            _logger.LogWarning("Produto {Id} não encontrado", id);
            return null;
        }
        return MapToDto(produto);
    }

    public async Task<ProdutoDto> CriarAsync(CriarProdutoDto dto)
    {
        ValidarProduto(dto.Nome, dto.Preco, dto.QuantidadeEstoque);

        var produto = new Produto
        {
            Nome = dto.Nome.Trim(),
            Descricao = dto.Descricao?.Trim() ?? string.Empty,
            Preco = dto.Preco,
            Categoria = dto.Categoria.Trim(),
            QuantidadeEstoque = dto.QuantidadeEstoque,
            ImagemUrl = dto.ImagemUrl?.Trim(),
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        };

        var criado = await _repository.CriarAsync(produto);
        _logger.LogInformation("Produto criado: {Id} - {Nome}", criado.Id, criado.Nome);
        return MapToDto(criado);
    }

    public async Task<ProdutoDto?> AtualizarAsync(Guid id, AtualizarProdutoDto dto)
    {
        var produto = await _repository.ObterPorIdAsync(id);
        if (produto is null)
        {
            _logger.LogWarning("Tentativa de atualizar produto inexistente: {Id}", id);
            return null;
        }

        ValidarProduto(dto.Nome, dto.Preco, dto.QuantidadeEstoque);

        produto.Nome = dto.Nome.Trim();
        produto.Descricao = dto.Descricao?.Trim() ?? string.Empty;
        produto.Preco = dto.Preco;
        produto.Categoria = dto.Categoria.Trim();
        produto.QuantidadeEstoque = dto.QuantidadeEstoque;
        produto.ImagemUrl = dto.ImagemUrl?.Trim();
        produto.Ativo = dto.Ativo;
        produto.AtualizadoEm = DateTime.UtcNow;

        var atualizado = await _repository.AtualizarAsync(produto);
        _logger.LogInformation("Produto atualizado: {Id} - {Nome}", atualizado.Id, atualizado.Nome);
        return MapToDto(atualizado);
    }

    public async Task<bool> ExcluirAsync(Guid id)
    {
        var resultado = await _repository.ExcluirAsync(id);
        if (resultado)
            _logger.LogInformation("Produto excluído: {Id}", id);
        else
            _logger.LogWarning("Tentativa de excluir produto inexistente: {Id}", id);
        return resultado;
    }

    private static void ValidarProduto(string nome, decimal preco, int quantidade)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(nome))
            erros.Add("Nome é obrigatório.");
        if (nome?.Length > 200)
            erros.Add("Nome deve ter no máximo 200 caracteres.");
        if (preco < 0)
            erros.Add("Preço não pode ser negativo.");
        if (quantidade < 0)
            erros.Add("Quantidade em estoque não pode ser negativa.");

        if (erros.Count > 0)
            throw new ValidationException(erros);
    }

    private static ProdutoDto MapToDto(Produto p) => new(
        p.Id, p.Nome, p.Descricao, p.Preco, p.Categoria,
        p.QuantidadeEstoque, p.ImagemUrl, p.Ativo, p.CriadoEm, p.AtualizadoEm
    );
}

public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors) : base(string.Join(" ", errors))
    {
        Errors = errors.ToList().AsReadOnly();
    }
}
