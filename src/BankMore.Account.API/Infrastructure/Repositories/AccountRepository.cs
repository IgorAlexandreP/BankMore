namespace BankMore.Account.API.Infrastructure.Repositories;

using BankMore.Account.API.Domain;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

public interface IAccountRepository
{
    Task AddAsync(ContaCorrente account);
    Task UpdateAsync(ContaCorrente account);
    Task<ContaCorrente?> GetByIdAsync(string id);
    Task<ContaCorrente?> GetByNumberAsync(int number);
    Task<ContaCorrente?> GetByCpfAsync(string cpf);
    Task AddMovimentoAsync(Movimento movimento);
    Task<IEnumerable<Movimento>> GetMovimentosAsync(string accountId);
    Task<bool> IsIdempotentAsync(string key);
    Task RegisterIdempotencyAsync(string key, string req, string res);
}

public class AccountRepository : IAccountRepository
{
    private readonly string _connectionString;

    public AccountRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' not found.");
    }
    
    private IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

    public async Task AddAsync(ContaCorrente account)
    {
        using var conn = CreateConnection();
        var sql = @"INSERT INTO contacorrente (idcontacorrente, numero, nome, cpf, ativo, senha, salt) 
                    VALUES (@Id, @Numero, @Nome, @Cpf, @Ativo, @Senha, @Salt)";
        await conn.ExecuteAsync(sql, new { 
            account.Id, account.Numero, account.Nome, account.Cpf, Ativo = account.Ativo ? 1 : 0, account.Senha, account.Salt 
        });
    }

    public async Task<ContaCorrente?> GetByIdAsync(string id)
    {
        using var conn = CreateConnection();
        var sql = "SELECT idcontacorrente as Id, numero, nome, cpf, ativo, senha, salt FROM contacorrente WHERE idcontacorrente = @Id";
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });
        
        if (result == null) return null;
        
        return new ContaCorrente(result.Id, (int)result.numero, result.nome, result.cpf, result.ativo == 1, result.senha, result.salt);
    }

    public async Task<ContaCorrente?> GetByNumberAsync(int number)
    {
        using var conn = CreateConnection();
        var sql = "SELECT idcontacorrente as Id, numero, nome, cpf, ativo, senha, salt FROM contacorrente WHERE numero = @Numero";
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { Numero = number });
        
        if (result == null) return null;
        
        return new ContaCorrente(result.Id, (int)result.numero, result.nome, result.cpf, result.ativo == 1, result.senha, result.salt);
    }

    public async Task<ContaCorrente?> GetByCpfAsync(string cpf)
    {
        using var conn = CreateConnection();
        var sql = "SELECT idcontacorrente as Id, numero, nome, cpf, ativo, senha, salt FROM contacorrente WHERE cpf = @Cpf";
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { Cpf = cpf });
        
        if (result == null) return null;
        
        return new ContaCorrente(result.Id, (int)result.numero, result.nome, result.cpf, result.ativo == 1, result.senha, result.salt);
    }
    
    public async Task UpdateAsync(ContaCorrente account)
    {
        using var conn = CreateConnection();
        var sql = "UPDATE contacorrente SET ativo = @Ativo WHERE idcontacorrente = @Id";
        await conn.ExecuteAsync(sql, new { Ativo = account.Ativo ? 1 : 0, account.Id });
    }

    public async Task AddMovimentoAsync(Movimento movimento)
    {
        using var conn = CreateConnection();
        var sql = @"INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor) 
                    VALUES (@Id, @IdContaCorrente, @DataMovimento, @TipoMovimento, @Valor)";
        await conn.ExecuteAsync(sql, movimento);
    }

    public async Task<IEnumerable<Movimento>> GetMovimentosAsync(string accountId)
    {
        using var conn = CreateConnection();
        var sql = "SELECT idmovimento as Id, idcontacorrente as IdContaCorrente, datamovimento as DataMovimento, tipomovimento as TipoMovimento, valor FROM movimento WHERE idcontacorrente = @Id";
        var result = await conn.QueryAsync<dynamic>(sql, new { Id = accountId });
        return result.Select(m => new Movimento((string)m.Id, (string)m.IdContaCorrente, (string)m.DataMovimento, (string)m.TipoMovimento, (decimal)(double)m.valor));
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
