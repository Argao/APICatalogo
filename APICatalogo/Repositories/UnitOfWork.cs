using APICatalogo.Context;

namespace APICatalogo.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private IProdutoRepository? _produtoRepository;
    private ICategoriaRepository? _categoriaRepository;
    public AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

   public IProdutoRepository ProdutoRepository => _produtoRepository ??= new ProdutoRepository(_context);

   public ICategoriaRepository  CategoriaRepository => _categoriaRepository ??= new CategoriaRepository(_context);
   
    
    public  async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}