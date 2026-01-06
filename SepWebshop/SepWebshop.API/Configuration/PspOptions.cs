namespace SepWebshop.API.Configuration;

public sealed class PspOptions
{
    public const string SectionName = "PSP"; // refers to appsettings.json "PSP": { ... }
    public string FrontendBaseUrl { get; init; } = null!;
}
