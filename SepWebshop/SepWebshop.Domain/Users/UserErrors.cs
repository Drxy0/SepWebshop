namespace SepWebshop.Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");

    public static Error Unauthorized() => Error.Failure(
        "Users.Unauthorized",
        "You are not authorized to perform this action.");

    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "The provided email is not unique");

    public static readonly Error EmailNotConfirmed = Error.Conflict(
        "Users.EmailNotConfirmed",
        "You must confirm your email before logging in");
    
    public static readonly Error InvalidConfirmationToken = Error.Conflict(
        "Users.InvalidConfirmationToken",
        "Invalid confirmation token");

    public static readonly Error EmailSendFailed = Error.Problem(
        "Users.EmailSendFailed",
        "Email failed to send");

    public static readonly Error WeakPassword = Error.Problem(
        "Users.WeakPassword",
        "The provided password is not storng enogh");

    public static readonly Error InvalidCredentials = Error.Problem(
        "Users.InvalidCredentials",
        "Invalid credentials");

    public static readonly Error InvalidRefreshToken = Error.Problem( 
        "Users.InvalidRefreshToken",
        "The refresh token is invalid");

}
