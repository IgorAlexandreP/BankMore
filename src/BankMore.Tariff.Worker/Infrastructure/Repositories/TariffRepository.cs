namespace BankMore.Tariff.Worker.Infrastructure.Repositories;

using BankMore.Tariff.Worker.Domain;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

public interface ITariffRepository
{
    Task AddAsync(Tarifa tarifa);
}

public class TariffRepository : ITariffRepository
{
    private readonly string _connectionString;

    public TariffRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' not found.");
    }
    
    private IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task AddAsync(Tarifa tarifa)
    {
        using var conn = CreateConnection();
        var sql = @"INSERT INTO tarifa (idtarifa, idcontacorrente, valor, datamovimento) 
                    VALUES (@Id, @IdContaCorrente, @Valor, @Data)";
        await conn.ExecuteAsync(sql, new { tarifa.Id, tarifa.IdContaCorrente, tarifa.Valor, Data = tarifa.Data.ToString("dd/MM/yyyy HH:mm:ss") });
    }
}
