using AppTodoList.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AppTodoList.Api.Tests;

public sealed class TareasApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _conexion = new("Data Source=:memory:");

    public TareasApiFactory()
    {
        _conexion.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<TareasDbContext>>();
            services.RemoveAll<TareasDbContext>();
            services.AddDbContext<TareasDbContext>(options => options.UseSqlite(_conexion));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _conexion.Dispose();
        }
    }
}
