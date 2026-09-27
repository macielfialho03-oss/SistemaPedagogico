# Caderno Pedagógico

Sistema web local para acompanhamento e reforço escolar, construído com ASP.NET Core Razor Pages, .NET 10, Entity Framework Core e SQLite.

## Executar no desenvolvimento

1. Abra `SistemaPedagogico.slnx` no Visual Studio 2026 Insiders.
2. Defina `SistemaPedagogico.Web` como projeto de inicialização.
3. Execute o perfil `http`.

Pelo terminal, dentro da pasta da solução:

```powershell
dotnet tool restore
dotnet run --project src\SistemaPedagogico.Web
```

O endereço padrão é `http://localhost:5003`.

## Dados

Em desenvolvimento, o SQLite fica em `Dados\sistema-pedagogico.db`, separado de `src`, `bin` e `publish`. A pasta `Dados` não é versionada e nunca deve ser incluída em pacotes de atualização.

Em uma publicação instalada, o caminho padrão é resolvido pelo sistema operacional em `LocalApplicationData\SistemaPedagogico\Dados`. Ele pode ser alterado por `Storage:DataDirectory`, sem caminhos de unidade fixos no código.

## Atualizações

- Nunca use `EnsureDeleted`.
- Gere uma migration para cada alteração do modelo.
- O sistema cria backup antes de aplicar migrations pendentes em um banco existente.
- Pacotes de atualização devem conter somente o programa, nunca a pasta de dados.

## Comandos úteis

```powershell
dotnet build SistemaPedagogico.slnx
dotnet test SistemaPedagogico.slnx
dotnet tool run dotnet-ef migrations add NomeDaMigration --project src\SistemaPedagogico.Infrastructure --startup-project src\SistemaPedagogico.Web --output-dir Data\Migrations
```
