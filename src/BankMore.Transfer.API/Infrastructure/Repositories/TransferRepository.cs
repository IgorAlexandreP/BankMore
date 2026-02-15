namespace BankMore.Transfer.API.Infrastructure.Repositories;

using BankMore.Transfer.API.Domain;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

public interface ITransferRepository
{
    Task AddAsync(Transferencia transferencia);
    Task<bool> IsIdempotentAsync(string key);
    Task RegisterIdempotencyAsync(string key, string req, string res);
}

public class TransferRepository : ITransferRepository
{
    private readonly string _connectionString;

    public TransferRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' not found.");
    }
    
    private IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task AddAsync(Transferencia transferencia)
    {
        using var conn = CreateConnection();
        var sql = @"INSERT INTO transferencia (idtransferencia, idcontacorrente_origem, idcontacorrente_destino, datamovimento, valor) 
                    VALUES (@Id, @IdContaOrigem, @IdContaDestino, @DataMovimento, @Valor)";
        await conn.ExecuteAsync(sql, transferencia);
    }
    
    public async Task<bool> IsIdempotentAsync(string key)
    {
        using var conn = CreateConnection();
        var count = await conn.ExecuteScalarAsync<int>("SELECT count(*) FROM idempotencia WHERE chave_idempotencia = @Key", new { Key = key });
        return count > 0;
    }

    public async Task RegisterIdempotencyAsync(string key, string req, string res)
    {
        using var conn = CreateConnection();
        var sql = "INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado) VALUES (@Key, @Req, @Res)";
        await conn.ExecuteAsync(sql, new { Key = key, Req = req, Res = res });
    }
}
