namespace BankMore.Account.API.Infrastructure;

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

        var tableExists = connection.ExecuteScalar<int>("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='contacorrente';");
        
        if (tableExists == 0)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BankMore.Account.API.contacorrente.sql";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException("Embedded resource not found.", resourceName);

            using var reader = new StreamReader(stream);
            var script = reader.ReadToEnd();
            connection.Execute(script);
        }
        else
        {
            var movimentoExists = connection.ExecuteScalar<int>("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='movimento';");
            if (movimentoExists == 0)
            {
                var createMovimentoSql = @"
                    CREATE TABLE movimento (
                    	idmovimento TEXT(37) PRIMARY KEY,
                    	idcontacorrente TEXT(37) NOT NULL,
                    	datamovimento TEXT(25) NOT NULL,
                    	tipomovimento TEXT(1) NOT NULL,
                    	valor REAL NOT NULL,
                    	FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente),
                    	CHECK (tipomovimento in ('C','D'))
                    );
                    CREATE TABLE idempotencia (
                    	chave_idempotencia TEXT(37) PRIMARY KEY,
                    	requisicao TEXT(1000),
                    	resultado TEXT(1000)
                    );";
                 connection.Execute(createMovimentoSql);
            }
        }
    }
}
