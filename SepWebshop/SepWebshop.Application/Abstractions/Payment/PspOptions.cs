namespace SepWebshop.Application.Abstractions.Payment;

public sealed class PspOptions
{
    public const string SectionName = "PSP"; // refers to appsettings.json "PSP": { ... }
    public string FrontendBaseUrl { get; init; } = null!;
    public string PaymentInitEndpoint { get; init; } = null!;
    public string MerchantId { get; init; } = null!;
    public string MerchantPassword { get; init; } = null!;
}
