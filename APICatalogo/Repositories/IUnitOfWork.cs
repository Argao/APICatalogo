namespace APICatalogo.Repositories;

public interface IUnitOfWork : IDisposable
{
    IProdutoRepository ProdutoRepository { get; }
    ICategoriaRepository CategoriaRepository { get; }
    void Commit();
}