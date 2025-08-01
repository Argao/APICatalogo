using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Repositories;

public class CategoriaRepository(AppDbContext context) : Repository<Categoria>(context), ICategoriaRepository
{
    public async Task<PagedList<Categoria>> GetCategoriasFiltroNomeAsync (CategoriaFiltroNome categoriaParameters)
    {
        var query = _context.Categorias.AsQueryable();
        
        if (!string.IsNullOrEmpty(categoriaParameters.Nome))
        {
            query = query.Where(c => c.Nome.Contains(categoriaParameters.Nome));
        }
        
        return await PagedList<Categoria>.ToPagedListAsync(query, categoriaParameters.PageNumber, categoriaParameters.PageSize);
    }

    public async Task<PagedList<Categoria>> GetCategoriasAsync(CategoriaParameters categoriaParameters)
    {
        var query = _context.Categorias.OrderBy(c => c.CategoriaId).AsQueryable();
        
       return await PagedList<Categoria>
            .ToPagedListAsync(query, categoriaParameters.PageNumber, categoriaParameters.PageSize);
    }
}