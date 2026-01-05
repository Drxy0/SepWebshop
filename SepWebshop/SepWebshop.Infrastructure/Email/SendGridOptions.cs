namespace SepWebshop.Infrastructure.Email;

public sealed class SendGridOptions
{
    public const string SectionName = "SendGrid";
    public string ApiKey { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
}
