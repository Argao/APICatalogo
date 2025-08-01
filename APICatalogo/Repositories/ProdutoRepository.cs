using APICatalogo.Context;
using APICatalogo.DTO;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Repositories;

public class ProdutoRepository(AppDbContext context) : Repository<Produto>(context), IProdutoRepository
{
    public async Task<PagedList<Produto>> GetProdutosAsync(ProdutosParameters produtosParameters)
    {
        IQueryable<Produto> query = _context.Produtos.OrderBy(p => p.ProdutoId) ?? throw new ArgumentNullException(nameof(query));
        
        return await PagedList<Produto>.ToPagedListAsync(query,produtosParameters.PageNumber, produtosParameters.PageSize);
    }

    public async Task<PagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroParameters)
    {
        IQueryable<Produto> query = _context.Produtos ?? throw new ArgumentNullException(nameof(query));
    
        if (produtosFiltroParameters.Preco.HasValue && produtosFiltroParameters.PrecoCriterio.HasValue)
        {
            var preco = produtosFiltroParameters.Preco.Value;
            var criterio = produtosFiltroParameters.PrecoCriterio.Value;

            query = criterio switch
            {
                CriterioEnum.Maior => query.Where(p => p.Preco > preco),
                CriterioEnum.Igual => query.Where(p => p.Preco == preco),
                CriterioEnum.Menor => query.Where(p => p.Preco < preco),
                _ => throw new ArgumentOutOfRangeException(nameof(criterio))
            };

            query =  query.OrderBy(p => p.Preco);
        }

        return await PagedList<Produto>.ToPagedListAsync(query, produtosFiltroParameters.PageNumber, produtosFiltroParameters.PageSize);
    }


    public async Task<IEnumerable<Produto>> GetProdutosPorCategoriaAsync(int id)
    {
        return await _context.Produtos.Where(c => c.CategoriaId == id).ToListAsync();
    }
}