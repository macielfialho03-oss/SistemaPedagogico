# Tutoring — versão 4.1

Versão de teste inspirada no protótipo Tutoring, construída sobre o projeto Caderno Pedagógico com ASP.NET Core, .NET 10 e SQLite.

**Comece por [LEIA-ME-TESTE.md](LEIA-ME-TESTE.md)**: instalação, roteiro de teste, importação dos dados e limitações.

1. Instale o SDK .NET 10.
2. Execute `Verificar-Tutoring.cmd` para compilar e rodar os testes.
3. Execute `Iniciar-Tutoring.cmd`.
4. Abra http://localhost:5003 e crie o administrador.

Validação da versão 4.1: compilação .NET/Razor, **39 testes automatizados**, 28 páginas dos três perfis por HTTP e testes de navegador em desktop e celular. Matrícula por etapas, notas com vírgula e lançamento contínuo, preservação após erro e recuperação de backup foram verificadas com dados sintéticos. Veja [VERSAO-4.1.md](VERSAO-4.1.md).

## Recursos

- Perfis de administrador, professor e aluno com permissões no servidor.
- Cadastros de alunos/responsáveis, professores, matérias e contas.
- Aulas com disponibilidade, prevenção de conflitos, cancelamento, remarcação e frequência.
- Notas, atividades com entrega e correção, materiais e histórico de progresso.
- Registro de mensalidades e pagamentos, CSV e backup completo.
- Preservação do histórico pedagógico anterior em uma área exclusiva do administrador.

## Executar pelo terminal

```bat
dotnet run --project src\SistemaPedagogico.Web --launch-profile http
```

## Dados e atualizações

O portal usa `educamais.db` ao lado do banco original. Os dados anteriores são importados uma vez; não há sincronização posterior. Não apague bancos para atualizar o programa. A versão 4.1 atualiza o banco existente automaticamente, preservando o histórico. Faça uma cópia da pasta Dados antes de atualizar.

Não versionar bancos, anexos ou chaves de sessão. O código não contém credenciais fixas de demonstração.
