# Tutoring — versão 4.3

## Agenda

- Minha disponibilidade foi removida. Data e hora são escolhidas diretamente, sem depender dos dias e horários da configuração antiga.
- Uma aula pode ter vários alunos ativos com uma matéria em comum. O sistema recusa sobreposição com outra aula não cancelada.
- O grupo aparece em um único cartão. A presença e o cancelamento de participação são individuais; Cancelar grupo cancela todos os participantes ainda agendados.
- Remarcar grupo mantém os participantes e atualiza a data e horário do grupo inteiro. Participações canceladas continuam canceladas. Aulas iniciadas ou com presença registrada não podem ser remarcadas.
- O aluno vê somente sua participação e não agenda nem remarca aulas.

## Atividades

- Uma publicação para vários alunos gera um cartão compacto, com prazo e resumo das entregas.
- Ao abrir o cartão, a professora vê o enunciado e os alunos pendentes, entregues para corrigir ou já corrigidos. Busca e filtros ajudam a localizar cada aluno.
- Cada resposta, anexo, nota e feedback permanece individual. O aluno acessa somente sua própria entrega.
- É possível editar o enunciado para todos antes da primeira entrega. Depois disso, o enunciado fica preservado.
- Publicações novas recebem identificadores próprios: atividades publicadas separadamente não são unidas apenas por terem o mesmo título.
- Documentos e imagens continuam limitados a 5 MB por arquivo novo, sem vídeos.

## Preservação dos dados

A atualização acrescenta colunas de agrupamento. Não elimina registros, respostas, notas ou anexos antigos. Aulas anteriores continuam individuais: não é seguro deduzir grupos apenas pelo horário.

Para organizar atividades antigas, a migração reúne registros com a mesma professora, matéria, título, descrição, prazo, origem e anexo de enunciado. Mantém cada aluno no máximo uma vez em cada grupo: repetições para o mesmo aluno ficam em grupos distintos. Essa correspondência é uma aproximação para o histórico; identificadores, respostas e correções de cada destinatário são preservados. A migração executa uma única vez.

Faça uma cópia completa dos dados antes de atualizar, conforme ATUALIZAR-CADASTROS.md. A cópia automática do banco do portal não substitui esse backup completo.

## Verificação e escopo

65 testes automatizados aprovados, incluindo conflitos, remarcação em grupo, presença individual, privacidade de entregas e migração do histórico. Fluxos conferidos em navegador desktop e largura de celular com dados de teste.

Continua sendo uma versão local com SQLite. Hospedagem, acesso remoto e contratação de serviços ficam para a próxima etapa.
