using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories;

public interface ICategoriaRepository :  IRepository<Categoria>
{
    PagedList<Categoria> GetCategoriasFiltroNome(CategoriaFiltroNome categoriaParameters); 
    PagedList<Categoria> GetCategorias(CategoriaParameters categoriaParameters);
}