namespace BankMore.Transfer.API.Application.Commands;

using MediatR;
using BankMore.Core.Shared;

public record MakeTransferCommand(
    string RequestId,
    string TargetAccount,
    decimal Value,
    string Token,
    string AuthenticatedAccountId
) : IRequest<Result>;
