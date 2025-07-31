using APICatalogo.Context;
using APICatalogo.DTO;
using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories;

public class ProdutoRepository(AppDbContext context) : Repository<Produto>(context), IProdutoRepository
{
    public PagedList<Produto> GetProdutos(ProdutosParameters produtosParameters)
    {
        var produtos = GetAll().OrderBy(p => p.ProdutoId).AsQueryable();
        var produtosOrdenados =  PagedList<Produto>.ToPagedList(produtos, produtosParameters.PageNumber, produtosParameters.PageSize);
        return produtosOrdenados;
    }

    public PagedList<Produto> GetProdutosFiltroPreco(ProdutosFiltroPreco produtosFiltroParameters)
    {
        var produtos = GetAll().AsQueryable();
    
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

        return PagedList<Produto>.ToPagedList(produtos, produtosFiltroParameters.PageNumber, produtosFiltroParameters.PageSize);
    }


    public IEnumerable<Produto> GetProdutosPorCategoria(int id)
    {
        return GetAll().Where(c => c.CategoriaId == id);
    }
}