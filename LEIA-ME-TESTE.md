# Tutoring — versão 4.3 para teste local

Se já utiliza o aplicativo, comece por `ATUALIZAR-CADASTROS.md` para manter seus cadastros.

## Iniciar pela primeira vez

1. Extraia o ZIP.
2. Instale o SDK .NET 10 se ainda não estiver instalado.
3. Execute `Iniciar-Tutoring.cmd`.
4. Aguarde a mensagem “Now listening on” e abra http://localhost:5003.
5. Crie a conta única da professora, que também administra o sistema.
6. Cadastre as matérias, os alunos e seus acessos. O horário é escolhido diretamente no agendamento.

Deixe o terminal aberto durante o uso. Para encerrar, pressione Ctrl+C.

## Roteiro de teste

- Cadastre um aluno sem escolher professora. Gere três mensalidades de R$ 250,50 com vencimento inicial em 31/01/2027: confira 31/01, 28/02, 31/03 e total R$ 751,50.
- Cadastre outro aluno sem mensalidades e confira que não aparecem parcelas.
- Selecione dois alunos da mesma matéria e agende uma aula em grupo. Confira o cartão único, os participantes e início/fim. Remarque o grupo e confira a nova data para ambos. Tente agendar outra no mesmo horário e verifique o bloqueio.
- Publique uma atividade para dois alunos: confira um único cartão e abra a página de entregas. Envie a resposta como um aluno e corrija como professora; o outro aluno deve continuar pendente.
- Entre como aluno: confira que não há agendamento nem remarcação. Apenas seus registros ficam disponíveis.
- Envie documentos e imagens válidos de até 5 MB. Tente vídeo, vídeo renomeado ou arquivo maior e confira a recusa.
- Na conta da professora, confira notas, progresso, materiais, atividades, mensalidades e backup.

Para executar os testes automatizados, use `Verificar-Tutoring.cmd`.

## Dados

SQLite local, normalmente em `src/Dados`. Não envie bancos, anexos ou chaves de sessão para o GitHub. O ZIP não contém dados ou senhas de demonstração. Faça backups regulares e guarde uma cópia fora do computador.

A publicação online fica para a próxima etapa. Nada foi contratado ou publicado nesta versão.
