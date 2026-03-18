using CoreBackendApp.Application.Common.Models;

namespace CoreBackendApp.Application.Auth;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Invalid email or password.");
    public static readonly Error InvalidToken = new("Auth.InvalidToken", "Invalid refresh token.");
    public static readonly Error TokenExpired = new("Auth.TokenExpired", "Refresh token has expired.");
    public static readonly Error TokenReused = new("Auth.TokenReuseDetected", "Refresh token reuse detected! All sessions revoked.");
    public static readonly Error UserNotFound = new("Auth.UserNotFound", "User not found.");
}
