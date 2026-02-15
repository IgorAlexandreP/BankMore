namespace BankMore.Core.Auth;

public class JwtSettings
{
    public string Secret { get; set; } = "BankMore_API_v1_SecretKey_@2024_SecureToken_DoNotShare";
    public string Issuer { get; set; } = "BankMore";
    public string Audience { get; set; } = "BankMoreUsers";
    public int ExpiryMinutes { get; set; } = 60;
}
