extern alias AccountApi;
using AccountApi::BankMore.Account.API.Application.Commands;
using BankMore.Tests.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AccountProgram = AccountApi::Program;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace BankMore.Tests.Integration;

public class AccountIntegrationTests : IClassFixture<WebApplicationFactory<AccountProgram>>
{
    private readonly WebApplicationFactory<AccountProgram> _factory;
    private readonly HttpClient _client;

    public AccountIntegrationTests(WebApplicationFactory<AccountProgram> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateAccount_ShouldReturnOk_WhenDataIsValid()
    {
        var command = new CreateAccountCommand(CpfGenerator.Generate(), "Igor Alexandre", "SenhaForte123!");

        var response = await _client.PostAsJsonAsync("/api/conta/cadastro", command);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        
        var accNumber = result.GetProperty("accountNumber");
        Assert.NotEqual(JsonValueKind.Undefined, accNumber.ValueKind);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var cpf = CpfGenerator.Generate();
        var createCommand = new CreateAccountCommand(cpf, "Thais Xavier", "SenhaSegura456!");
        await _client.PostAsJsonAsync("/api/conta/cadastro", createCommand);

        var loginCommand = new LoginCommand(cpf, "SenhaSegura456!");

        var response = await _client.PostAsJsonAsync("/api/conta/login", loginCommand);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        
        var tokenProp = result.GetProperty("token");
        Assert.NotEqual(JsonValueKind.Undefined, tokenProp.ValueKind);
    }

    [Fact]
    public async Task Inactivate_ShouldReturnNoContent_WhenTokenIsValid()
    {
        var cpf = CpfGenerator.Generate();
        var createCommand = new CreateAccountCommand(cpf, "Alice Adriana", "MinhaSenha789!");
        await _client.PostAsJsonAsync("/api/conta/cadastro", createCommand);

        var loginCommand = new LoginCommand(cpf, "MinhaSenha789!");
        var loginResponse = await _client.PostAsJsonAsync("/api/conta/login", loginCommand);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        
        var token = loginResult.GetProperty("token").GetString();
        Assert.NotNull(token);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var inactivateRequest = new { Senha = "MinhaSenha789!" };

        var response = await _client.PostAsJsonAsync("/api/conta/inativar", inactivateRequest);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetBalance_ShouldReturnOk_WhenTokenIsValid()
    {
        var cpf = CpfGenerator.Generate();
        var createCommand = new CreateAccountCommand(cpf, "Alfredo Norberto", "SenhaAna1010!");
        await _client.PostAsJsonAsync("/api/conta/cadastro", createCommand);

        var loginCommand = new LoginCommand(cpf, "SenhaAna1010!");
        var loginResponse = await _client.PostAsJsonAsync("/api/conta/login", loginCommand);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        
        var token = loginResult.GetProperty("token").GetString();
        Assert.NotNull(token);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/conta/saldo");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        
        var saldo = result.GetProperty("saldo").ToString();
        Assert.Equal("0,00", saldo);
    }

    [Fact]
    public async Task Transaction_ShouldReturnNoContent_WhenCreditIsSuccessful()
    {
        var cpf = CpfGenerator.Generate();
        var createCommand = new CreateAccountCommand(cpf, "Rafael Fabio", "SenhaRoberto2020!");
        await _client.PostAsJsonAsync("/api/conta/cadastro", createCommand);

        var loginCommand = new LoginCommand(cpf, "SenhaRoberto2020!");
        var loginResponse = await _client.PostAsJsonAsync("/api/conta/login", loginCommand);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        
        var token = loginResult.GetProperty("token").GetString();
        Assert.NotNull(token);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var transactionRequest = new 
        { 
            IdRequisicao = Guid.NewGuid().ToString(),
            Valor = 100.50, 
            Tipo = "C" 
        };

        var response = await _client.PostAsJsonAsync("/api/conta/movimentacao", transactionRequest);

        response.EnsureSuccessStatusCode();
    }
}