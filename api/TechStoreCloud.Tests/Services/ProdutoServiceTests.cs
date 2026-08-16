using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TechStoreCloud.Api.DTOs;
using TechStoreCloud.Api.Models;
using TechStoreCloud.Api.Repositories;
using TechStoreCloud.Api.Services;
using Xunit;

namespace TechStoreCloud.Tests.Services;

public class ProdutoServiceTests
{
    private readonly Mock<IProdutoRepository> _repositoryMock;
    private readonly Mock<ILogger<ProdutoService>> _loggerMock;
    private readonly ProdutoService _service;

    public ProdutoServiceTests()
    {
        _repositoryMock = new Mock<IProdutoRepository>();
        _loggerMock = new Mock<ILogger<ProdutoService>>();
        _service = new ProdutoService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ObterTodos_DeveRetornarListaDeProdutos()
    {
        var produtos = new List<Produto>
        {
            CriarProdutoExemplo("Notebook", 3500m),
            CriarProdutoExemplo("Mouse", 89.90m)
        };
        _repositoryMock.Setup(r => r.ObterTodosAsync()).ReturnsAsync(produtos);

        var resultado = await _service.ObterTodosAsync();

        resultado.Should().HaveCount(2);
        resultado.First().Nome.Should().Be("Notebook");
    }

    [Fact]
    public async Task ObterPorId_QuandoExiste_DeveRetornarProduto()
    {
        var id = Guid.NewGuid();
        var produto = CriarProdutoExemplo("Teclado", 250m, id);
        _repositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(produto);

        var resultado = await _service.ObterPorIdAsync(id);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("Teclado");
        resultado.Preco.Should().Be(250m);
    }

    [Fact]
    public async Task ObterPorId_QuandoNaoExiste_DeveRetornarNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Produto?)null);

        var resultado = await _service.ObterPorIdAsync(id);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Criar_ComDadosValidos_DeveCriarProduto()
    {
        var dto = new CriarProdutoDto("Monitor 4K", "Monitor UHD 27 polegadas", 2200m, "Monitores", 15, null);
        _repositoryMock.Setup(r => r.CriarAsync(It.IsAny<Produto>()))
            .ReturnsAsync((Produto p) => { p.Id = Guid.NewGuid(); return p; });

        var resultado = await _service.CriarAsync(dto);

        resultado.Nome.Should().Be("Monitor 4K");
        resultado.Preco.Should().Be(2200m);
        resultado.QuantidadeEstoque.Should().Be(15);
        _repositoryMock.Verify(r => r.CriarAsync(It.IsAny<Produto>()), Times.Once);
    }

    [Fact]
    public async Task Criar_ComNomeVazio_DeveLancarValidationException()
    {
        var dto = new CriarProdutoDto("", "Desc", 100m, "Cat", 10, null);

        var act = () => _service.CriarAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Nome é obrigatório*");
    }

    [Fact]
    public async Task Criar_ComPrecoNegativo_DeveLancarValidationException()
    {
        var dto = new CriarProdutoDto("Produto", "Desc", -10m, "Cat", 10, null);

        var act = () => _service.CriarAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Preço não pode ser negativo*");
    }

    [Fact]
    public async Task Criar_ComQuantidadeNegativa_DeveLancarValidationException()
    {
        var dto = new CriarProdutoDto("Produto", "Desc", 100m, "Cat", -5, null);

        var act = () => _service.CriarAsync(dto);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Quantidade em estoque não pode ser negativa*");
    }

    [Fact]
    public async Task Atualizar_QuandoExiste_DeveAtualizarProduto()
    {
        var id = Guid.NewGuid();
        var produto = CriarProdutoExemplo("Antigo", 100m, id);
        _repositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(produto);
        _repositoryMock.Setup(r => r.AtualizarAsync(It.IsAny<Produto>()))
            .ReturnsAsync((Produto p) => p);

        var dto = new AtualizarProdutoDto("Novo Nome", "Nova desc", 200m, "Nova Cat", 20, null, true);
        var resultado = await _service.AtualizarAsync(id, dto);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("Novo Nome");
        resultado.Preco.Should().Be(200m);
    }

    [Fact]
    public async Task Atualizar_QuandoNaoExiste_DeveRetornarNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Produto?)null);

        var dto = new AtualizarProdutoDto("Nome", "Desc", 100m, "Cat", 10, null, true);
        var resultado = await _service.AtualizarAsync(id, dto);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Excluir_QuandoExiste_DeveRetornarTrue()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ExcluirAsync(id)).ReturnsAsync(true);

        var resultado = await _service.ExcluirAsync(id);

        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task Excluir_QuandoNaoExiste_DeveRetornarFalse()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.ExcluirAsync(id)).ReturnsAsync(false);

        var resultado = await _service.ExcluirAsync(id);

        resultado.Should().BeFalse();
    }

    private static Produto CriarProdutoExemplo(string nome, decimal preco, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Nome = nome,
        Descricao = $"Descrição do {nome}",
        Preco = preco,
        Categoria = "Eletrônicos",
        QuantidadeEstoque = 10,
        Ativo = true,
        CriadoEm = DateTime.UtcNow,
        AtualizadoEm = DateTime.UtcNow
    };
}
