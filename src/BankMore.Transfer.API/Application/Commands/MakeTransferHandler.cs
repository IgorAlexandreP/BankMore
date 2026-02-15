namespace BankMore.Transfer.API.Application.Commands;

using MediatR;
using BankMore.Core.Shared;
using BankMore.Transfer.API.Domain;
using BankMore.Transfer.API.Infrastructure.Repositories;
using BankMore.Transfer.API.Infrastructure.Services;
using KafkaFlow.Producers;

public class MakeTransferHandler : IRequestHandler<MakeTransferCommand, Result>
{
    private readonly ITransferRepository _repository;
    private readonly IAccountService _accountService;
    private readonly IProducerAccessor _producerAccessor;

    public MakeTransferHandler(ITransferRepository repository, IAccountService accountService, IProducerAccessor producerAccessor)
    {
        _repository = repository;
        _accountService = accountService;
        _producerAccessor = producerAccessor;
    }

    public async Task<Result> Handle(MakeTransferCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.IsIdempotentAsync(request.RequestId))
            return Result.Success();

        if (request.Value <= 0)
            return Result.Failure("Valor inválido", "INVALID_VALUE");

        var debitSuccess = await _accountService.DebitAsync(request.Token, request.RequestId + "-D", request.Value);
        if (!debitSuccess)
            return Result.Failure("Falha ao debitar conta de origem", "TRANSACTION_FAILED");

        var creditSuccess = await _accountService.CreditAsync(request.Token, request.RequestId + "-C", request.TargetAccount, request.Value);
        if (!creditSuccess)
        {
            await _accountService.ReverseDebitAsync(request.Token, request.RequestId + "-R", request.Value);
            return Result.Failure("Falha ao creditar conta de destino. Estornado.", "TRANSACTION_FAILED");
        }

        var transfer = Transferencia.Create(request.AuthenticatedAccountId, request.TargetAccount, request.Value);
        await _repository.AddAsync(transfer);

        await _repository.RegisterIdempotencyAsync(request.RequestId, "Transfer", "Success");

        try
        {
            var producer = _producerAccessor.GetProducer("transferencia-producer");
            await producer.ProduceAsync("transferencias-realizadas", Guid.NewGuid().ToString(), 
                new TransferenciaRealizadaEvent(request.RequestId, request.AuthenticatedAccountId, request.Value, DateTime.Now));
        }
        catch
        {
            
        }

        return Result.Success();
    }
}
