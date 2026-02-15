extern alias AccountApi;
using AccountApi::BankMore.Account.API.Application.Commands;
using AccountApi::BankMore.Account.API.Domain;
using AccountApi::BankMore.Account.API.Infrastructure.Repositories;
using AccountApi::BankMore.Account.API.Infrastructure.Services;
using Moq;

namespace BankMore.Tests;

public class CreateAccountHandlerTests
{
    private readonly Mock<IAccountRepository> _repositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly CreateAccountHandler _handler;

    public CreateAccountHandlerTests()
    {
        _repositoryMock = new Mock<IAccountRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _handler = new CreateAccountHandler(_repositoryMock.Object, _passwordServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCpfIsValidAndAccountDoesNotExist()
    {
        var command = new CreateAccountCommand("52998224725", "Test User", "SenhaForte123!");
        _repositoryMock.Setup(r => r.GetByCpfAsync(It.IsAny<string>())).ReturnsAsync((ContaCorrente?)null);
        _passwordServiceMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hashed_password");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ContaCorrente>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCpfIsInvalid()
    {
        var command = new CreateAccountCommand("11111111111", "Test User", "SenhaForte123!");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_DOCUMENT", result.ErrorType);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ContaCorrente>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAccountAlreadyExists()
    {
        var command = new CreateAccountCommand("52998224725", "Test User", "SenhaForte123!");
        var existingAccount = ContaCorrente.Create("52998224725", "Existing User", "hash", "salt");
        _repositoryMock.Setup(r => r.GetByCpfAsync(It.IsAny<string>())).ReturnsAsync(existingAccount);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("DUPLICATE_ACCOUNT", result.ErrorType);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ContaCorrente>()), Times.Never);
    }
}
