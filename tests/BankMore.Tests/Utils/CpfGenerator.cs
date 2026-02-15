namespace BankMore.Tests.Utils;

public static class CpfGenerator
{
    public static string Generate()
    {
        var random = Random.Shared;
        var numbers = new int[9];
        for (int i = 0; i < 9; i++)
            numbers[i] = random.Next(0, 9);

        var sum = 0;
        for (int i = 0; i < 9; i++)
            sum += numbers[i] * (10 - i);

        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        sum = 0;
        for (int i = 0; i < 9; i++)
            sum += numbers[i] * (11 - i);
        sum += digit1 * 2;

        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        return string.Join("", numbers) + digit1 + digit2;
    }
}
