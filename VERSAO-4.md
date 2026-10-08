# EducaMais — versão 4

## Novidades

- Rodapé identificado como versão 4.
- Nomes de alunos, responsáveis e professores com espaços normalizados e iniciais maiúsculas, sem inventar acentos. Nome e sobrenome obrigatórios para alunos/professores e para responsáveis quando preenchidos.
- Nascimento limitado a alunos com pelo menos 4 anos. Campo permanece opcional.
- Pré-escola entre os níveis de ensino.
- Atendimento do aluno: Reforço escolar, Acompanhamento escolar ou Ambos.
- Professor inicial opcional no cadastro; vínculos adicionais continuam disponíveis na área de alunos.
- Mensalidades opcionais durante o cadastro: quantidade de meses, valor e primeiro vencimento. Geradas uma vez, dentro da mesma transação do cadastro, com proteção contra reenvio do formulário. Meses curtos usam o último dia disponível mantendo o dia original nos meses seguintes.
- Forma de pagamento registrada opcionalmente somente ao informar que uma mensalidade foi paga.
- Botões de ação organizados e edição em janelas, confirmação de exclusão com nome e botão vermelho. Exclusão de aluno continua restrita a cadastros sem histórico.
- Matérias com busca, filtro ativa/inativa, professores associados, quantidade de alunos e ação de ativar/desativar preservando histórico.
- Agenda sem ID manual: botão Remarcar na própria aula. Opções de 30, 60, 90 e 120 minutos; horários livres calculados a cada seleção, com validação de conflitos ao salvar. Observação separada do link de aula online. Dias da semana escritos por nome.
- Seleções pedagógicas por nome com busca e filtros de aluno, professor vinculado e matérias compatíveis.
- Contas com perfil primeiro, cadastro vinculado por nome, preenchimento de nome/e-mail, botão Criar acesso em alunos/professores, geração/visualização de senha e ações separadas de editar, redefinir senha e ativar/desativar acesso.
- Atividades para um ou mais alunos, distinguindo tarefa da escola e proposta da professora. Enunciado aceita texto, arquivo ou ambos; aluno baixa o enunciado e entrega resposta/anexo. Atividade pode ser editada antes de receber uma entrega. Uma cópia individual é criada para cada aluno.
- Materiais com seleções compatíveis, arquivo identificado por nome/tamanho, remoção da seleção e tipo do arquivo na lista. Continua sendo publicação individual.
- Progresso estruturado: objetivos, conteúdo/estratégias, evolução e próximos passos. No reforço, dificuldade identificada; no acompanhamento, tarefas escolares e autonomia. Histórico por aluno com opção de imprimir/salvar PDF pelo navegador.
- Notas distinguem Nota da escola e Avaliação da professora.
- Botão principal do cabeçalho acompanha a área aberta.

## Verificação

Compilação .NET 10 / Razor aprovada. 28 testes automatizados aprovados, incluindo migração de banco V3, proteção contra mensalidades duplicadas, idade mínima, nome completo, anexo para vários alunos, disponibilidade de horários e progresso estruturado. Login e 13 áreas carregaram por HTTP. Conferência visual e uso no Windows ainda precisam ser feitos pelo usuário.

## Roteiro rápido

1. Confira versão 4 no rodapé.
2. Cadastre professor com nome completo, telefone, matérias e dias disponíveis.
3. Cadastre aluno com nome completo e nascimento de pelo menos 4 anos, atendimento Ambos, matérias, professor inicial e 3 mensalidades de R$ 200.
4. Confira 3 parcelas em Mensalidades. Edite o aluno e confirme que não foram criadas novas parcelas.
5. Na Agenda, selecione aluno/professor/matéria/dia e escolha um horário. Use Remarcar na linha da aula.
6. Crie acessos a partir dos cadastros. Use contas fictícias para testar perfis.
7. Publique atividade com somente um PDF, para mais de um aluno com matéria em comum; entre como aluno, baixe e entregue; entre como professor e corrija.
8. Registre um progresso de Reforço e outro de Acompanhamento para o aluno com Ambos. Confira os campos diferentes e o histórico.
9. Lance uma nota da escola e outra de avaliação da professora.
10. Teste edição em janela, filtros de matérias, materiais, registro de pagamento e backup.

Limites: sem cobrança online, notificações, envio de e-mail, turmas ou calendário semanal em grade. Histórico de progresso e notas não têm edição/exclusão geral nesta versão. O administrador redefine senhas. O relatório do progresso usa a impressão do navegador. Para materiais, o professor é determinado automaticamente pelo usuário conectado; para atividades em lote, a edição posterior é individual.
