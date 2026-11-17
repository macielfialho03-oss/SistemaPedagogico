# Arquitetura do Tutoring

Documento alinhado ao código da `main` em `c09bf6a` (17/11/2026). Consulte o [guia técnico](GUIA-TECNICO.md) para regras, permissões e pendências.

## Projetos e camadas

| Componente | Responsabilidade |
| --- | --- |
| `SistemaPedagogico.Web` | Aplicação ASP.NET Core: portal MVC/Razor Views, login, API de horários, anexos e páginas Razor legadas |
| `SistemaPedagogico.Core` | Entidades e regras do modelo pedagógico anterior |
| `SistemaPedagogico.Infrastructure` | EF Core SQLite legado, caminhos de armazenamento e backup local |
| `SistemaPedagogico.Tests` | Testes de fluxos e regras |

## Portal atual e interface antiga

A interface corrente fica em `Web/Portal` e `Web/Views/Portal`. `Program.cs` registra os controllers em ambos os modos e as antigas `Web/Pages` somente no modo local. As páginas antigas usam `SistemaPedagogicoDbContext`, `site.css` e `site.js`; o portal usa `EducaDb`, `educamais.css` e `educamais.js`.

No modo local, `PortalBootstrap.ImportAsync` lê o banco anterior e copia os dados relevantes uma vez para o banco do portal. Os bancos permanecem separados e não se sincronizam posteriormente. Essas páginas e o caminho de importação precisam ser tratados juntos numa futura limpeza do código.

## Persistência

Sem `DATABASE_URL`, o aplicativo usa `educamais.db` no portal e `sistema-pedagogico.db` na versão legada, com anexos locais. Com `DATABASE_URL`, o `EducaDb` usa Npgsql/PostgreSQL no schema `tutoring` e `SupabaseAttachmentStorage` grava em bucket privado. O aplicativo usa credencial de serviço do Storage **somente no servidor**; autorizações de leitura são conferidas no controller.

O bootstrap online cria a estrutura inicial quando a tabela `Students` não existe. Mudanças posteriores no modelo exigem um processo de migração versionado.

## Acesso e sessões

`AccountController` cuida de login, conta inicial local e bloqueio de tentativas. O bootstrap online cria a primeira conta com variáveis do servidor e oferece recuperação por variável temporária. `Program.cs` valida a sessão por usuário ativo e selo de sessão; chaves criptográficas do cookie ficam no PostgreSQL online ou na pasta local no modo SQLite.

Os perfis atuais são Admin, Professor e Aluno. A separação entre vários professores foi adicionada depois da decisão inicial de uma professora única; a revisão de acesso por Professor está pendente.

## Backup

Localmente, há backup SQLite e ZIP dos dois bancos e anexos pelo portal. Online, `scripts/online-backup.py` exporta PostgreSQL e objetos do bucket para um arquivo externo; a restauração deve ser testada em outro projeto vazio. O endpoint de backup web não gera um ZIP online.

## Publicação

`Dockerfile` e `vercel.json` configuram o contêiner. `ONLINE.md` lista as variáveis e o roteiro de implantação. A GitHub Actions verifica testes .NET e sintaxe dos scripts, enquanto testes em celular, autorização e restauração dependem de uma homologação real.
