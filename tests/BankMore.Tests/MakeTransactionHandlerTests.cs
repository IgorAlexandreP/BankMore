extern alias AccountApi;
using AccountApi::BankMore.Account.API.Application.Commands;
using AccountApi::BankMore.Account.API.Domain;
using AccountApi::BankMore.Account.API.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace BankMore.Tests;

public class MakeTransactionHandlerTests
{
    private readonly Mock<IAccountRepository> _repositoryMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly MakeTransactionHandler _handler;

    public MakeTransactionHandlerTests()
    {
        _repositoryMock = new Mock<IAccountRepository>();
        _cacheMock = new Mock<IMemoryCache>();
        _cacheMock.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>);
        
        _handler = new MakeTransactionHandler(_repositoryMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCreditTransactionIsValid()
    {
        var accountId = Guid.NewGuid().ToString();
        var account = new ContaCorrente(accountId, 12345, "Test User", "52998224725", true, "hash", "salt");
        var command = new MakeTransactionCommand(Guid.NewGuid().ToString(), null, 100, "C", accountId);

        _repositoryMock.Setup(r => r.IsIdempotentAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.GetByIdAsync(accountId)).ReturnsAsync(account);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.AddMovimentoAsync(It.IsAny<Movimento>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDebitBalanceIsInsufficient()
    {
        var accountId = Guid.NewGuid().ToString();
        var account = new ContaCorrente(accountId, 12345, "Test User", "52998224725", true, "hash", "salt");
        var command = new MakeTransactionCommand(Guid.NewGuid().ToString(), null, 100, "D", accountId);

        _repositoryMock.Setup(r => r.IsIdempotentAsync(It.IsAny<string>())).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.GetByIdAsync(accountId)).ReturnsAsync(account);
        _repositoryMock.Setup(r => r.GetMovimentosAsync(accountId)).ReturnsAsync(new List<Movimento>());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("INSUFFICIENT_FUNDS", result.ErrorType);
        _repositoryMock.Verify(r => r.AddMovimentoAsync(It.IsAny<Movimento>()), Times.Never);
    }
}
