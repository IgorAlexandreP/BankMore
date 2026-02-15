namespace BankMore.Transfer.API.Domain;

using BankMore.Core.Domain;

public class Transferencia : Entity
{
    public string Id { get; private set; }
    public string IdContaOrigem { get; private set; }
    public string IdContaDestino { get; private set; }
    public string DataMovimento { get; private set; }
    public decimal Valor { get; private set; }

    public Transferencia(string id, string idContaOrigem, string idContaDestino, string dataMovimento, decimal valor)
    {
        Id = id;
        IdContaOrigem = idContaOrigem;
        IdContaDestino = idContaDestino;
        DataMovimento = dataMovimento;
        Valor = valor;
    }
    
    public static Transferencia Create(string origem, string destino, decimal valor)
    {
        return new Transferencia(
            Guid.NewGuid().ToString(),
            origem,
            destino,
            DateTime.Now.ToString("dd/MM/yyyy"),
            valor
        );
    }
}
