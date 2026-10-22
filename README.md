# Tutoring — versão 4.3

Versão de teste inspirada no protótipo Tutoring, construída sobre o projeto Caderno Pedagógico com ASP.NET Core, .NET 10 e SQLite.

**Comece por [LEIA-ME-TESTE.md](LEIA-ME-TESTE.md)**: instalação, roteiro de teste, importação dos dados e limitações.

1. Instale o SDK .NET 10.
2. Execute `Verificar-Tutoring.cmd` para compilar e rodar os testes.
3. Execute `Iniciar-Tutoring.cmd`.
4. Abra http://localhost:5003 e crie a conta da professora.

Veja as mudanças e regras de migração em [VERSAO-4.3.md](VERSAO-4.3.md).

## Recursos

- Conta única da professora (administração e ensino) e acessos individuais dos alunos.
- Cadastros de alunos/responsáveis, matérias e contas.
- Aulas individuais ou em grupo, escolha direta de horário, prevenção de conflitos, cancelamento, remarcação e frequência.
- Notas, atividades agrupadas com página de entregas e correção individual, materiais e histórico de progresso.
- Registro de mensalidades e pagamentos, CSV e backup completo.
- Preservação do histórico pedagógico anterior em uma área exclusiva da professora.

## Executar pelo terminal

```bat
dotnet run --project src\SistemaPedagogico.Web --launch-profile http
```

## Dados e atualizações

O portal usa `educamais.db` ao lado do banco original. Os dados anteriores são importados uma vez; não há sincronização posterior. Não apague bancos para atualizar o programa. A versão 4.3 atualiza o banco existente automaticamente, preservando o histórico. Faça uma cópia da pasta Dados antes de atualizar.

Não versionar bancos, anexos ou chaves de sessão. O código não contém credenciais fixas de demonstração.

Validação: 65 testes automatizados aprovados e fluxos conferidos em navegador desktop e celular, com dados de teste.
