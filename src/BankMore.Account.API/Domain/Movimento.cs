namespace BankMore.Account.API.Domain;

using BankMore.Core.Domain;

public class Movimento : Entity
{
    public string Id { get; private set; }
    public string IdContaCorrente { get; private set; }
    public string DataMovimento { get; private set; }
    public string TipoMovimento { get; private set; }
    public decimal Valor { get; private set; }

    public Movimento(string id, string idContaCorrente, string dataMovimento, string tipoMovimento, decimal valor)
    {
        Id = id;
        IdContaCorrente = idContaCorrente;
        DataMovimento = dataMovimento;
        TipoMovimento = tipoMovimento;
        Valor = valor;
    }

    public Movimento() 
    { 
        Id = string.Empty;
        IdContaCorrente = string.Empty;
        DataMovimento = string.Empty;
        TipoMovimento = string.Empty;
    }

    public static Movimento Create(string idConta, string tipo, decimal valor)
    {
        return new Movimento(
            Guid.NewGuid().ToString(),
            idConta,
            DateTime.Now.ToString("dd/MM/yyyy"),
            tipo,
            valor
        );
    }
}
