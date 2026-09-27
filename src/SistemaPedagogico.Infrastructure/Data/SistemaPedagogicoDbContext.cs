using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;

namespace SistemaPedagogico.Infrastructure.Data;

public class SistemaPedagogicoDbContext(DbContextOptions<SistemaPedagogicoDbContext> options)
    : DbContext(options)
{
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Responsavel> Responsaveis => Set<Responsavel>();
    public DbSet<AlunoResponsavel> AlunosResponsaveis => Set<AlunoResponsavel>();
    public DbSet<Disciplina> Disciplinas => Set<Disciplina>();
    public DbSet<Nota> Notas => Set<Nota>();
    public DbSet<TarefaEscolar> TarefasEscolares => Set<TarefaEscolar>();
    public DbSet<Acompanhamento> Acompanhamentos => Set<Acompanhamento>();
    public DbSet<Dificuldade> Dificuldades => Set<Dificuldade>();
    public DbSet<PlanoReforco> PlanosReforco => Set<PlanoReforco>();
    public DbSet<SessaoReforco> SessoesReforco => Set<SessaoReforco>();
    public DbSet<Exercicio> Exercicios => Set<Exercicio>();
    public DbSet<SessaoExercicio> SessoesExercicios => Set<SessaoExercicio>();
    public DbSet<EventoAgenda> EventosAgenda => Set<EventoAgenda>();
    public DbSet<Observacao> Observacoes => Set<Observacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (
            var relationship in modelBuilder
                .Model.GetEntityTypes()
                .SelectMany(x => x.GetForeignKeys())
        )
            relationship.DeleteBehavior = DeleteBehavior.Restrict;

        modelBuilder.Entity<Aluno>().HasIndex(x => x.Nome);
        modelBuilder.Entity<Disciplina>().HasIndex(x => x.Nome).IsUnique();
        modelBuilder
            .Entity<AlunoResponsavel>()
            .HasIndex(x => new { x.AlunoId, x.ResponsavelId })
            .IsUnique();
        modelBuilder.Entity<Nota>().HasIndex(x => new { x.AlunoId, x.Data });
        modelBuilder.Entity<Dificuldade>().HasIndex(x => new { x.AlunoId, x.Status });
        modelBuilder.Entity<EventoAgenda>().HasIndex(x => x.Inicio);

        modelBuilder.Entity<Nota>().Property(x => x.Valor).HasConversion<double>();
        modelBuilder.Entity<Nota>().Property(x => x.ValorMaximo).HasConversion<double>();
    }

    public override int SaveChanges()
    {
        AtualizarDatas();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AtualizarDatas();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AtualizarDatas()
    {
        var agora = DateTime.Now;
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CriadoEm = agora;
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.AtualizadoEm = agora;
        }
    }
}
