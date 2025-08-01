using APICatalogo.Context;
using APICatalogo.DTO;
using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories;

public class ProdutoRepository(AppDbContext context) : Repository<Produto>(context), IProdutoRepository
{
    public async Task<PagedList<Produto>> GetProdutosAsync(ProdutosParameters produtosParameters)
    {
        var produtos = await GetAllAsync();
        var produtosOrdenados = produtos.OrderBy(p => p.ProdutoId).AsQueryable();
        var resultado =  PagedList<Produto>.ToPagedList(produtosOrdenados,produtosParameters.PageNumber, produtosParameters.PageSize);
        return resultado;
    }

    public async Task<PagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroParameters)
    {
        var produtos = await GetAllAsync();
    
        if (produtosFiltroParameters.Preco.HasValue && produtosFiltroParameters.PrecoCriterio.HasValue)
        {
            var preco = produtosFiltroParameters.Preco.Value;
            var criterio = produtosFiltroParameters.PrecoCriterio.Value;

            produtos = criterio switch
            {
                CriterioEnum.Maior => produtos.Where(p => p.Preco > preco),
                CriterioEnum.Igual => produtos.Where(p => p.Preco == preco),
                CriterioEnum.Menor => produtos.Where(p => p.Preco < preco),
                _ => throw new ArgumentOutOfRangeException(nameof(criterio))
            };

            produtos = produtos.OrderBy(p => p.Preco);
        }

        return PagedList<Produto>.ToPagedList(produtos.AsQueryable(), produtosFiltroParameters.PageNumber, produtosFiltroParameters.PageSize);
    }


    public async Task<IEnumerable<Produto>> GetProdutosPorCategoriaAsync(int id)
    {
        var  produtos = await GetAllAsync();
        return produtos.Where(c => c.CategoriaId == id);
    }
}