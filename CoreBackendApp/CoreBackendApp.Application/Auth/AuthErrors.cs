using CoreBackendApp.Application.Common.Models;

namespace CoreBackendApp.Application.Auth;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = Error.Failure("Auth.InvalidCredentials", "Invalid email or password.");
    public static readonly Error InvalidToken = Error.Failure("Auth.InvalidToken", "Invalid refresh token.");
    public static readonly Error TokenExpired = Error.Failure("Auth.TokenExpired", "Refresh token has expired.");
    public static readonly Error TokenReused = Error.Failure("Auth.TokenReuseDetected", "Refresh token reuse detected! All sessions revoked.");
    public static readonly Error UserNotFound = Error.NotFound("Auth.UserNotFound", "User not found.");
}
