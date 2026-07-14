namespace Backups.Domain.Tests;

using System.Linq;
using Backups.Core.Application.UseCases;
using NetArchTest.Rules;

/// <summary>
/// Verifica automáticamente el límite de la arquitectura hexagonal: el núcleo
/// (Backups.Core: dominio, casos de uso y puertos) no debe conocer ninguna clase
/// de infraestructura (adaptadores de compresión, red o UI).
/// </summary>
public class ArchitectureTests
{
    private static readonly string[] EnsambladosDeInfraestructuraProhibidos =
    {
        "Backups.Infrastructure.Compression",
        "Backups.Infrastructure.Network",
        "Backups.Adapters.Infrastructure",
        "Backups.Adapters.UI.WinForms",
        "Backups.UI",
    };

    [Fact]
    public void Core_NoDebeDependerDeEnsambladosDeInfraestructura()
    {
        var resultado = Types.InAssembly(typeof(EjecutarRespaldoUseCase).Assembly)
            .Should()
            .NotHaveDependencyOnAny(EnsambladosDeInfraestructuraProhibidos)
            .GetResult();

        Assert.True(
            resultado.IsSuccessful,
            "Backups.Core depende de infraestructura en: " +
            string.Join(", ", resultado.FailingTypeNames ?? Enumerable.Empty<string>()));
    }
}
