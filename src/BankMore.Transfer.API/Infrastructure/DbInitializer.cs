namespace BankMore.Transfer.API.Infrastructure;

using Dapper;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Reflection;

public class DbInitializer
{
    private readonly string _connectionString;

    public DbInitializer(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' not found.");
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var tableExists = connection.ExecuteScalar<int>("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='transferencia';");
        
        if (tableExists == 0)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BankMore.Transfer.API.transferencia.sql";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException("Embedded resource not found.", resourceName);

            using var reader = new StreamReader(stream);
            var script = reader.ReadToEnd();
            connection.Execute(script);
        }
    }
}
