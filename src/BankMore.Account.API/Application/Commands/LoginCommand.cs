namespace BankMore.Account.API.Application.Commands;

using MediatR;
using BankMore.Core.Shared;

public record LoginCommand(string Login, string Senha) : IRequest<Result<string>>;
