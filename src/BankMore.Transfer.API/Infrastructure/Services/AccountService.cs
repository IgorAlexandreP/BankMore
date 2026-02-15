namespace BankMore.Transfer.API.Infrastructure.Services;

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

public interface IAccountService
{
    Task<bool> DebitAsync(string token, string requestId, decimal amount);
    Task<bool> CreditAsync(string token, string requestId, string targetAccount, decimal amount);
    Task<bool> ReverseDebitAsync(string token, string requestId, decimal amount);
}

public class AccountService : IAccountService
{
    private readonly HttpClient _httpClient;

    public AccountService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> DebitAsync(string token, string requestId, decimal amount)
    {
        return await SendTransactionAsync(token, requestId, null, amount, "D");
    }

    public async Task<bool> CreditAsync(string token, string requestId, string targetAccount, decimal amount)
    {
        return await SendTransactionAsync(token, requestId, targetAccount, amount, "C");
    }

    public async Task<bool> ReverseDebitAsync(string token, string requestId, decimal amount)
    {
        return await SendTransactionAsync(token, requestId, null, amount, "C");
    }

    private async Task<bool> SendTransactionAsync(string token, string requestId, string? targetAccount, decimal amount, string type)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var payload = new
        {
            IdRequisicao = requestId,
            NumeroConta = targetAccount,
            Valor = amount,
            Tipo = type
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/conta/movimentacao", content);
        
        return response.IsSuccessStatusCode;
    }
}
