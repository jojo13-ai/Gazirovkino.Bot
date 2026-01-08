using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Gazirovkino.Bot.Data;

public class GazirovkinoDbContextFactory : IDesignTimeDbContextFactory<GazirovkinoDbContext>
{
    public GazirovkinoDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("GazirovkinoDb");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'GazirovkinoDb' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<GazirovkinoDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new GazirovkinoDbContext(optionsBuilder.Options);
    }
}
