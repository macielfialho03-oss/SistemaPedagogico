# Atualizar para a versão 4.3 sem perder seus dados

1. Pare o aplicativo com Ctrl+C no terminal.
2. Faça uma cópia da pasta inteira do aplicativo. Ela deve incluir os dados; se configurou uma pasta externa de dados, copie essa pasta também.
3. Extraia `Tutoring-v4.3.zip` em outra pasta.
4. Copie `src/SistemaPedagogico.Web` do ZIP para o mesmo local no projeto atual, substituindo os arquivos de código.
5. Preserve sua configuração `Storage:DataDirectory` caso a tenha personalizado. Não exclua nem substitua `Dados`, `Arquivos`, `Chaves` ou os bancos. Mantenha a pasta `.git` do projeto.
6. Copie também a pasta `tests`, os arquivos de instruções e `Iniciar-Tutoring.cmd` / `Verificar-Tutoring.cmd` para o projeto atual.
7. Execute `Iniciar-Tutoring.cmd` na pasta atual. Abra http://localhost:5003 e use Ctrl+F5.
8. Entre com sua conta habitual. Se ainda não fez a unificação da versão 4.2, entre com a conta de professora OU administradora que deseja manter e confirme a conta única; os demais acessos da equipe serão desativados.
9. Confira “versão 4.3” no rodapé. Minha disponibilidade foi removida: escolha data e hora ao agendar.

Normalmente os cadastros ficam em `src/Dados`. Iniciar a cópia extraída em outra pasta usa outro banco inicialmente vazio. Para continuar com os mesmos registros, atualize o projeto atual conforme acima.

A cópia automática `Dados/Backups/tutoring-antes-v4.2-*.db` protege o banco do portal antes da unificação da conta. Ela não inclui anexos nem o outro banco e não substitui a cópia completa do passo 2.

Nenhum cadastro, histórico ou arquivo antigo é apagado pela atualização. O limite de 5 MB vale para novos envios. Mensalidades automáticas são geradas apenas no cadastro inicial do aluno, quando você escolhe essa opção.

A versão 4.3 faz uma atualização aditiva do banco e cria `Dados/Backups/tutoring-antes-v4.3-*.db` antes de adicionar as colunas de agrupamento. Essa cópia também não inclui anexos nem o outro banco. As regras de agrupamento das atividades antigas estão em `VERSAO-4.3.md`.
