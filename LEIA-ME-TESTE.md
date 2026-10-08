# EducaMais — versão para teste

Este pacote contém a versão 4. Foi compilado e verificado com 28 testes automatizados e carregamento das 13 áreas por HTTP. A conferência visual no Windows continua necessária. Veja `VERSAO-4.md` para novidades e roteiro de teste; para manter seus dados atuais, siga `ATUALIZAR-CADASTROS.md`.

## Começar

1. Instale o **SDK do .NET 10**, Windows x64: https://dotnet.microsoft.com/download/dotnet/10.0.
2. Extraia este ZIP em uma **pasta nova**, por exemplo `Documentos\GITS\EducaMais-teste`. Não substitua a pasta original.
3. Execute `Verificar-EducaMais.cmd`. Ele compila o projeto e executa os testes. Se falhar, envie `resultado-testes.txt` para revisão.
4. Execute `Iniciar-EducaMais.cmd` e aguarde `Now listening on: http://localhost:5003`.
5. Abra http://localhost:5003. Crie o primeiro administrador com nome, e-mail e senha de pelo menos 10 caracteres. Esse passo só funciona pelo próprio computador que executa o servidor.
6. Entre com sua conta. Não há senhas de demonstração predefinidas.

Encerre outra instância do app com Ctrl+C antes de iniciar a nova: ambas usam a porta 5003. A instalação inicial precisa de internet para baixar dependências; depois disso, o aplicativo pode funcionar localmente.

## Testar todos os perfis

Siga o roteiro de `VERSAO-4.md`. Cadastre matérias, professor e aluno; selecione matérias e dias pelas caixas de seleção. Defina professor inicial no cadastro ou vincule depois. Crie acessos selecionando nomes, sem informar IDs manualmente. Na agenda, escolha os horários disponíveis e use Remarcar na própria linha. Confira aluno/professor separadamente para validar permissões e entregas.

## Dados antigos e novos

- O novo banco é `educamais.db`, na mesma pasta do banco original `sistema-pedagogico.db`.
- A configuração original aponta para `..\Dados` a partir de `src\SistemaPedagogico.Web`: normalmente **`src\Dados`**. A localização pode mudar se você personalizou `Storage:DataDirectory` em `appsettings.json`.
- Uma pasta extraída nova começa sem seus dados anteriores. Para testar a importação, com ambos os apps fechados, copie o banco antigo para a pasta de dados configurada na versão nova **antes da primeira execução**. Preserve uma cópia segura; se houver arquivos `-wal` e `-shm`, faça o backup pelo aplicativo antigo em vez de copiar um banco em uso.
- Na primeira execução, o portal importa alunos, contato principal dos responsáveis, matérias, notas normalizadas para 0–10 e tarefas. Não cria contas automaticamente.
- Alunos importados precisam receber matérias e vínculo com professor antes do agendamento.
- Dificuldades, planos, sessões, observações, relatórios e agenda anteriores permanecem no banco original, acessíveis ao administrador pelo link **Histórico pedagógico anterior**.
- A importação ocorre uma vez. Alterações no histórico anterior não sincronizam com o novo portal, nem o contrário. Use o portal novo para novos registros.
- Arquivos enviados ficam em `Arquivos`, fora da pasta pública. A pasta `Chaves` guarda as chaves de sessão e não deve ser publicada.
- Backup do novo portal inclui os dois bancos e os anexos. Ao restaurar sem `Chaves`, as sessões anteriores serão invalidadas; os usuários e senhas permanecem no banco.

## Escopo e limitações

O escopo e as limitações da versão 4 estão em `VERSAO-4.md`. O aplicativo funciona localmente com .NET 10 e SQLite. Não contém senhas fixas de demonstração. O banco novo e o histórico anterior continuam separados após a importação inicial. Para atualização com dados existentes, siga `ATUALIZAR-CADASTROS.md`.

## Desenvolvimento

`dotnet watch --project src\SistemaPedagogico.Web`

Novos arquivos ficam em `Portal`, `Views/Portal`, `Views/Account`, `Views/Shared/_EducaLayout.cshtml`, `wwwroot/css/educamais.css` e `wwwroot/js/educamais.js`.

O projeto anterior continua em `Pages`. As alterações manuais de aparência feitas no seu computador depois do ZIP original não estão neste pacote; o novo portal usa sua própria folha de estilos.
