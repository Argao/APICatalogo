using APICatalogo.Context;
using APICatalogo.DTO;
using APICatalogo.DTO.Mappings;
using APICatalogo.Filters;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using APICatalogo.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace APICatalogo.Controllers;

//[EnableCors("OrigensComAcessoPermitido")]
[Route("[controller]")]
[ApiController]
[EnableRateLimiting(("fixedwindow"))]
public class CategoriasController : ControllerBase
{
    
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(ILogger<CategoriasController> logger, IUnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    /// <summary>
    /// Retrieves a list of all categories as asynchronous operation.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an action result with a list of CategoriaDTO objects.</returns>
    //[Authorize]
    [HttpGet]
    [DisableRateLimiting]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetAsync()
    {
        var categorias = await _uow.CategoriaRepository.GetAllAsync();
        return Ok(categorias.ToCategoriaDTOList());
    }

    /// <summary>
    /// Retrieves a category by its identifier as an asynchronous operation.
    /// </summary>
    /// <param name="id">The unique identifier of the category to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an action result with the requested category as a <see cref="CategoriaDTO"/> object,
    /// or a not found result if the category does not exist.
    /// </returns>
    [DisableCors]
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


    /// <summary>
    /// Retrieves all categories asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.
    /// The task result contains an action result with an enumerable of CategoriaDTO objects.</returns>
    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetAsync([FromQuery] CategoriaParameters categoriaParameters)
    {
        var categorias = await _uow.CategoriaRepository.GetCategoriasAsync(categoriaParameters);
        return ObterCategorias(categorias);
    }


    /// <summary>
    /// Retrieves a paginated list of filtered categories based on name as an asynchronous operation.
    /// </summary>
    /// <param name="categoriaParameters">The filter parameters used to refine the category list.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an action result with a list of filtered CategoriaDTO objects.</returns>
    [HttpGet("filter/nome/pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategoriasFiltadasAsync([FromQuery] CategoriaFiltroNome categoriaParameters)
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

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="categoriaDto">The data transfer object containing the details of the category to be created.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an action result with the newly created category.</returns>
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
    [Authorize(Policy = "AdminOnly")]
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