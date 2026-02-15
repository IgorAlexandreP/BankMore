namespace BankMore.Account.API.Application.Commands;

using MediatR;
using BankMore.Core.Shared;

public record InactivateAccountCommand(string AccountId, string Senha) : IRequest<Result>;
