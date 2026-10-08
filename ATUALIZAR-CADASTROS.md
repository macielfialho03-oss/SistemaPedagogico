# Atualizar para a versão 4 sem perder seus dados

1. Pare o aplicativo atual com Ctrl+C no terminal.
2. Faça uma cópia de segurança da pasta inteira do aplicativo.
3. Extraia o ZIP da versão 4 em outra pasta.
4. Copie a pasta `src/SistemaPedagogico.Web` do ZIP novo para o mesmo lugar na pasta do aplicativo atual, substituindo todos os arquivos dessa pasta.
5. Preserve sua configuração `Storage:DataDirectory` se você a personalizou. Não exclua nem substitua nenhuma pasta `Dados`, os bancos, `Arquivos` ou `Chaves`.
6. Copie também os arquivos de instruções e a pasta `tests` se quiser executar os testes atualizados.
7. Inicie pelo `Iniciar-EducaMais.cmd` da pasta atual e atualize o navegador com Ctrl+F5.
8. Confira “versão 4” no rodapé.

Normalmente seus cadastros ficam em `src/Dados`. Extrair tudo em outra pasta e iniciar lá abre outro banco, inicialmente vazio. Para continuar com os mesmos registros, atualize os arquivos na pasta atual conforme os passos acima.

O aplicativo acrescenta os campos da versão 4 ao banco automaticamente. Antes de atualizar um banco existente em disco para a V4, também salva uma cópia de `educamais.db` em `Dados/Backups`. Essa cópia automática não inclui anexos nem substitui a cópia completa do passo 2. Não retorne à versão anterior após começar a gravar os novos campos sem restaurar um backup compatível.

Cadastros anteriores ficam inicialmente como Reforço escolar. Edite o tipo de atendimento quando necessário. Notas antigas recebem a origem Avaliação da professora: confira e diferencie os novos lançamentos. Nenhum texto de progresso anterior é apagado.

Nome e sobrenome são exigidos ao salvar alunos e professores e quando o responsável estiver preenchido. Nascimento continua opcional; quando preenchido, o aluno deve ter no mínimo 4 anos. Corrija registros antigos fora dessas regras antes de editar. A geração de mensalidades aparece somente em novos cadastros; editar um aluno existente não gera parcelas.
