# Requisitos e critérios de aceitação — Tutoring

Este documento organiza os pedidos da cliente e as funcionalidades observadas no código da `main` em `69ce4fe` (17/11/2026). A integração publicada é acessível pela [página de entrada do Tutoring](https://sistemapedagogico.vercel.app/conta/entrar?ReturnUrl=%2Fportal), mas abrir a página não comprova os fluxos após o login. **Presente no código** e **validado no site** são estados diferentes.

## Contexto e atores

O Tutoring atende uma professora de reforço e acompanhamento escolar e seus alunos. A professora cadastra responsáveis como contatos dos alunos; responsável não possui perfil de login. O escopo acadêmico original prevê uma única professora administradora. O código atual também permite criar perfis adicionais de Admin e Professor: essa divergência precisa de decisão antes da versão final.

## Requisitos funcionais

| ID | O sistema deve permitir | Situação no código | Critério de aceitação |
| --- | --- | --- | --- |
| RF01 | Autenticar contas individuais de equipe e alunos. | Presente | Cada conta entra com e-mail e senha; aluno não abre páginas administrativas. |
| RF02 | Cadastrar e atualizar alunos, contato do responsável, serviço e matérias. | Presente | O aluno aparece nos filtros e pode ser associado às suas matérias. |
| RF03 | Cadastrar e ativar/inativar matérias. | Presente | Somente matérias ativas são escolhidas nos novos agendamentos. |
| RF04 | Agendar aula individual, para vários alunos ou para os alunos ativos de uma matéria. | Presente | Cada aluno escolhido vê a própria participação; conflito de horários é recusado. |
| RF05 | Criar repetições semanais de uma aula. | Presente, até 52 ocorrências | As ocorrências aparecem nas semanas solicitadas, com os participantes selecionados na criação. |
| RF06 | Remarcar ou cancelar uma ocorrência inteira para o grupo, registrando justificativa no cancelamento. | Presente | Todas as participações ainda agendadas da ocorrência são afetadas; histórico não é apagado. |
| RF07 | Remarcar ou cancelar toda a série semanal em uma ação. | Pendente | A professora consegue selecionar a série, rever o impacto e aplicar a alteração a todas as ocorrências futuras elegíveis. |
| RF08 | Publicar atividades para um ou vários alunos, receber respostas e corrigir individualmente. | Presente | Cada resposta, anexo, nota e feedback permanece associado ao aluno certo. |
| RF09 | Publicar materiais individuais ou gerais e permitir download autorizado. | Presente | Um aluno acessa seus materiais e os gerais; não acessa materiais individuais de outro aluno. |
| RF10 | Registrar notas, frequência, progresso e mensalidades. | Presente | A professora consulta o histórico por aluno; mensalidade é registro, sem cobrança online. |
| RF11 | Provisionar conta inicial online, criar acesso para aluno, trocar e recuperar senha. | Presente | Professora troca a própria senha; recuperação administrativa invalida sessões anteriores. |
| RF12 | Exportar dados e criar cópia do PostgreSQL e dos anexos com restauração separada. | CSV e script presentes | A restauração em projeto vazio reproduz registros e anexos; **ainda não há teste registrado**. |

## Requisitos não funcionais

| ID | Qualidade esperada | Evidência atual e validação |
| --- | --- | --- |
| RNF01 | Privacidade e autorização por perfil. | Há checagens no servidor e bucket privado. Testar acesso cruzado, inclusive redefinição/desativação de contas por Professor. |
| RNF02 | Proteção de senha e sessão. | Hash de senha, cookie HTTP-only e bloqueio após cinco falhas por 15 minutos no código. Testar em redes diferentes. |
| RNF03 | Comunicação segura e segredos fora do código. | Cookie exige HTTPS no modo online e variáveis de ambiente guardam chaves. Conferir configuração da hospedagem. |
| RNF04 | Compatibilidade com celular e desktop. | Há regras responsivas; testar formulários e agenda em larguras reais. |
| RNF05 | Acessibilidade e clareza de erros. | Conferir teclado, foco, rótulos, contraste, leitor de tela e mensagens de falha. |
| RNF06 | Recuperação dos dados. | Existe `scripts/online-backup.py`; testar exportação, cópia externa e restauração em ambiente separado. |
| RNF07 | Acesso independente e simultâneo. | Arquitetura na Vercel/Supabase; testar professora e alunos ao mesmo tempo em redes distintas. |
| RNF08 | Arquivos dentro dos limites da hospedagem. | Upload atual passa pelo servidor, limitado a 4 MiB; envio direto para recuperar 5 MB é melhoria futura. |

## Regras de negócio

| ID | Regra atual |
| --- | --- |
| RN01 | Ao criar aula por matéria, selecionar alunos **ativos já cadastrados** nessa matéria; a seleção não se atualiza retroativamente. |
| RN02 | Uma aula pode ter 1 a 200 participantes e 1 a 52 ocorrências semanais. |
| RN03 | Conflitos com aulas não canceladas são recusados; remarcação preserva participantes da ocorrência. |
| RN04 | Cancelar exige justificativa; presença e conclusão são individuais. |
| RN05 | A publicação da atividade é agrupada, mas a entrega/correção é individual. |
| RN06 | Material geral é visível a todos os alunos; material individual exige vínculo. |
| RN07 | Anexo permitido: PDF, DOCX, XLSX, PPTX, TXT, PNG, JPG ou JPEG, até 4 MiB; vídeos são recusados. |
| RN08 | Responsáveis são contato cadastral, sem autenticação própria; não há cadastro público de aluno. |

## Histórias de usuário

**HU01 — Cadastro.** Como professora, quero cadastrar aluno, responsável, serviço e matérias para organizar seu atendimento. Aceite: os dados salvos aparecem na ficha e o aluno pode ser selecionado em aulas e atividades compatíveis.

**HU02 — Agenda por matéria.** Como professora, quero marcar uma aula de matemática na segunda-feira para todos os alunos ativos de matemática. Aceite: cada elegível recebe a ocorrência e um conflito de horário impede salvar uma aula incompatível. Alunos inscritos posteriormente não entram nas ocorrências já criadas.

**HU03 — Repetição.** Como professora, quero repetir a aula por um número definido de semanas. Aceite: o sistema cria de 1 a 52 ocorrências nas semanas indicadas; uma alteração de grupo afeta apenas a ocorrência escolhida na versão atual.

**HU04 — Cancelamento.** Como professora, quero cancelar uma ocorrência do grupo justificando o motivo. Aceite: todos os participantes ainda agendados nessa ocorrência ficam cancelados com a mesma justificativa.

**HU05 — Atividade.** Como professora, quero publicar uma atividade para vários alunos e acompanhar respostas individuais. Aceite: corrigir uma entrega não modifica a situação ou os arquivos de outro participante.

**HU06 — Acesso do aluno.** Como aluno, quero consultar minha agenda, notas, materiais, progresso e atividades. Aceite: minha conta mostra apenas registros autorizados e permite enviar minha resposta.

**HU07 — Conta da professora.** Como professora, quero receber uma conta inicial e trocar minha senha. Aceite: a nova senha permite entrar, a antiga deixa de funcionar e a recuperação administrativa exige acesso às variáveis privadas do servidor.

## Verificação antes da entrega

Os critérios acima precisam de teste no site publicado com contas e dados de teste. Prioridades: acesso cruzado entre alunos e professores, agendamento semanal, anexos, celular, mensagens de erro, sessão em duas redes e restauração real do backup em outro Supabase. Os detalhes de implantação estão em [ONLINE.md](../ONLINE.md) e as regras implementadas em [GUIA-TECNICO.md](GUIA-TECNICO.md).

**Decisões pendentes:** confirmar se a professora precisa cancelar/remarcar a série inteira, comportamento ao clicar fora do cartão e manter ou remover criação de novos perfis de equipe. Esses itens não devem ser apresentados como concluídos.
