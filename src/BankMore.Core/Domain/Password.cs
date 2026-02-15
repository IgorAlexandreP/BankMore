namespace BankMore.Core.Domain;

using BankMore.Core.Shared;
using System.Text.RegularExpressions;

public record Password
{
    public string Value { get; }
    
    private static readonly Regex PasswordRegex = new Regex(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
        RegexOptions.Compiled);

    private Password(string value) => Value = value;

    public static Result<Password> Create(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return Result<Password>.Failure("A senha é obrigatória.", "INVALID_PASSWORD");

        if (!PasswordRegex.IsMatch(plainText))
            return Result<Password>.Failure(
                "A senha deve ter no mínimo 8 caracteres, contendo letra maiúscula, minúscula, número e caractere especial.",
                "WEAK_PASSWORD");

        return Result<Password>.Success(new Password(plainText));
    }

    public override string ToString() => Value;
}
