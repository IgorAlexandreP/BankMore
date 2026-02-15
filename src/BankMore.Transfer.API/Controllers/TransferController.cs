namespace BankMore.Transfer.API.Controllers;

using BankMore.Transfer.API.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/transferencia")]
public class TransferController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransferController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        var accountId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(accountId)) return Forbid();

        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var command = new MakeTransferCommand(
            request.IdRequisicao,
            request.ContaDestino,
            request.Valor,
            token,
            accountId
        );

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
             return BadRequest(new { message = result.Error, type = result.ErrorType });

        return NoContent();
    }
}

public record TransferRequest(string IdRequisicao, string ContaDestino, decimal Valor);
