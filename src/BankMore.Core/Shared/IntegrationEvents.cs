namespace BankMore.Core.Shared;

public record TransferenciaRealizadaEvent(string IdRequisicao, string IdContaCorrente, decimal Valor, DateTime DataHora);
public record TarifaCalculadaEvent(string IdRequisicao, string IdContaCorrente, decimal Valor, DateTime DataHora);
