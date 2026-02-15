namespace BankMore.Tariff.Worker.Domain;

using BankMore.Core.Domain;

public class Tarifa : Entity
{
    public string Id { get; private set; }
    public string IdContaCorrente { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime Data { get; private set; }

    public Tarifa(string id, string idContaCorrente, decimal valor, DateTime data)
    {
        Id = id;
        IdContaCorrente = idContaCorrente;
        Valor = valor;
        Data = data;
    }

    public static Tarifa Create(string idContaCorrente, decimal valor)
    {
        return new Tarifa(Guid.NewGuid().ToString(), idContaCorrente, valor, DateTime.Now);
    }
}
