using SistemaPedagogico.Core.Entities;

namespace SistemaPedagogico.Tests;

public class CalculosPedagogicosTests
{
    [Fact]
    public void Nota_calcula_percentual()
    {
        var nota = new Nota { Valor = 7.5m, ValorMaximo = 10m };
        Assert.Equal(75m, nota.Percentual);
    }

    [Fact]
    public void Resultado_calcula_aproveitamento()
    {
        var resultado = new SessaoExercicio { Acertos = 8, Erros = 2 };
        Assert.Equal(80m, resultado.Aproveitamento);
    }

    [Fact]
    public void Sessao_agrega_resultados()
    {
        var sessao = new SessaoReforco
        {
            Exercicios = [new() { Acertos = 4, Erros = 1 }, new() { Acertos = 3, Erros = 2 }],
        };
        Assert.Equal(70m, sessao.Aproveitamento);
    }
}
