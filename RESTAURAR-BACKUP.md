# Recuperar um backup do Tutoring

1. Encerre todas as instâncias do aplicativo (Ctrl+C no terminal).
2. Localize a pasta de dados configurada em src/SistemaPedagogico.Web/appsettings.json. A configuração padrão usa src/Dados.
3. Faça uma cópia completa da pasta Dados atual e guarde separadamente. Não a apague.
4. Extraia o ZIP de backup em uma pasta nova e confira educamais.db, sistema-pedagogico.db e Arquivos (se havia anexos).
5. Na pasta Dados atual, substitua os bancos pelos bancos do backup e substitua a pasta Arquivos pela versão do backup. Com o aplicativo encerrado, mova eventuais arquivos .db-wal e .db-shm junto para a cópia de segurança; não os deixe ao lado dos bancos restaurados.
6. Preserve a pasta Chaves atual. As chaves de sessão não estão no ZIP de backup; em outra máquina será necessário entrar novamente.
7. Inicie Iniciar-Tutoring.cmd. Confira um aluno, uma mensalidade, uma atividade e um download de material.

A recuperação volta ao estado do backup: alterações feitas depois dele não estarão nessa cópia. A versão 4.1 foi testada extraindo um backup com registros e anexos e verificando a integridade do banco restaurado.
