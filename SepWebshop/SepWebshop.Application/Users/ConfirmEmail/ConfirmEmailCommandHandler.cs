using MediatR;
using Microsoft.EntityFrameworkCore;
using SepWebshop.Application.Abstractions.Data;
using SepWebshop.Application.Users.ConfirmUser;
using SepWebshop.Domain;
using SepWebshop.Domain.Users;

namespace SepWebshop.Application.Users.ConfirmEmail;

internal sealed class ConfirmEmailCommandHandler(IApplicationDbContext context) : IRequestHandler<ConfirmEmailCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<string>(UserErrors.NotFound(command.UserId));
        }

        if (user.ConfirmationToken != command.Token)
        {
            return Result.Failure<string>(UserErrors.InvalidConfirmationToken);
        }

        user.IsAccountActive = true;
        user.ConfirmationToken = null;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success("Your account has been confirmed. You can now log in.");
    }
}
