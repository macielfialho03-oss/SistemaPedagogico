# Tutoring — Reforço e Acompanhamento Escolar

O **Tutoring** é um sistema pedagógico desenvolvido em um projeto acadêmico de extensão para organizar aulas de reforço e acompanhamento escolar. A professora administra cadastros, agenda, atividades e registros de aprendizagem; cada aluno acessa seus próprios dados.

> **Estado da documentação:** descreve o código da branch `main` no commit `c09bf6a` (17/11/2026). A [página de entrada publicada](https://sistemapedagogico.vercel.app/conta/entrar?ReturnUrl=%2Fportal) abre na Vercel; o modo online usa PostgreSQL e Storage do Supabase. Isso ainda não substitui os testes funcionais, de privacidade e restauração. Consulte [o guia técnico](docs/GUIA-TECNICO.md) e [o roteiro online](ONLINE.md).

## Problema e público

Uma professora precisa acompanhar alunos de matérias diferentes, organizar aulas, registrar notas e progresso e disponibilizar materiais e atividades. Os alunos precisam consultar seus compromissos e entregas com acesso individual.

O projeto atende à professora e aos alunos cadastrados. Responsáveis são registrados no cadastro do aluno; o código atual não oferece um perfil de acesso independente para responsáveis.

## Proposta e objetivo

Reunir agenda, cadastros, atividades, materiais, progresso e registros de mensalidades em um portal web, com permissões por perfil. O objetivo do piloto online é validar o uso da professora e dos alunos, a privacidade dos anexos e a recuperação dos dados antes da entrega final.

## Requisitos funcionais

Os códigos `RF` identificam o que o sistema deve fazer. **Presente no código** significa que a função foi encontrada na versão analisada; ainda pode exigir teste no site publicado.

| Código | Requisito | Situação |
| --- | --- | --- |
| RF01 | Permitir entrada com contas individuais de professora e alunos. | Presente no código |
| RF02 | Cadastrar alunos, responsáveis e matérias. | Presente no código |
| RF03 | Agendar aulas para um ou vários alunos, inclusive os alunos ativos de uma matéria e repetições semanais. | Presente no código |
| RF04 | Remarcar ou cancelar uma ocorrência para todos os participantes, com justificativa no cancelamento. | Presente no código |
| RF05 | Publicar atividades para vários alunos e corrigir cada entrega individualmente. | Presente no código |
| RF06 | Disponibilizar materiais, notas, progresso e agenda conforme o perfil. | Presente no código |
| RF07 | Registrar mensalidades e exportar a relação de alunos. | Presente no código; não realiza cobrança |
| RF08 | Criar a conta inicial online, permitir troca de senha e limitar tentativas de login. | Presente no código |
| RF09 | Gerar cópia do banco e anexos online e restaurá-la em outro projeto. | Script presente; restauração não comprovada |
| RF10 | Remarcar ou cancelar a **série semanal inteira** de uma vez. | Pendente; hoje afeta uma ocorrência |

## Requisitos não funcionais

| Código | Requisito | Situação |
| --- | --- | --- |
| RNF01 | Proteger contas e dados com autenticação, autorização e conexões HTTPS. | Implementação presente; teste de isolamento pendente |
| RNF02 | Manter anexos em armazenamento privado, com limite e validação de formato. | Implementação presente; teste no ambiente publicado pendente |
| RNF03 | Permitir acesso em celular e computador. | Layout responsivo presente; revisão prática pendente |
| RNF04 | Permitir uso por professora e alunos em redes e dispositivos diferentes, sem depender do computador da professora. | Arquitetura online presente; teste simultâneo pendente |
| RNF05 | Garantir navegação por teclado, mensagens claras e contraste adequado. | Avaliação de acessibilidade pendente |
| RNF06 | Recuperar dados e anexos a partir de cópia externa. | Script presente; rotina e restauração pendentes |

## Regras de negócio

- Uma aula para uma matéria inclui os alunos ativos dessa matéria **no momento do agendamento**. Novos alunos não entram automaticamente nas ocorrências já criadas.
- O agendamento semanal cria até 52 ocorrências. Cada ocorrência agrupa seus participantes; a série completa ainda não tem ação de remarcação ou cancelamento.
- Aulas conflitantes no mesmo horário são recusadas; cancelar exige uma justificativa.
- Atividades publicadas em grupo preservam resposta, nota e feedback separados por aluno.
- Materiais marcados como **gerais** são destinados a todos os alunos; os demais dependem do aluno vinculado.
- No fluxo atual, anexos de até **4 MiB** aceitam PDF, DOCX, XLSX, PPTX, TXT, JPG ou PNG. Vídeos não são aceitos.
- Responsáveis são dados do cadastro do aluno, sem conta própria. O código permite hoje múltiplos perfis administrativos e de professor; a definição anterior do projeto era uma professora única.

## Histórias de usuário e aceitação

- **HU01 — Professora:** quero cadastrar alunos e matérias para organizar meus atendimentos. **Aceite:** o cadastro salvo aparece no portal e pode ser usado no agendamento.
- **HU02 — Professora:** quero marcar matemática na segunda para os alunos ativos dessa matéria. **Aceite:** todos os elegíveis no momento da criação recebem sua participação; outro compromisso no horário gera aviso de conflito.
- **HU03 — Aluno:** quero consultar minhas aulas, materiais e atividades e enviar minha resposta. **Aceite:** minha conta não mostra os registros individuais de outro aluno.
- **HU04 — Professora:** quero acompanhar as entregas de uma atividade em grupo. **Aceite:** corrigir um aluno não altera a resposta nem a situação dos demais.
- **HU05 — Professora:** quero cancelar uma aula para o grupo explicando o motivo. **Aceite:** a ocorrência é cancelada para seus participantes e a justificativa fica registrada.

O [documento de requisitos](docs/REQUISITOS.md) detalha os critérios de aceitação, a situação de cada item e os testes necessários antes da entrega.

## Funcionalidades presentes no código

- Cadastros de alunos, responsáveis, matérias e contas de acesso.
- Aulas individuais ou em grupo, com repetição semanal de até 52 ocorrências, bloqueio de conflitos, remarcação e cancelamento com justificativa.
- Inclusão dos alunos ativos de uma matéria ao **criar** uma aula para a matéria; alunos vinculados depois não são inseridos nas ocorrências já criadas.
- Atividades para um ou vários alunos, página de entregas, resposta e correção individual.
- Notas, materiais, progresso e registro de mensalidades, sem cobrança online.
- Exportação CSV dos alunos, backup local e script separado de backup/restauração do PostgreSQL e anexos online.

## Regras e limites importantes

| Tema | Comportamento atual |
| --- | --- |
| Agenda | Uma ocorrência reúne os participantes em um grupo; cancelamento e remarcação em grupo afetam aquela ocorrência, não toda a série semanal. |
| Acesso | Existem perfis Admin, Professor e Aluno no código. A interface atual permite criar mais de um administrador ou professor, embora o requisito inicial previsse uma professora única. |
| Anexos | PDF, DOCX, XLSX, PPTX, TXT, JPG e PNG, validados pelo servidor, até **4 MiB** por arquivo no fluxo atual. Vídeos são recusados. |
| Materiais gerais | Um material marcado como geral fica acessível a todos os alunos. |
| Privacidade | O acesso a registros e downloads passa por regras no servidor; a revisão de permissões para ações de contas por Professor ainda está pendente. |

## Usar o Tutoring online

**Endereço público do site:** [Entrar no Tutoring](https://sistemapedagogico.vercel.app/conta/entrar?ReturnUrl=%2Fportal). Esta página foi aberta e conferida; seu uso com contas reais ainda precisa dos testes descritos abaixo.

1. **Professora:** entre com o e-mail e a senha inicial configurados na Vercel pelo responsável pela implantação. Na seção **Acessos**, use **Alterar minha senha**. Cadastre matérias e alunos; para cada aluno, crie um acesso individual e entregue suas credenciais por um canal privado.
2. **Aulas:** em **Aulas**, selecione uma matéria e use a opção de incluir os alunos ativos dessa matéria. Escolha data/hora e, se desejar, repetições semanais; confira os participantes antes de salvar.
3. **Atividades e materiais:** publique para alunos escolhidos ou materiais gerais. Cada aluno entra com a própria conta para ver aulas, notas, materiais e atividades e enviar sua resposta.
4. **Saída:** encerre a sessão ao terminar, especialmente em computador compartilhado.

Todos acessam **o mesmo site online** com suas próprias contas. Os dados ficam no PostgreSQL Supabase e os anexos no bucket privado; o computador da professora não precisa estar ligado para o aluno entrar. O aplicativo não oferece criação pública de conta de aluno. Se a professora esquecer a senha, o responsável pela implantação usa o procedimento de recuperação descrito em [ONLINE.md](ONLINE.md).

## Tecnologias e organização

- **ASP.NET Core / .NET 10**, Razor Pages antigas e portal atual em MVC com Razor Views.
- **Entity Framework Core**, SQLite na instalação local e PostgreSQL no modo online.
- **Supabase Storage** privado para anexos no modo online; **Dockerfile** e `vercel.json` para implantação do contêiner.
- Testes automatizados com **xUnit** e verificação em GitHub Actions.

O portal atual fica em `src/SistemaPedagogico.Web/Portal` e `Views/Portal`. O projeto `Core` contém entidades antigas, `Infrastructure` contém persistência/backup SQLite e `tests` contém os testes. Veja [o mapa de arquitetura](docs/ARQUITETURA.md).

## Executar localmente para desenvolvimento

1. Instale o **SDK .NET 10**.
2. No Windows, execute `Verificar-Tutoring.cmd` para compilar/testar e `Iniciar-Tutoring.cmd` para iniciar; ou use:

   ```bash
   dotnet test --configuration Release
   dotnet run --project src/SistemaPedagogico.Web --launch-profile http
   ```
3. Abra `http://localhost:5003` e, na primeira execução local, crie a conta da professora no próprio computador.

O portal usa `/portal`. O modo local ainda registra rotas Razor Pages da versão anterior e mantém **dois bancos SQLite**; veja [o roteiro local](LEIA-ME-TESTE.md). Esses dados não são transferidos automaticamente para o Supabase.

## Configuração do ambiente online

O modo online é ativado por `DATABASE_URL` na Vercel e exige outras variáveis privadas do servidor, projeto Supabase e bucket privado. A primeira conta online é provisionada por variáveis de ambiente. Siga [ONLINE.md](ONLINE.md) para configuração, recuperação da senha e backup. Nunca publique senhas, strings de conexão, bancos, anexos ou arquivos de backup no GitHub.

## Verificação e pendências

Os testes automatizados podem ser executados pelo comando acima; isso não substitui testes no ambiente hospedado. Antes da entrega, validar restauração em um **segundo projeto**, isolamento entre alunos, permissões do perfil Professor, celular, acessibilidade, formulários, erros e uso em redes diferentes. Decidir com a cliente o comportamento ao clicar fora de um cartão e se haverá ação para a série inteira.

## Documentação

- [Requisitos e critérios de aceitação](docs/REQUISITOS.md)
- [Guia técnico e regras verificadas](docs/GUIA-TECNICO.md)
- [Arquitetura](docs/ARQUITETURA.md)
- [Implantação online e backup](ONLINE.md)
- [Roteiro da versão local](LEIA-ME-TESTE.md)
- [Histórico da versão 4.3](VERSAO-4.3.md)
