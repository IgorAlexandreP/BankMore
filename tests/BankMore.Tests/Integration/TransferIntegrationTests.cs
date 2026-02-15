extern alias AccountApi;
extern alias TransferApi;
using BankMore.Tests.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AccountProgram = AccountApi::Program;
using TransferProgram = TransferApi::Program;

namespace BankMore.Tests.Integration;

public class TransferIntegrationTests : IClassFixture<WebApplicationFactory<TransferProgram>>, IClassFixture<WebApplicationFactory<AccountProgram>>
{
    private readonly WebApplicationFactory<TransferProgram> _transferFactory;
    private readonly WebApplicationFactory<AccountProgram> _accountFactory;
    private readonly HttpClient _transferClient;
    private readonly HttpClient _accountClient;

    public TransferIntegrationTests(WebApplicationFactory<TransferProgram> transferFactory, WebApplicationFactory<AccountProgram> accountFactory)
    {
        _accountFactory = accountFactory;
        _accountClient = _accountFactory.CreateClient();

        _transferFactory = transferFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.ConfigureAll<HttpClientFactoryOptions>(options =>
                {
                    options.HttpMessageHandlerBuilderActions.Add(builder =>
                    {
                        builder.PrimaryHandler = _accountFactory.Server.CreateHandler();
                    });
                });
            });
        });
        
        _transferClient = _transferFactory.CreateClient();
    }

    [Fact]
    public async Task Transfer_ShouldReturnSuccess_WhenBalanceIsSufficient()
    {
        var (originAccountNumber, originToken) = await CreateAndLoginAccountAsync("User Origin");
        var (destinationAccountNumber, _) = await CreateAndLoginAccountAsync("User Destination");

        await CreditAccountAsync(originToken, 1000.00);

        var transferCommand = new 
        { 
            IdRequisicao = Guid.NewGuid().ToString(),
            ContaDestino = destinationAccountNumber,
            Valor = 100.00
        };

        _transferClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", originToken);

        var transferResponse = await _transferClient.PostAsJsonAsync("/api/transferencia", transferCommand);
        
        if (!transferResponse.IsSuccessStatusCode)
        {
             var error = await transferResponse.Content.ReadAsStringAsync();
             throw new HttpRequestException($"Falha na transferência: {transferResponse.StatusCode} - {error}");
        }
        transferResponse.EnsureSuccessStatusCode();
    }

    private async Task<(string AccountNumber, string Token)> CreateAndLoginAccountAsync(string userName)
    {
        var cpf = CpfGenerator.Generate();
        var password = "Password123!";
        
        var createCommand = new { Cpf = cpf, Nome = userName, Senha = password };
        var createResponse = await _accountClient.PostAsJsonAsync("/api/conta/cadastro", createCommand);
        
        if (!createResponse.IsSuccessStatusCode)
        {
             var error = await createResponse.Content.ReadAsStringAsync();
             throw new HttpRequestException($"Falha ao criar conta para {userName}: {createResponse.StatusCode} - {error}");
        }

        var createResult = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accountNumber = createResult.GetProperty("accountNumber").ToString();

        var loginCommand = new { Login = cpf, Senha = password };
        var loginResponse = await _accountClient.PostAsJsonAsync("/api/conta/login", loginCommand);
        
        if (!loginResponse.IsSuccessStatusCode)
        {
             var error = await loginResponse.Content.ReadAsStringAsync();
             throw new HttpRequestException($"Falha no login para {userName}: {loginResponse.StatusCode} - {error}");
        }

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = loginResult.GetProperty("token").GetString();

        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException($"Token nulo ou vazio para {userName}");

        return (accountNumber, token);
    }

    private async Task CreditAccountAsync(string token, double amount)
    {
        _accountClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var creditCommand = new { IdRequisicao = Guid.NewGuid().ToString(), Valor = amount, Tipo = "C" };
        var creditResponse = await _accountClient.PostAsJsonAsync("/api/conta/movimentacao", creditCommand);
        
        if (!creditResponse.IsSuccessStatusCode)
        {
             var error = await creditResponse.Content.ReadAsStringAsync();
             throw new HttpRequestException($"Falha no crédito: {creditResponse.StatusCode} - {error}");
        }
    }
}
