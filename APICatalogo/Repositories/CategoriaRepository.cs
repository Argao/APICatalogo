using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Repositories;

public class CategoriaRepository(AppDbContext context) : Repository<Categoria>(context), ICategoriaRepository
{
    public async Task<PagedList<Categoria>> GetCategoriasFiltroNomeAsync (CategoriaFiltroNome categoriaParameters)
    {
        var categorias = await GetAllAsync();
        
        if (!string.IsNullOrEmpty(categoriaParameters.Nome))
        {
            categorias = categorias.Where(c => c.Nome.Contains(categoriaParameters.Nome));
        }
        
        return PagedList<Categoria>.ToPagedList(categorias.AsQueryable(), categoriaParameters.PageNumber, categoriaParameters.PageSize);
        
    }

    public async Task<PagedList<Categoria>> GetCategoriasAsync(CategoriaParameters categoriaParameters)
    {
        var categorias = await GetAllAsync();
        var categoriasOrdenadas = categorias.OrderBy(c => c.CategoriaId).AsQueryable();
        var resultado =  PagedList<Categoria>
            .ToPagedList(categoriasOrdenadas, categoriaParameters.PageNumber, categoriaParameters.PageSize);
        return resultado;
    }
}