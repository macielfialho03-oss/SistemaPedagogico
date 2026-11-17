# Tutoring — Reforço e Acompanhamento Escolar

O **Tutoring** é um sistema pedagógico desenvolvido em um projeto acadêmico de extensão para organizar aulas de reforço e acompanhamento escolar. A professora administra cadastros, agenda, atividades e registros de aprendizagem; cada aluno acessa seus próprios dados.

> **Estado da documentação:** descreve o código da branch `main` no commit `c09bf6a` (17/11/2026). A integração online está implementada para um piloto, mas a documentação não certifica o funcionamento em produção. Consulte [o guia técnico](docs/GUIA-TECNICO.md) e [o roteiro de implantação](ONLINE.md).

## Problema e público

Uma professora precisa acompanhar alunos de matérias diferentes, organizar aulas, registrar notas e progresso e disponibilizar materiais e atividades. Os alunos precisam consultar seus compromissos e entregas com acesso individual.

O projeto atende à professora e aos alunos cadastrados. Responsáveis são registrados no cadastro do aluno; o código atual não oferece um perfil de acesso independente para responsáveis.

## Proposta e objetivo

Reunir agenda, cadastros, atividades, materiais, progresso e registros de mensalidades em um portal web, com permissões por perfil. O objetivo do piloto online é validar o uso da professora e dos alunos, a privacidade dos anexos e a recuperação dos dados antes da entrega final.

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

## Tecnologias e organização

- **ASP.NET Core / .NET 10**, Razor Pages antigas e portal atual em MVC com Razor Views.
- **Entity Framework Core**, SQLite na instalação local e PostgreSQL no modo online.
- **Supabase Storage** privado para anexos no modo online; **Dockerfile** e `vercel.json` para implantação do contêiner.
- Testes automatizados com **xUnit** e verificação em GitHub Actions.

O portal atual fica em `src/SistemaPedagogico.Web/Portal` e `Views/Portal`. O projeto `Core` contém entidades antigas, `Infrastructure` contém persistência/backup SQLite e `tests` contém os testes. Veja [o mapa de arquitetura](docs/ARQUITETURA.md).

## Executar localmente

1. Instale o **SDK .NET 10**.
2. No Windows, execute `Verificar-Tutoring.cmd` para compilar/testar e `Iniciar-Tutoring.cmd` para iniciar; ou use:

   ```bash
   dotnet test --configuration Release
   dotnet run --project src/SistemaPedagogico.Web --launch-profile http
   ```
3. Abra `http://localhost:5003` e, na primeira execução local, crie a conta da professora no próprio computador.

O portal usa `/portal`. O modo local ainda registra rotas Razor Pages da versão anterior e mantém **dois bancos SQLite**; veja [o roteiro local](LEIA-ME-TESTE.md). Esses dados não são transferidos automaticamente para o Supabase.

## Implantação e dados

O modo online é ativado por `DATABASE_URL` e exige outras variáveis no servidor, um projeto Supabase e bucket privado. A conta inicial online é provisionada por variáveis de ambiente. Siga [ONLINE.md](ONLINE.md) para instalação, gestão de segredos e backup. Nunca publique senhas, strings de conexão, bancos, anexos ou arquivos de backup no GitHub.

## Verificação e pendências

Os testes automatizados podem ser executados pelo comando acima; isso não substitui testes no ambiente hospedado. Antes da entrega, validar restauração em um **segundo projeto**, isolamento entre alunos, permissões do perfil Professor, celular, acessibilidade, formulários, erros e uso em redes diferentes. Decidir com a cliente o comportamento ao clicar fora de um cartão e se haverá ação para a série inteira.

## Documentação

- [Guia técnico e regras verificadas](docs/GUIA-TECNICO.md)
- [Arquitetura](docs/ARQUITETURA.md)
- [Implantação online e backup](ONLINE.md)
- [Roteiro da versão local](LEIA-ME-TESTE.md)
- [Histórico da versão 4.3](VERSAO-4.3.md)
