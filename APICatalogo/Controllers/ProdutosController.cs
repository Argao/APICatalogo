using APICatalogo.Context;
using APICatalogo.DTO;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace APICatalogo.Controllers;

[Route("[controller]")]
[ApiController]
public class ProdutosController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    private readonly IMapper _mapper;

    public ProdutosController(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetAsync()
    {
        var produtos = await _uow.ProdutoRepository.GetAllAsync();

        if (produtos.Any())
        {
            return NotFound();
        }

        return Ok(_mapper.Map<IEnumerable<ProdutoDTO>>(produtos));
    }

    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetAsync([FromQuery] ProdutosParameters produtosParameters)
    {
        var produtos = await _uow.ProdutoRepository.GetProdutosAsync(produtosParameters);

        return ObterProdutos(produtos);
    }
    

    [HttpGet("filter/preco/pagination")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosFilterPrecoAsync([FromQuery] ProdutosFiltroPreco produtosFiltroParameters)
    {
        var produtos = await _uow.ProdutoRepository.GetProdutosFiltroPrecoAsync(produtosFiltroParameters);
        return ObterProdutos(produtos);
    }

    private ActionResult<IEnumerable<ProdutoDTO>> ObterProdutos(PagedList<Produto> produtos)
    {
        var metadata = new
        {
            produtos.TotalCount,
            produtos.PageSize,
            produtos.CurrentPage,
            produtos.TotalPages,
            produtos.HasNext,
            produtos.HasPrevious
        };
        
        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        var produtosDTO = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDTO);
    }
    
    [HttpGet("{id}", Name = "ObterProduto")]
    public async Task<ActionResult<ProdutoDTO>> GetAsync(int id)
    {
        var produto = await _uow.ProdutoRepository.GetAsync(c => c.ProdutoId == id);
        if (produto is null)
        {
            return NotFound("Produto não encontrado...");
        }

        return Ok(_mapper.Map<ProdutoDTO>(produto));
    }

    [HttpGet("produtos/{id}")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosCategoriaAsync(int id)
    {
        var produtos = await _uow.ProdutoRepository.GetProdutosPorCategoriaAsync(id);

        return (!produtos.Any())
            ? NotFound("Nenhum produto com essa categoria")
            : Ok(_mapper.Map<IEnumerable<ProdutoDTO>>(produtos));
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoDTO>> PostAsync(ProdutoDTO produtoDto)
    {
        var novoProduto = _uow.ProdutoRepository.Create(_mapper.Map<Produto>(produtoDto));
        await _uow.CommitAsync();

        var novoProdutoDTO = _mapper.Map<ProdutoDTO>(novoProduto);

        return new CreatedAtRouteResult("ObterProduto",
            new { id = novoProdutoDTO.ProdutoId },
            novoProdutoDTO
        );
    }

    [HttpPatch("{id}/UpdatePartial")]
    public async Task<ActionResult<ProdutoDTOUpdateResponse>> PatchAsync(int id,
        JsonPatchDocument<produtoDTOUpdateRequest> patchProdutoDTO)
    {
        if (patchProdutoDTO is null || id <= 0) return BadRequest();
        var produto = await _uow.ProdutoRepository.GetAsync(c => c.ProdutoId == id);
        if (produto is null) return NotFound();
        var produtoUpdateRequest = _mapper.Map<produtoDTOUpdateRequest>(produto);

        patchProdutoDTO.ApplyTo(produtoUpdateRequest, ModelState);

        // Verifica erros no ModelState após o Patch
        if (!ModelState.IsValid || !TryValidateModel(produtoUpdateRequest))
            return BadRequest(ModelState);


        _mapper.Map(produtoUpdateRequest, produto);
        _uow.ProdutoRepository.Update(produto);
        await _uow.CommitAsync();

        return Ok(_mapper.Map<ProdutoDTOUpdateResponse>(produto));
    }


    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoDTO>> PutAsync(int id, ProdutoDTO produtoDto)
    {
        if (id != produtoDto.ProdutoId)
        {
            return BadRequest();
        }

        var produtoAtualizado = _uow.ProdutoRepository.Update(_mapper.Map<Produto>(produtoDto));
        await _uow.CommitAsync();
        return (produtoAtualizado is null)
            ? BadRequest()
            : Ok(_mapper.Map<ProdutoDTO>(produtoAtualizado));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProdutoDTO>> Delete(int id)
    {
        var produto = await _uow.ProdutoRepository.GetAsync(c => c.ProdutoId == id);

        if (produto is null)
        {
            return NotFound($"Produto com id={id} não encontrada...");
        }

        var produtoExcluido = _uow.ProdutoRepository.Delete(produto);
        await _uow.CommitAsync();
        return Ok(_mapper.Map<ProdutoDTO>(produtoExcluido));
    }
}