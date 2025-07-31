using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Repositories;

public class CategoriaRepository(AppDbContext context) : Repository<Categoria>(context), ICategoriaRepository
{
    public PagedList<Categoria> GetCategoriasFiltroNome(CategoriaFiltroNome categoriaParameters)
    {
        var categorias = GetAll().AsQueryable();
        
        if (!string.IsNullOrEmpty(categoriaParameters.Nome))
        {
            categorias = categorias.Where(c => c.Nome.Contains(categoriaParameters.Nome));
        }
        
        return PagedList<Categoria>.ToPagedList(categorias, categoriaParameters.PageNumber, categoriaParameters.PageSize);
        
    }

    public PagedList<Categoria> GetCategorias(CategoriaParameters categoriaParameters)
    {
        var categorias = GetAll().OrderBy(c => c.CategoriaId).AsQueryable();
        var categoriasOrdenadas =  PagedList<Categoria>.ToPagedList(categorias, categoriaParameters.PageNumber, categoriaParameters.PageSize);
        return categoriasOrdenadas;
    }
}