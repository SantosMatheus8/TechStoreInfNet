namespace TechStoreCloud.Api.DTOs;

public record ProdutoDto(
    Guid Id,
    string Nome,
    string Descricao,
    decimal Preco,
    string Categoria,
    int QuantidadeEstoque,
    string? ImagemUrl,
    bool Ativo,
    DateTime CriadoEm,
    DateTime AtualizadoEm
);

public record CriarProdutoDto(
    string Nome,
    string Descricao,
    decimal Preco,
    string Categoria,
    int QuantidadeEstoque,
    string? ImagemUrl
);

public record AtualizarProdutoDto(
    string Nome,
    string Descricao,
    decimal Preco,
    string Categoria,
    int QuantidadeEstoque,
    string? ImagemUrl,
    bool Ativo
);
