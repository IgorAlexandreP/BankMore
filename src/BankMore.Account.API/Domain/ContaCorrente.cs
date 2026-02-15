namespace BankMore.Account.API.Domain;

using BankMore.Core.Domain;

public class ContaCorrente : Entity
{
    public string Id { get; private set; }
    public int Numero { get; private set; }
    public string Nome { get; private set; }
    public string Cpf { get; private set; }
    public bool Ativo { get; private set; }
    public string Senha { get; private set; }
    public string Salt { get; private set; }

    public ContaCorrente(string id, int numero, string nome, string cpf, bool ativo, string senha, string salt)
    {
        Id = id;
        Numero = numero;
        Nome = nome;
        Cpf = cpf;
        Ativo = ativo;
        Senha = senha;
        Salt = salt;
    }

    public static ContaCorrente Create(string cpf, string nome, string senhaHash, string salt)
    {
        var random = new Random();
        var numero = random.Next(1000, 999999);
        
        return new ContaCorrente(
            Guid.NewGuid().ToString(),
            numero,
            nome,
            cpf,
            true,
            senhaHash,
            salt
        );
    }

    public void Inactivate()
    {
        Ativo = false;
    }
}
