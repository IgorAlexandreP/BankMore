namespace BankMore.Account.API.Application.Commands;

using MediatR;
using BankMore.Core.Shared;

public record MakeTransactionCommand(
    string RequestId,
    string? AccountNumber,
    decimal Value,
    string Type,
    string AuthenticatedAccountId 
) : IRequest<Result>;
