namespace BankMore.Account.API.Controllers;

using BankMore.Account.API.Application.Commands;
using BankMore.Account.API.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/conta")]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("cadastro")]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateAccountCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Error, type = result.ErrorType });
        
        return Ok(new { accountNumber = result.Value });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return Unauthorized(new { message = result.Error, type = result.ErrorType });
        
        return Ok(new { token = result.Value });
    }

    [HttpPost("inativar")]
    [Authorize]
    public async Task<IActionResult> Inactivate([FromBody] InactivateAccountRequest request)
    {
        var accountId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(accountId)) return Forbid();

        var command = new InactivateAccountCommand(accountId, request.Senha);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
             return BadRequest(new { message = result.Error, type = result.ErrorType });
        }
            
        return NoContent();
    }
    
    [HttpPost("movimentacao")]
    [Authorize]
    public async Task<IActionResult> Transaction([FromBody] TransactionRequest request)
    {
        var accountId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(accountId)) return Forbid();
        
        var command = new MakeTransactionCommand(
            request.IdRequisicao,
            request.NumeroConta,
            request.Valor,
            request.Tipo,
            accountId
        );

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error, type = result.ErrorType });
        }

        return NoContent();
    }
    
    [HttpGet("saldo")]
    [Authorize]
    public async Task<IActionResult> GetBalance()
    {
        var accountId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(accountId)) return Forbid();

        var query = new GetBalanceQuery(accountId);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
             return BadRequest(new { message = result.Error, type = result.ErrorType });

        return Ok(new {
             numeroConta = result.Value.AccountNumber,
             nomeTitular = result.Value.AccountName,
             dataHora = result.Value.Date,
             saldo = result.Value.Balance.ToString("N2", new System.Globalization.CultureInfo("pt-BR"))
        });
    }
}

public record InactivateAccountRequest(string Senha);
public record TransactionRequest(string IdRequisicao, string? NumeroConta, decimal Valor, string Tipo);
