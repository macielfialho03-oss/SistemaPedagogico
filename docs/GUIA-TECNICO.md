# Guia técnico — Tutoring

Este guia descreve o código da `main` no commit `c09bf6a` (17/11/2026). Ele registra o que foi implementado e o que ainda precisa ser verificado. Não atesta que o deploy, o banco hospedado ou a restauração estejam prontos para uso com dados reais.

## Modos de execução

| Modo | Ativação | Dados | Interface |
| --- | --- | --- | --- |
| Local | Sem `DATABASE_URL` | `educamais.db` para o portal e `sistema-pedagogico.db` legado, mais arquivos locais | MVC em `/portal` e Razor Pages antigas ainda mapeadas |
| Online | Com `DATABASE_URL` | PostgreSQL no schema `tutoring`, anexos em bucket privado | MVC em `/portal`; páginas legadas não são mapeadas |

A importação local do banco legado ocorre uma vez e não mantém sincronização contínua. O modo online não importa automaticamente os bancos SQLite.

## Componentes

| Caminho | Responsabilidade |
| --- | --- |
| `src/SistemaPedagogico.Web/Program.cs` | Inicialização, escolha do provedor de dados, autenticação, rotas e serviços |
| `src/SistemaPedagogico.Web/Portal/AccountController.cs` | Login, primeira conta local, saída e bloqueio de tentativas |
| `src/SistemaPedagogico.Web/Portal/PortalController.cs` | Consultas e comandos de cadastro, agenda, notas, atividades, materiais, contas e downloads |
| `src/SistemaPedagogico.Web/Portal/Models.cs` | Modelo EF Core do portal e auxiliares de apresentação |
| `src/SistemaPedagogico.Web/Portal/OnlineBootstrap.cs` | Criação inicial do schema online, matérias e conta inicial |
| `src/SistemaPedagogico.Web/Portal/AttachmentStorage.cs` | Arquivos locais ou bucket privado Supabase, conforme o modo |
| `src/SistemaPedagogico.Infrastructure` | Modelo legado, migrações SQLite, caminhos e backup do banco anterior |
| `src/SistemaPedagogico.Web/Pages` | Interface antiga, disponível apenas no modo local |
| `scripts/online-backup.py` | Exportação e restauração separadas do banco online e anexos |
| `tests/SistemaPedagogico.Tests` | Testes automatizados |

## Perfis e permissões

| Perfil | Acesso previsto no código |
| --- | --- |
| Admin | Gerencia alunos, matérias, aulas, contas, mensalidades e relatórios; também atua como professora no portal |
| Professor | Consulta alunos vinculados e registra notas, atividades, materiais e progresso; não cria aulas pelo comando de agenda |
| Aluno | Consulta os próprios registros, envia respostas e pode cancelar a própria participação futura com justificativa |

A sessão usa cookie HTTP-only e antifalsificação nos formulários. No modo online, o cookie exige HTTPS e as chaves de proteção são persistidas no PostgreSQL. A senha é armazenada por `PasswordHasher`. Cinco erros de login para um e-mail acionam bloqueio de 15 minutos. A tela permite troca de senha com a senha atual para a própria conta.

**Ponto a corrigir antes de dados reais:** ações de senha e ativação de contas por um Professor não verificam, em todos os casos, se o aluno pertence a ele. A interface e o servidor também permitem múltiplos administradores e professores, em desacordo com a decisão anterior de uma única professora. Registros históricos podem ser preservados sem oferecer criação de novas contas de equipe.

## Regras de agenda

- A professora pode selecionar participantes ou escolher os alunos ativos da matéria no momento da criação.
- A repetição cria de 1 a 52 ocorrências a cada sete dias. Cada ocorrência recebe um grupo próprio para seus participantes; não há identificador de série para cancelar/remarcar todas de uma vez.
- Remarcação de um grupo mantém os participantes; aulas iniciadas ou concluídas não podem ser remarcadas.
- O servidor verifica conflito com outras aulas não canceladas no intervalo.
- Cancelamento exige justificativa. Presença e conclusão são registradas por participação.
- Um aluno incluído na matéria depois do agendamento não entra automaticamente nas ocorrências já salvas.

## Atividades, materiais e anexos

A publicação de uma atividade para vários alunos cria uma linha por destinatário ligada por grupo; entrega e correção continuam individuais. Há edição do enunciado antes de qualquer entrega, além de exclusão condicionada ao estado da atividade. Materiais podem ser vinculados a um aluno ou marcados como gerais; os gerais ficam visíveis a todos os alunos.

O servidor valida tipo e conteúdo antes de gravar o arquivo. São aceitos `pdf`, `docx`, `xlsx`, `pptx`, `txt`, `png`, `jpg` e `jpeg`. O limite do fluxo atual é **4 MiB**, com limite da requisição de 4.400.000 bytes. O upload passa pelo ASP.NET; upload direto com autorização temporária ainda não foi implementado. O bucket online deve ser **privado**. O download consulta a permissão no aplicativo antes de buscar o objeto.

## Persistência e inicialização

- Local: EF Core com SQLite. `PortalBootstrap` prepara o banco do portal e importa dados antigos do `SistemaPedagogicoDbContext`; o backup local do portal inclui os dois bancos e arquivos.
- Online: EF Core com Npgsql e PostgreSQL. `OnlineBootstrap` cria o schema `tutoring` e o modelo inicial se a tabela `Students` ainda não existir. Isso não substitui um fluxo de migrações versionadas para futuras mudanças de schema.
- A conexão ao pooler Supabase na porta 6543 é convertida pelo aplicativo para a porta 5432, modo de sessão. A instrução anterior em `ONLINE.md` sobre pooler de transação precisa ser atualizada junto com a configuração efetivamente usada.
- A inicialização online ocorre em serviço em segundo plano; falhas são registradas no log. É preciso validar o comportamento em uma implantação nova e em reinicializações.
- `INITIAL_ADMIN_NAME`, `INITIAL_ADMIN_EMAIL` e `INITIAL_ADMIN_PASSWORD` criam a primeira conta online; a senha inicial exige 12 caracteres. `RECOVERY_ADMIN_PASSWORD` permite recuperar a conta da professora e deve ser removida depois. Credenciais ficam apenas no ambiente do servidor.

## Rotas principais

| Rota | Uso |
| --- | --- |
| `GET/POST /conta/entrar` | Login |
| `POST /conta/configurar` | Primeira conta local, restrita ao próprio computador |
| `POST /conta/sair` | Encerrar sessão |
| `GET /portal?section=...` | Telas do portal conforme o perfil |
| `POST /portal/salvar?kind=...` | Ações do portal com validação antifalsificação |
| `GET /portal/arquivo/{type}/{id}` | Download autorizado de material, enunciado ou resposta |
| `GET /portal/relatorio` | CSV de alunos, apenas Admin |
| `POST /portal/backup` | ZIP local, apenas Admin; online usa script externo |

## Testes e publicação

Execute `dotnet test --configuration Release`, `node --check src/SistemaPedagogico.Web/wwwroot/js/educamais.js` e `python3 -m py_compile scripts/online-backup.py`. O workflow GitHub Actions também executa essas verificações em push e PR. Os testes automatizados não confirmam sozinhos a aplicação hospedada.

Antes da entrega online: criar dados de teste em homologação; conferir isolamento de alunos e autorização de arquivos; testar professora e aluno em redes/navegadores distintos, login bloqueado, recuperação de senha, celular, teclado, contraste, erros e escrita simultânea. Criar backup e **restaurar em outro projeto Supabase vazio**, conferindo banco e anexos. Siga [ONLINE.md](../ONLINE.md) para variáveis, script e cuidados com as cópias.

## Pendências de documentação e produto

- Decidir o comportamento da edição ao clicar fora do cartão e validar a paleta de conforto com a cliente.
- Definir se haverá comando para cancelar/remarcar a série semanal inteira; hoje apenas uma ocorrência em grupo é afetada.
- Consolidar a apresentação de uma professora e revisar privilégios de contas.
- Atualizar instruções antigas: o roteiro local cita limite de 5 MB e o histórico 4.3 ainda descreve a hospedagem como etapa futura. O código atual usa 4 MiB online.
- Conferir uso real da Vercel, versionamento de schema e execução periódica/restauração do backup antes de usar dados reais.
