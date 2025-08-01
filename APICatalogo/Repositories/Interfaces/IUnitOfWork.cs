namespace APICatalogo.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProdutoRepository ProdutoRepository { get; }
    ICategoriaRepository CategoriaRepository { get; }
    Task CommitAsync();
}