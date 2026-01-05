using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using SendGrid.Helpers.Mail.Model;
using SepWebshop.Application.Abstractions.Email;

namespace SepWebshop.Infrastructure.Email;

internal sealed class SendGridEmailSender : IEmailSender
{
    private readonly SendGridClient _client;
    private readonly SendGridOptions _options;

    public SendGridEmailSender(IOptions<SendGridOptions> options)
    {
        _options = options.Value;
        _client = new SendGridClient(_options.ApiKey);
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var from = new EmailAddress(_options.FromEmail, _options.FromName);
        var recipient = new EmailAddress(to);

        var message = MailHelper.CreateSingleEmail(
            from,
            recipient,
            subject,
            plainTextContent: null,
            htmlBody);

        var response = await _client.SendEmailAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"SendGrid failed: {response.StatusCode} - {body}");
        }
    }
}
