using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaPedagogico.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alunos",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Escola = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    SerieAno = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    Turno = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    InformacoesGerais = table.Column<string>(
                        type: "TEXT",
                        maxLength: 1000,
                        nullable: true
                    ),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alunos", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Disciplinas",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Cor = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disciplinas", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Responsaveis",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Observacoes = table.Column<string>(
                        type: "TEXT",
                        maxLength: 500,
                        nullable: true
                    ),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsaveis", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Acompanhamentos",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Resumo = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Desempenho = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ProximosPassos = table.Column<string>(
                        type: "TEXT",
                        maxLength: 1000,
                        nullable: true
                    ),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acompanhamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Acompanhamentos_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "EventosAgenda",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Inicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Termino = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosAgenda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosAgenda_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Observacoes",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Texto = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Destaque = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Observacoes_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Exercicios",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisciplinaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Conteudo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Enunciado = table.Column<string>(
                        type: "TEXT",
                        maxLength: 2000,
                        nullable: false
                    ),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Nivel = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    RespostaEsperada = table.Column<string>(
                        type: "TEXT",
                        maxLength: 1000,
                        nullable: true
                    ),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercicios_Disciplinas_DisciplinaId",
                        column: x => x.DisciplinaId,
                        principalTable: "Disciplinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Notas",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisciplinaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Periodo = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    TipoAvaliacao = table.Column<string>(
                        type: "TEXT",
                        maxLength: 80,
                        nullable: false
                    ),
                    Valor = table.Column<double>(type: "REAL", nullable: false),
                    ValorMaximo = table.Column<double>(type: "REAL", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notas_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Notas_Disciplinas_DisciplinaId",
                        column: x => x.DisciplinaId,
                        principalTable: "Disciplinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "TarefasEscolares",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisciplinaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Prazo = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ConcluidaEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarefasEscolares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TarefasEscolares_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_TarefasEscolares_Disciplinas_DisciplinaId",
                        column: x => x.DisciplinaId,
                        principalTable: "Disciplinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "AlunosResponsaveis",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponsavelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Parentesco = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ContatoPrincipal = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlunosResponsaveis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlunosResponsaveis_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_AlunosResponsaveis_Responsaveis_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Responsaveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Dificuldades",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisciplinaId = table.Column<int>(type: "INTEGER", nullable: false),
                    AcompanhamentoOrigemId = table.Column<int>(type: "INTEGER", nullable: true),
                    Conteudo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(
                        type: "TEXT",
                        maxLength: 1500,
                        nullable: false
                    ),
                    Nivel = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IdentificadaEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SuperadaEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Observacao = table.Column<string>(
                        type: "TEXT",
                        maxLength: 1000,
                        nullable: true
                    ),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dificuldades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dificuldades_Acompanhamentos_AcompanhamentoOrigemId",
                        column: x => x.AcompanhamentoOrigemId,
                        principalTable: "Acompanhamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Dificuldades_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Dificuldades_Disciplinas_DisciplinaId",
                        column: x => x.DisciplinaId,
                        principalTable: "Disciplinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "PlanosReforco",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DificuldadeId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisciplinaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Objetivo = table.Column<string>(type: "TEXT", maxLength: 1500, nullable: false),
                    Estrategia = table.Column<string>(
                        type: "TEXT",
                        maxLength: 1500,
                        nullable: true
                    ),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataPrevista = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacoes = table.Column<string>(
                        type: "TEXT",
                        maxLength: 1000,
                        nullable: true
                    ),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosReforco", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanosReforco_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_PlanosReforco_Dificuldades_DificuldadeId",
                        column: x => x.DificuldadeId,
                        principalTable: "Dificuldades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_PlanosReforco_Disciplinas_DisciplinaId",
                        column: x => x.DisciplinaId,
                        principalTable: "Disciplinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "SessoesReforco",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlanoReforcoId = table.Column<int>(type: "INTEGER", nullable: false),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    ConteudoTrabalhado = table.Column<string>(
                        type: "TEXT",
                        maxLength: 1000,
                        nullable: false
                    ),
                    Resumo = table.Column<string>(type: "TEXT", maxLength: 1500, nullable: true),
                    Avaliacao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ProximosPassos = table.Column<string>(
                        type: "TEXT",
                        maxLength: 1000,
                        nullable: true
                    ),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessoesReforco", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessoesReforco_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_SessoesReforco_PlanosReforco_PlanoReforcoId",
                        column: x => x.PlanoReforcoId,
                        principalTable: "PlanosReforco",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "SessoesExercicios",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessaoReforcoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExercicioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Acertos = table.Column<int>(type: "INTEGER", nullable: false),
                    Erros = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessoesExercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessoesExercicios_Exercicios_ExercicioId",
                        column: x => x.ExercicioId,
                        principalTable: "Exercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_SessoesExercicios_SessoesReforco_SessaoReforcoId",
                        column: x => x.SessaoReforcoId,
                        principalTable: "SessoesReforco",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Acompanhamentos_AlunoId",
                table: "Acompanhamentos",
                column: "AlunoId"
            );

            migrationBuilder.CreateIndex(name: "IX_Alunos_Nome", table: "Alunos", column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_AlunosResponsaveis_AlunoId_ResponsavelId",
                table: "AlunosResponsaveis",
                columns: new[] { "AlunoId", "ResponsavelId" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_AlunosResponsaveis_ResponsavelId",
                table: "AlunosResponsaveis",
                column: "ResponsavelId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Dificuldades_AcompanhamentoOrigemId",
                table: "Dificuldades",
                column: "AcompanhamentoOrigemId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Dificuldades_AlunoId_Status",
                table: "Dificuldades",
                columns: new[] { "AlunoId", "Status" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Dificuldades_DisciplinaId",
                table: "Dificuldades",
                column: "DisciplinaId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Disciplinas_Nome",
                table: "Disciplinas",
                column: "Nome",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_EventosAgenda_AlunoId",
                table: "EventosAgenda",
                column: "AlunoId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_EventosAgenda_Inicio",
                table: "EventosAgenda",
                column: "Inicio"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Exercicios_DisciplinaId",
                table: "Exercicios",
                column: "DisciplinaId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Notas_AlunoId_Data",
                table: "Notas",
                columns: new[] { "AlunoId", "Data" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Notas_DisciplinaId",
                table: "Notas",
                column: "DisciplinaId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Observacoes_AlunoId",
                table: "Observacoes",
                column: "AlunoId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlanosReforco_AlunoId",
                table: "PlanosReforco",
                column: "AlunoId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlanosReforco_DificuldadeId",
                table: "PlanosReforco",
                column: "DificuldadeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlanosReforco_DisciplinaId",
                table: "PlanosReforco",
                column: "DisciplinaId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_SessoesExercicios_ExercicioId",
                table: "SessoesExercicios",
                column: "ExercicioId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_SessoesExercicios_SessaoReforcoId",
                table: "SessoesExercicios",
                column: "SessaoReforcoId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_SessoesReforco_AlunoId",
                table: "SessoesReforco",
                column: "AlunoId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_SessoesReforco_PlanoReforcoId",
                table: "SessoesReforco",
                column: "PlanoReforcoId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_TarefasEscolares_AlunoId",
                table: "TarefasEscolares",
                column: "AlunoId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_TarefasEscolares_DisciplinaId",
                table: "TarefasEscolares",
                column: "DisciplinaId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AlunosResponsaveis");

            migrationBuilder.DropTable(name: "EventosAgenda");

            migrationBuilder.DropTable(name: "Notas");

            migrationBuilder.DropTable(name: "Observacoes");

            migrationBuilder.DropTable(name: "SessoesExercicios");

            migrationBuilder.DropTable(name: "TarefasEscolares");

            migrationBuilder.DropTable(name: "Responsaveis");

            migrationBuilder.DropTable(name: "Exercicios");

            migrationBuilder.DropTable(name: "SessoesReforco");

            migrationBuilder.DropTable(name: "PlanosReforco");

            migrationBuilder.DropTable(name: "Dificuldades");

            migrationBuilder.DropTable(name: "Acompanhamentos");

            migrationBuilder.DropTable(name: "Disciplinas");

            migrationBuilder.DropTable(name: "Alunos");
        }
    }
}
