using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories.Interfaces;

public interface ICategoriaRepository :  IRepository<Categoria>
{
    Task<PagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriaFiltroNome categoriaParameters); 
    Task<PagedList<Categoria>> GetCategoriasAsync(CategoriaParameters categoriaParameters);
}