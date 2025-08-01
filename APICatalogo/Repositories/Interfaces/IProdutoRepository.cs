using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories.Interfaces;

public interface IProdutoRepository : IRepository<Produto>
{
    
    Task<PagedList<Produto>> GetProdutosAsync(ProdutosParameters produtosParameters);
    Task<PagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroParameters);
    Task<IEnumerable<Produto>> GetProdutosPorCategoriaAsync(int id);
}