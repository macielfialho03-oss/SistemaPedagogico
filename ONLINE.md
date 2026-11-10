# Tutoring online: implantação piloto

O modo online usa **um banco PostgreSQL Supabase** (schema `tutoring`) e um bucket privado. Os dois SQLite locais permanecem disponíveis para a instalação antiga; os cadastros antigos, declarados como testes, não são enviados automaticamente. Não execute a aplicação online contra o banco de produção sem validar primeiro este fluxo em homologação.

## Configuração

1. Crie um projeto Supabase e um bucket **privado** chamado `tutoring-private` (ou defina `SUPABASE_BUCKET`). Não permita acesso público, listagem anônima nem políticas de escrita para usuários anônimos. O servidor usa a chave `service_role` exclusivamente no backend; jamais inclua a chave no JavaScript ou em variáveis `NEXT_PUBLIC_*`.
2. Configure estas variáveis como segredos no serviço que hospeda o contêiner ASP.NET Core:

   | Variável | Conteúdo |
   | --- | --- |
   | `DATABASE_URL` | String de conexão ADO.NET Npgsql do PostgreSQL, com SSL e schema `tutoring`; use o pooler de transação Supabase e valide compatibilidade com Npgsql. |
   | `SUPABASE_URL` | URL HTTPS do projeto Supabase. |
   | `SUPABASE_SERVICE_ROLE_KEY` | Chave secreta da API Storage, somente no servidor. |
   | `SUPABASE_BUCKET` | Nome do bucket privado (opcional, padrão `tutoring-private`). |
   | `INITIAL_ADMIN_NAME`, `INITIAL_ADMIN_EMAIL`, `INITIAL_ADMIN_PASSWORD` | Conta inicial da professora; senha com no mínimo 12 caracteres. |
   | `PORT` | Porta HTTP do contêiner (padrão Docker 8080). |

   Exemplo de string Npgsql: `Host=...;Port=6543;Database=postgres;Username=postgres.PROJECT_REF;Password=...;SSL Mode=Require;Pooling=true;Max Pool Size=10;No Reset On Close=false`. Use os dados exatos do painel Supabase, sem copiar credenciais para o repositório. O schema `tutoring` é criado na primeira inicialização; o usuário de banco precisa de permissão de criação no banco. Retire `INITIAL_ADMIN_PASSWORD` depois da primeira inicialização bem sucedida: a conta persiste no banco e a variável não é mais lida para substituir a senha.
3. Construa o `Dockerfile` na Vercel ou em um host de contêiner compatível e configure domínio HTTPS. O cookie exige HTTPS. Não monte armazenamento local para sessões: as chaves de proteção são guardadas no PostgreSQL.
4. A professora entra com a conta inicial, muda a senha em **Acessos > Conta da professora** e cria uma conta individual para cada aluno. Entregue as credenciais por um canal privado. Se esquecer a senha, configure temporariamente `RECOVERY_ADMIN_PASSWORD` (12+ caracteres) e publique novamente; depois de entrar e alterar a senha, remova a variável. A rotação invalida as sessões anteriores.

## Anexos e limites

Arquivos permitidos até **4 MiB** atravessam a função ASP.NET e são validados antes da gravação no bucket privado. A margem abaixo do limite de 4,5 MB da função inclui os campos do formulário. Downloads verificam o aluno vinculado antes de buscar o objeto com a chave do servidor. Para recuperar o limite anterior de 5 MB, será necessário implementar envio direto com autorização temporária e uma etapa de confirmação/validação do conteúdo.

## Cópias e restauração

Em um computador confiável com Python 3.12+, `pg_dump` e `pg_restore`, configure `PG_BACKUP_URL` (URI PostgreSQL/libpq, diferente da string ADO.NET usada pelo aplicativo), `SUPABASE_URL`, `SUPABASE_SERVICE_ROLE_KEY` e opcionalmente `SUPABASE_BUCKET`. Execute `python3 scripts/online-backup.py backup tutoring-AAAA-MM-DD.tar.gz` diariamente ou após alterações relevantes. O pacote contém `database.dump`, anexos e manifesto SHA-256. Transfira a cópia para mídia ou armazenamento fora de Supabase e Vercel, com acesso restrito e criptografia. Não armazene o arquivo em um repositório Git.

Para testar a restauração, crie **outro projeto Supabase vazio** e um bucket privado vazio, substitua as variáveis pelas credenciais desse projeto e execute `CONFIRM_EMPTY_STAGING=yes python3 scripts/online-backup.py restore tutoring-AAAA-MM-DD.tar.gz`. O restaurador exige bucket vazio e verifica hashes antes de restaurar. Confirme a contagem de alunos, aulas e anexos e entre com uma conta de teste. Nunca aponte o comando de restauração para o projeto principal. Repita o teste periodicamente. O serviço não gera ZIP grande em resposta HTTP.

## Verificações antes da entrega

- CI: `dotnet test --configuration Release`; a pipeline executa testes locais e sintaxe JS.
- Supabase de homologação: criar conta, matérias, alunos e aulas semanais, anexar PDF de 4 MiB, verificar erro acima do limite, baixar como aluno correto e negar acesso cruzado.
- Abrir em dois navegadores e redes diferentes: login, bloqueio após cinco erros por 15 minutos, troca/recuperação de senha, expiração da sessão e escritas simultâneas.
- Testar formulários, calendário, contraste, teclado, leitor de tela e largura de celular. Confirmar que fechar uma edição ao clicar fora da caixa atende à decisão da cliente.
- Fazer backup externo, restaurar no projeto de homologação e comparar dados e anexos antes de usar dados reais.

O plano gratuito do Supabase pode pausar por inatividade e não inclui backup automático; acompanhe cotas e mantenha a rotina externa. Verifique os termos do plano de hospedagem no momento da publicação do projeto de extensão sem receita.
