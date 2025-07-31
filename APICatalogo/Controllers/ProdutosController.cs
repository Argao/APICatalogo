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
    public ActionResult<IEnumerable<ProdutoDTO>> Get()
    {
        var produtos = _uow.ProdutoRepository.GetAll().ToList();

        if (produtos.Count == 0)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<IEnumerable<ProdutoDTO>>(produtos));
    }

    [HttpGet("pagination")]
    public ActionResult<IEnumerable<ProdutoDTO>> Get([FromQuery] ProdutosParameters produtosParameters)
    {
        var produtos = _uow.ProdutoRepository.GetProdutos(produtosParameters);

        return ObterProdutos(produtos);
    }
    

    [HttpGet("filter/preco/pagination")]
    public ActionResult<IEnumerable<ProdutoDTO>> GetProdutosFilterPreco([FromQuery] ProdutosFiltroPreco produtosFiltroParameters)
    {
        var produtos = _uow.ProdutoRepository.GetProdutosFiltroPreco(produtosFiltroParameters);
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
    public ActionResult<ProdutoDTO> Get(int id)
    {
        var produto = _uow.ProdutoRepository.Get(c => c.ProdutoId == id);
        if (produto is null)
        {
            return NotFound("Produto não encontrado...");
        }

        return Ok(_mapper.Map<ProdutoDTO>(produto));
    }

    [HttpGet("produtos/{id}")]
    public ActionResult<IEnumerable<ProdutoDTO>> GetProdutosCategoria(int id)
    {
        var produtos = _uow.ProdutoRepository.GetProdutosPorCategoria(id);

        return (!produtos.Any())
            ? NotFound("Nenhum produto com essa categoria")
            : Ok(_mapper.Map<IEnumerable<ProdutoDTO>>(produtos));
    }

    [HttpPost]
    public ActionResult<ProdutoDTO> Post(ProdutoDTO produtoDto)
    {
        var novoProduto = _uow.ProdutoRepository.Create(_mapper.Map<Produto>(produtoDto));
        _uow.Commit();

        var novoProdutoDTO = _mapper.Map<ProdutoDTO>(novoProduto);

        return new CreatedAtRouteResult("ObterProduto",
            new { id = novoProdutoDTO.ProdutoId },
            novoProdutoDTO
        );
    }

    [HttpPatch("{id}/UpdatePartial")]
    public ActionResult<ProdutoDTOUpdateResponse> Patch(int id,
        JsonPatchDocument<produtoDTOUpdateRequest> patchProdutoDTO)
    {
        if (patchProdutoDTO is null || id <= 0) return BadRequest();
        var produto = _uow.ProdutoRepository.Get(c => c.ProdutoId == id);
        if (produto is null) return NotFound();
        var produtoUpdateRequest = _mapper.Map<produtoDTOUpdateRequest>(produto);

        patchProdutoDTO.ApplyTo(produtoUpdateRequest, ModelState);

        // Verifica erros no ModelState após o Patch
        if (!ModelState.IsValid || !TryValidateModel(produtoUpdateRequest))
            return BadRequest(ModelState);


        _mapper.Map(produtoUpdateRequest, produto);
        _uow.ProdutoRepository.Update(produto);
        _uow.Commit();

        return Ok(_mapper.Map<ProdutoDTOUpdateResponse>(produto));
    }


    [HttpPut("{id:int}")]
    public ActionResult<ProdutoDTO> Put(int id, ProdutoDTO produtoDto)
    {
        if (id != produtoDto.ProdutoId)
        {
            return BadRequest();
        }

        var produtoAtualizado = _uow.ProdutoRepository.Update(_mapper.Map<Produto>(produtoDto));
        _uow.Commit();
        return (produtoAtualizado is null)
            ? BadRequest()
            : Ok(_mapper.Map<ProdutoDTO>(produtoAtualizado));
    }

    [HttpDelete("{id:int}")]
    public ActionResult<ProdutoDTO> Delete(int id)
    {
        var produto = _uow.ProdutoRepository.Get(c => c.ProdutoId == id);

        if (produto is null)
        {
            return NotFound($"Produto com id={id} não encontrada...");
        }

        var produtoExcluido = _uow.ProdutoRepository.Delete(produto);
        _uow.Commit();
        return Ok(_mapper.Map<ProdutoDTO>(produtoExcluido));
    }
}