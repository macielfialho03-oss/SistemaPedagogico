using System.ComponentModel.DataAnnotations;

namespace SistemaPedagogico.Core.Entities;

public abstract class Entity
{
    public int Id { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime AtualizadoEm { get; set; } = DateTime.Now;
}

public enum StatusDificuldade
{
    Identificada,
    EmAcompanhamento,
    EmReforco,
    Superada,
    Arquivada,
}

public enum StatusPlanoReforco
{
    Planejado,
    EmAndamento,
    Concluido,
    Suspenso,
}

public enum StatusTarefa
{
    Pendente,
    EmAndamento,
    Concluida,
    Atrasada,
}

public enum StatusAgenda
{
    Agendado,
    Realizado,
    Cancelado,
}

public class Aluno : Entity
{
    [Required, StringLength(120)]
    public string Nome { get; set; } = "";

    [DataType(DataType.Date)]
    public DateTime? DataNascimento { get; set; }

    [StringLength(120)]
    public string? Escola { get; set; }

    [StringLength(40)]
    public string? SerieAno { get; set; }

    [StringLength(30)]
    public string? Turno { get; set; }

    [StringLength(1000)]
    public string? InformacoesGerais { get; set; }
    public bool Ativo { get; set; } = true;
    public ICollection<AlunoResponsavel> Responsaveis { get; set; } = [];
    public ICollection<Nota> Notas { get; set; } = [];
    public ICollection<TarefaEscolar> Tarefas { get; set; } = [];
    public ICollection<Acompanhamento> Acompanhamentos { get; set; } = [];
    public ICollection<Dificuldade> Dificuldades { get; set; } = [];
    public ICollection<PlanoReforco> PlanosReforco { get; set; } = [];
    public ICollection<EventoAgenda> Eventos { get; set; } = [];
    public ICollection<Observacao> Observacoes { get; set; } = [];
}

public class Responsavel : Entity
{
    [Required, StringLength(120)]
    public string Nome { get; set; } = "";

    [StringLength(40)]
    public string? Telefone { get; set; }

    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }
    public bool Ativo { get; set; } = true;
    public ICollection<AlunoResponsavel> Alunos { get; set; } = [];
}

public class AlunoResponsavel : Entity
{
    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;
    public int ResponsavelId { get; set; }
    public Responsavel Responsavel { get; set; } = null!;

    [Required, StringLength(50)]
    public string Parentesco { get; set; } = "Responsável";
    public bool ContatoPrincipal { get; set; }
}

public class Disciplina : Entity
{
    [Required, StringLength(80)]
    public string Nome { get; set; } = "";

    [StringLength(300)]
    public string? Descricao { get; set; }

    [StringLength(10)]
    public string Cor { get; set; } = "#4f46e5";
    public bool Ativa { get; set; } = true;
}

public class Nota : Entity
{
    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;
    public int DisciplinaId { get; set; }
    public Disciplina Disciplina { get; set; } = null!;

    [Required, StringLength(60)]
    public string Periodo { get; set; } = "";

    [Required, StringLength(80)]
    public string TipoAvaliacao { get; set; } = "Avaliação";

    [Range(0, 1000)]
    public decimal Valor { get; set; }

    [Range(0.01, 1000)]
    public decimal ValorMaximo { get; set; } = 10;

    [DataType(DataType.Date)]
    public DateTime Data { get; set; } = DateTime.Today;

    [StringLength(500)]
    public string? Observacao { get; set; }
    public decimal Percentual => ValorMaximo <= 0 ? 0 : Math.Round(Valor / ValorMaximo * 100, 1);
}

public class TarefaEscolar : Entity
{
    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;
    public int? DisciplinaId { get; set; }
    public Disciplina? Disciplina { get; set; }

    [Required, StringLength(150)]
    public string Titulo { get; set; } = "";

    [StringLength(1000)]
    public string? Descricao { get; set; }

    [DataType(DataType.Date)]
    public DateTime Prazo { get; set; } = DateTime.Today;
    public StatusTarefa Status { get; set; } = StatusTarefa.Pendente;
    public DateTime? ConcluidaEm { get; set; }
}

public class Acompanhamento : Entity
{
    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;

    [DataType(DataType.Date)]
    public DateTime Data { get; set; } = DateTime.Today;

    [Required, StringLength(80)]
    public string Tipo { get; set; } = "Rotina escolar";

    [Required, StringLength(2000)]
    public string Resumo { get; set; } = "";

    [StringLength(500)]
    public string? Desempenho { get; set; }

    [StringLength(1000)]
    public string? ProximosPassos { get; set; }
    public ICollection<Dificuldade> DificuldadesIdentificadas { get; set; } = [];
}

public class Dificuldade : Entity
{
    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;
    public int DisciplinaId { get; set; }
    public Disciplina Disciplina { get; set; } = null!;
    public int? AcompanhamentoOrigemId { get; set; }
    public Acompanhamento? AcompanhamentoOrigem { get; set; }

    [Required, StringLength(150)]
    public string Conteudo { get; set; } = "";

    [Required, StringLength(1500)]
    public string Descricao { get; set; } = "";

    [StringLength(30)]
    public string Nivel { get; set; } = "Moderada";
    public StatusDificuldade Status { get; set; } = StatusDificuldade.Identificada;

    [DataType(DataType.Date)]
    public DateTime IdentificadaEm { get; set; } = DateTime.Today;
    public DateTime? SuperadaEm { get; set; }

    [StringLength(1000)]
    public string? Observacao { get; set; }
    public ICollection<PlanoReforco> Planos { get; set; } = [];
}

public class PlanoReforco : Entity
{
    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;
    public int DificuldadeId { get; set; }
    public Dificuldade Dificuldade { get; set; } = null!;
    public int DisciplinaId { get; set; }
    public Disciplina Disciplina { get; set; } = null!;

    [Required, StringLength(150)]
    public string Titulo { get; set; } = "";

    [Required, StringLength(1500)]
    public string Objetivo { get; set; } = "";

    [StringLength(1500)]
    public string? Estrategia { get; set; }

    [DataType(DataType.Date)]
    public DateTime DataInicio { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime? DataPrevista { get; set; }
    public StatusPlanoReforco Status { get; set; } = StatusPlanoReforco.Planejado;

    [StringLength(1000)]
    public string? Observacoes { get; set; }
    public ICollection<SessaoReforco> Sessoes { get; set; } = [];
}

public class SessaoReforco : Entity
{
    public int PlanoReforcoId { get; set; }
    public PlanoReforco PlanoReforco { get; set; } = null!;
    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;
    public DateTime DataHora { get; set; } = DateTime.Now;

    [Range(1, 600)]
    public int DuracaoMinutos { get; set; } = 60;

    [Required, StringLength(1000)]
    public string ConteudoTrabalhado { get; set; } = "";

    [StringLength(1500)]
    public string? Resumo { get; set; }

    [StringLength(500)]
    public string? Avaliacao { get; set; }

    [StringLength(1000)]
    public string? ProximosPassos { get; set; }
    public ICollection<SessaoExercicio> Exercicios { get; set; } = [];
    public decimal Aproveitamento =>
        Exercicios.Sum(x => x.Acertos + x.Erros) == 0
            ? 0
            : Math.Round(
                (decimal)Exercicios.Sum(x => x.Acertos)
                    / Exercicios.Sum(x => x.Acertos + x.Erros)
                    * 100,
                1
            );
}

public class Exercicio : Entity
{
    public int DisciplinaId { get; set; }
    public Disciplina Disciplina { get; set; } = null!;

    [Required, StringLength(150)]
    public string Conteudo { get; set; } = "";

    [Required, StringLength(2000)]
    public string Enunciado { get; set; } = "";

    [StringLength(60)]
    public string Tipo { get; set; } = "Questão";

    [StringLength(30)]
    public string Nivel { get; set; } = "Médio";

    [StringLength(1000)]
    public string? RespostaEsperada { get; set; }
    public bool Ativo { get; set; } = true;
}

public class SessaoExercicio : Entity
{
    public int SessaoReforcoId { get; set; }
    public SessaoReforco SessaoReforco { get; set; } = null!;
    public int ExercicioId { get; set; }
    public Exercicio Exercicio { get; set; } = null!;

    [Range(0, 10000)]
    public int Acertos { get; set; }

    [Range(0, 10000)]
    public int Erros { get; set; }

    [StringLength(500)]
    public string? Observacao { get; set; }
    public decimal Aproveitamento =>
        Acertos + Erros == 0 ? 0 : Math.Round((decimal)Acertos / (Acertos + Erros) * 100, 1);
}

public class EventoAgenda : Entity
{
    public int? AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    [Required, StringLength(150)]
    public string Titulo { get; set; } = "";

    [StringLength(1000)]
    public string? Descricao { get; set; }

    [StringLength(60)]
    public string Tipo { get; set; } = "Atendimento";
    public DateTime Inicio { get; set; } = DateTime.Now;
    public DateTime? Termino { get; set; }
    public StatusAgenda Status { get; set; } = StatusAgenda.Agendado;
}

public class Observacao : Entity
{
    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;

    [Required, StringLength(60)]
    public string Categoria { get; set; } = "Geral";

    [Required, StringLength(3000)]
    public string Texto { get; set; } = "";

    [DataType(DataType.Date)]
    public DateTime Data { get; set; } = DateTime.Today;
    public bool Destaque { get; set; }
}
