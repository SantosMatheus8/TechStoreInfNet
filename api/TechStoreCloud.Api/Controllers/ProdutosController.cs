using Microsoft.AspNetCore.Mvc;
using TechStoreCloud.Api.DTOs;
using TechStoreCloud.Api.Services;

namespace TechStoreCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _service;

    public ProdutosController(IProdutoService service)
    {
        _service = service;
    }

    /// <summary>Lista todos os produtos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProdutoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTodos()
    {
        var produtos = await _service.ObterTodosAsync();
        return Ok(produtos);
    }

    /// <summary>Obtém um produto pelo ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var produto = await _service.ObterPorIdAsync(id);
        if (produto is null) return NotFound();
        return Ok(produto);
    }

    /// <summary>Cadastra um novo produto.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarProdutoDto dto)
    {
        var produto = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, produto);
    }

    /// <summary>Atualiza um produto existente.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarProdutoDto dto)
    {
        var produto = await _service.AtualizarAsync(id, dto);
        if (produto is null) return NotFound();
        return Ok(produto);
    }

    /// <summary>Exclui um produto.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var resultado = await _service.ExcluirAsync(id);
        if (!resultado) return NotFound();
        return NoContent();
    }
}
