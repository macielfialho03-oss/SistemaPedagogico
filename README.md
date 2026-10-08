# EducaMais — versão 4

Versão de teste inspirada no protótipo EducaMais, construída sobre o projeto Caderno Pedagógico com ASP.NET Core, .NET 10 e SQLite.

**Comece por [LEIA-ME-TESTE.md](LEIA-ME-TESTE.md)**: instalação, roteiro de teste, importação dos dados e limitações.

1. Instale o SDK .NET 10.
2. Execute `Verificar-EducaMais.cmd` para compilar e rodar os testes.
3. Execute `Iniciar-EducaMais.cmd`.
4. Abra http://localhost:5003 e crie o administrador.

Validação da versão 4: compilação .NET/Razor aprovada, 28 testes automatizados aprovados e carregamento HTTP das 13 áreas do portal após login. A aparência e as interações devem ser conferidas no Windows; o navegador remoto não alcança o servidor local deste ambiente.

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

O portal usa `educamais.db` ao lado do banco original. Os dados anteriores são importados uma vez; não há sincronização posterior. Não apague bancos para atualizar o programa. A versão 4 atualiza o banco existente automaticamente, preservando o histórico. Faça uma cópia da pasta Dados antes de atualizar.

Não versionar bancos, anexos ou chaves de sessão. O código não contém credenciais fixas de demonstração.
