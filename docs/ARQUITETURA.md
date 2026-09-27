# Arquitetura

## Projetos

- `SistemaPedagogico.Core`: entidades e regras de domínio.
- `SistemaPedagogico.Infrastructure`: EF Core, SQLite, caminhos de armazenamento e backup.
- `SistemaPedagogico.Web`: interface Razor Pages e inicialização da aplicação.
- `SistemaPedagogico.Tests`: testes automatizados das regras.

## Fluxo pedagógico

`Acompanhamento → Dificuldade → Plano de reforço → Sessão → Exercício/resultado → Evolução`

## Persistência

O `StoragePaths` resolve o diretório em tempo de execução. O desenvolvimento usa caminho relativo até `D:\SistemaPedagogico\Dados`, configurado apenas no `appsettings.Development.json`. A configuração de produção não depende da unidade `D:`.

O `SistemaPedagogicoDbContext` aplica exclusão restrita às relações históricas. Cadastros de referência usam ativação/inativação em vez de exclusão física.

## Backup

O `BackupService` utiliza a API de backup do SQLite, lista cópias locais, verifica integridade com `PRAGMA integrity_check` e cria uma cópia do banco atual antes de uma restauração.
