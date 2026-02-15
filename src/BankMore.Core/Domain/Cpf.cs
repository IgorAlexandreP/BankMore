namespace BankMore.Core.Domain;

using BankMore.Core.Shared;
using System.Text.RegularExpressions;

public record Cpf
{
    public string Value { get; }

    private Cpf(string value) => Value = value;

    public static Result<Cpf> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Cpf>.Failure("CPF é obrigatório", "INVALID_DOCUMENT");

        var numbers = Regex.Replace(value, @"[^\d]", "");

        if (numbers.Length != 11)
            return Result<Cpf>.Failure("CPF deve conter 11 dígitos", "INVALID_DOCUMENT");
        
        if (new string(numbers[0], 11) == numbers)
             return Result<Cpf>.Failure("Formato de CPF inválido", "INVALID_DOCUMENT");

        int[] multiplier1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplier2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        
        string tempCpf = numbers.Substring(0, 9);
        int sum = 0;

        for (int i = 0; i < 9; i++)
            sum += int.Parse(tempCpf[i].ToString()) * multiplier1[i];

        int remainder = sum % 11;
        if (remainder < 2) remainder = 0;
        else remainder = 11 - remainder;

        string digit = remainder.ToString();
        tempCpf += digit;
        sum = 0;

        for (int i = 0; i < 10; i++)
            sum += int.Parse(tempCpf[i].ToString()) * multiplier2[i];

        remainder = sum % 11;
        if (remainder < 2) remainder = 0;
        else remainder = 11 - remainder;

        digit += remainder.ToString();

        if (!numbers.EndsWith(digit))
            return Result<Cpf>.Failure("CPF inválido", "INVALID_DOCUMENT");

        return Result<Cpf>.Success(new Cpf(numbers));
    }
    
    public override string ToString() => Value;
}
