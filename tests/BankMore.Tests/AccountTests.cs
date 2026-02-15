namespace BankMore.Tests;

extern alias AccountApi;
using AccountApi::BankMore.Account.API.Domain;
using BankMore.Core.Domain;
using Xunit;

public class AccountTests
{
    [Fact]
    public void CriarConta_DeveGerarNumeroEId_Corretamente()
    {
        var cpfValido = "12345678901";
        var nomeTitular = "Maria Silva";
        
        var conta = ContaCorrente.Create(cpfValido, nomeTitular, "hash_senha_segura", "salt_aleatorio");
        
        Assert.NotNull(conta);
        Assert.False(string.IsNullOrEmpty(conta.Id), "O ID da conta não deveria ser nulo ou vazio");
        Assert.Equal(cpfValido, conta.Cpf);
        Assert.True(conta.Numero > 0, "O número da conta deve ser positivo");
        Assert.True(conta.Ativo, "A conta deve nascer ativa");
    }

    [Fact]
    public void Cpf_DeveValidar_QuandoFormatoCorreto()
    {
        var cpfValido = "52998224725"; 
        
        var resultado = Cpf.Create(cpfValido);
        
        Assert.True(resultado.IsSuccess, "Deveria aceitar um CPF válido");
    }

    [Fact]
    public void Cpf_DeveFalhar_QuandoTodosDigitosIguais()
    {
        var cpfInvalido = "11111111111";

        var resultado = Cpf.Create(cpfInvalido);

        Assert.False(resultado.IsSuccess, "Não deveria aceitar CPF com todos dígitos iguais");
    }
}
