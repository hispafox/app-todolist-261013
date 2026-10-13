using AppTodoList.Api.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppTodoList.Api.Services;

public sealed class DatabaseHealthCheck(TareasDbContext contexto) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var disponible = await contexto.Database.CanConnectAsync(cancellationToken);

        return disponible
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy();
    }
}
