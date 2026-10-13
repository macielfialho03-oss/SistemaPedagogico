# Tutoring — Reforço e Acompanhamento Escolar · versão 4.1

## O que mudou
- Nome e identidade do produto atualizados; bancos e identificadores internos preservados para compatibilidade.
- Interface por perfil: professor sem cadastro de aluno; aluno sem ações administrativas ou remarcação/cancelamento de aulas passadas.
- Matrícula em cinco etapas com conferência, formatação de nome/telefone, vínculo inicial e geração de mensalidades sem duplicação.
- Ficha do aluno com contatos, professores, aulas, avaliações, atividades, materiais e evolução. Financeiro somente para administrador.
- Agenda com semana anterior/próxima para a equipe, próximas aulas para o aluno, histórico e registro de presença/falta.
- Seleções com busca integrada, navegação por teclado e opções compatíveis.
- Formulários em janelas com cancelamento, layout responsivo e preservação do preenchimento após erros na mesma aba. Senhas e arquivos não ficam em rascunho.
- Notas com decimal brasileiro, edição/exclusão autorizadas, filtros e “Salvar e lançar outra”. Professor só altera suas próprias notas.
- Atividades com cartões, filtros por situação, seleção múltipla e identificação clara de envio/atualização da resposta. Entregas atrasadas continuam permitidas e têm datas registradas.
- Materiais com cartões e visualização de PDF, imagens e TXT; demais arquivos são baixados.
- Progresso com blocos por atendimento, referência do último registro, vínculo opcional com aula concluída, edição e exclusão autorizadas.
- Anotações internas do progresso separadas das observações compartilhadas. Campos antigos continuam compartilhados, como eram antes; revise o conteúdo antes de usar dados reais.
- Relatório de progresso respeita filtros e omite anotações internas.
- Mensalidades com filtros, edição de valor/vencimento e desfazer pagamento mediante motivo; histórico preservado.
- Painel administrativo com aulas de hoje e pendências; médias de notas separadas por matéria e origem.
- Backup completo com data da última geração e instruções de recuperação.

## Atualização
Encerre o aplicativo e faça um backup antes de atualizar. Preserve sua pasta Dados (normalmente src/Dados), incluindo bancos, Arquivos e Chaves. A atualização adiciona campos sem apagar os registros e gera uma cópia antes da alteração do banco da V4. Não copie dados de teste por cima dos seus.

## Verificação
Execute Verificar-Tutoring.cmd. A entrega foi compilada e validada com testes automatizados, requisições HTTP e navegador nos perfis administrador, professor e aluno em tela grande e celular. O backup foi extraído e seu banco verificado, com cadastros, mensalidades e anexos recuperados.

## Regras mantidas
Agendamento pelo aluno continua permitido apenas com professores vinculados e horários futuros disponíveis. Alterações não passam por aprovação da professora nesta versão. Não foram adicionados pagamentos online, notificações externas ou envio de e-mail. A versão ainda precisa do teste de aceitação da cliente antes do uso definitivo.
