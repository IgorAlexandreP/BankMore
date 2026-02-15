namespace BankMore.Account.API.Application.Queries;

using MediatR;
using BankMore.Core.Shared;

public record GetBalanceQuery(string AccountId) : IRequest<Result<BalanceDto>>;

public record BalanceDto(int AccountNumber, string AccountName, DateTime Date, decimal Balance);
