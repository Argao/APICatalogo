# Configuração Docker para APICatalogo

## Configuração: Apenas SQL Server no Docker

A aplicação está configurada para rodar **localmente** (.NET) e conectar com o **SQL Server no Docker**.

### Arquitetura:
- ✅ **SQL Server**: Roda no container Docker (porta 1433)
- ✅ **Aplicação .NET**: Roda localmente com `dotnet run`
- ✅ **Desenvolvimento**: Usa HTTPS local normal

## Como usar:

### 1. Iniciar apenas o SQL Server no Docker:
```bash
docker-compose up -d
```
Isso iniciará apenas o container do SQL Server.

### 2. Executar a aplicação localmente:
```bash
cd APICatalogo
dotnet run --launch-profile https
```

### 3. Acessar a aplicação:
- **API**: https://localhost:7278
- **Swagger**: https://localhost:7278/swagger
- **SQL Server**: localhost:1433

## Credenciais do SQL Server:
- **Usuário**: sa
- **Senha**: YourStrong@Passw0rd
- **Porta**: 1433
- **Connection String**: `Server=localhost,1433;Database=CatalogoDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true`

## Comandos úteis:

### Docker (SQL Server):
```bash
# Iniciar SQL Server
docker-compose up -d

# Ver logs do SQL Server
docker-compose logs -f sqlserver

# Parar SQL Server
docker-compose down

# Parar e remover dados (cuidado!)
docker-compose down -v
```

### Aplicação local:
```bash
# Executar migrações (com SQL Server rodando)
dotnet ef database update --project APICatalogo

# Rodar aplicação em modo desenvolvimento
dotnet run --launch-profile https --project APICatalogo

# Rodar aplicação em modo HTTP
dotnet run --launch-profile http --project APICatalogo
```

## Vantagens desta configuração:
- ✅ Desenvolvimento local normal com hot reload
- ✅ Debug completo no Visual Studio/VS Code
- ✅ SQL Server isolado e persistente
- ✅ Sem problemas de certificados SSL
- ✅ Performance máxima da aplicação
- ✅ Fácil acesso ao banco via SQL Server Management Studio

## Para conectar via SQL Server Management Studio:
- **Server**: localhost,1433
- **Authentication**: SQL Server Authentication
- **Login**: sa
- **Password**: YourStrong@Passw0rd
