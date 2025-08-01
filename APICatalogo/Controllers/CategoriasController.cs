using APICatalogo.Context;
using APICatalogo.DTO;
using APICatalogo.DTO.Mappings;
using APICatalogo.Filters;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace APICatalogo.Controllers;

[Route("[controller]")]
[ApiController]
public class CategoriasController : ControllerBase
{
    
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(ILogger<CategoriasController> logger, IUnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetAsync()
    {
        var categorias = await _uow.CategoriaRepository.GetAllAsync();
        
        return Ok(categorias.ToCategoriaDTOList());
    }

    [HttpGet("{id:int}", Name = "ObterCategoria")]
    public async Task<ActionResult<CategoriaDTO>> GetAsync(int id)
    {
        var categoria = await _uow.CategoriaRepository.GetAsync(c => c.CategoriaId == id);

        if (categoria is null)
        {
            _logger.LogWarning($"Categoria com id= {id} não encontrada...");
            return NotFound($"Categoria com id= {id} não encontrada...");
        }

        return Ok(categoria.ToCategoriaDto());
    }

    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetAsync([FromQuery] CategoriaParameters categoriaParameters)
    {
        var categorias = await _uow.CategoriaRepository.GetCategoriasAsync(categoriaParameters);
        return ObterCategorias(categorias);
    }

    [HttpGet("filter/nome/pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>>GetCategoriasFiltadasAsync([FromQuery] CategoriaFiltroNome categoriaParameters)
    {
        var categoriasFiltradas = await _uow.CategoriaRepository.GetCategoriasFiltroNomeAsync(categoriaParameters);
        return ObterCategorias(categoriasFiltradas);
    }

    private ActionResult<IEnumerable<CategoriaDTO>> ObterCategorias(PagedList<Categoria> categorias)
    {
        var metadata = new
        {
            categorias.TotalCount,
            categorias.PageSize,
            categorias.CurrentPage,
            categorias.TotalPages,
            categorias.HasNext,
            categorias.HasPrevious
        };
        
        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));
        var categoriasDto = categorias.ToCategoriaDTOList();
        
        return Ok(categoriasDto);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDTO>> PostAsync(CategoriaDTO categoriaDto)
    {
        if (categoriaDto is null)
        {
            _logger.LogWarning($"Dados inválidos...");
            return BadRequest("Dados inválidos");
        }
        
        var categoriaCriada = _uow.CategoriaRepository.Create(categoriaDto.ToCategoria());
        await _uow.CommitAsync();

        var novaCategoriaDto = categoriaCriada.ToCategoriaDto();
        
        return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDto.CategoriaId }, novaCategoriaDto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoriaDTO>> PutAsync(int id, CategoriaDTO categoriaDto)
    {
        if (id != categoriaDto.CategoriaId)
        {
            _logger.LogWarning($"Dados inválidos...");
            return BadRequest("Dados inválidos");
        }
        
        var categoriaAtualizada = _uow.CategoriaRepository.Update(categoriaDto.ToCategoria());
        await _uow.CommitAsync();

        return Ok(categoriaAtualizada.ToCategoriaDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<CategoriaDTO>> DeleteAsync(int id)
    {
        var categoria = await _uow.CategoriaRepository.GetAsync(c=>c.CategoriaId == id);

        if (categoria is null)
        {
            _logger.LogWarning($"Categoria com id={id} não encontrada...");
            return NotFound($"Categoria com id={id} não encontrada...");
        }

        var categoriaExcluida = _uow.CategoriaRepository.Delete(categoria);
        await _uow.CommitAsync();
        
        return Ok(categoriaExcluida.ToCategoriaDto());
    }
}