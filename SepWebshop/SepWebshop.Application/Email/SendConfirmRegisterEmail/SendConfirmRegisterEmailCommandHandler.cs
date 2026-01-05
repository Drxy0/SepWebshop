using MediatR;
using SepWebshop.Application.Abstractions.Email;
using SepWebshop.Domain;

namespace SepWebshop.Application.Email.SendConfirmRegisterEmail;

internal sealed class SendConfirmRegisterEmailCommandHandler(IEmailSender emailSender) 
    : IRequestHandler<SendConfirmRegisterEmailCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SendConfirmRegisterEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            string htmlBody = ConfirmEmailHtmlBody(request);

            await emailSender.SendAsync(
                request.Email,
                "Confirm your registration",
                htmlBody,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Failure<Unit>(
                Error.Failure("Email.SendFailed", ex.Message));
        }

        return Result.Success(Unit.Value);
    }

    private string ConfirmEmailHtmlBody(SendConfirmRegisterEmailCommand request)
    {
        return $"""
                <html>
                <body style="font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;">
                    <div style="max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 8px; text-align: center;">
                        <h2 style="color: #2563eb;">Sep Webshop</h2>
                        <p>Thank you for registering! Please confirm your account by clicking the button below:</p>
                        <a href="{request.ConfirmationLink}" style="display: inline-block; padding: 12px 25px; margin: 20px 0; background-color: #2563eb; color: #ffffff; text-decoration: none; border-radius: 5px;">Confirm Account</a>
                        <p>If the button doesn’t work, copy and paste this link into your browser:</p>
                        <p style="word-break: break-all;"><a href="{request.ConfirmationLink}">{request.ConfirmationLink}</a></p>
                    </div>
                </body>
                </html>
                """;
    }
}
