namespace BankMore.Account.API.Application.Commands;

using MediatR;
using BankMore.Core.Shared;

public record CreateAccountCommand(string Cpf, string Nome, string Senha) : IRequest<Result<int>>;
