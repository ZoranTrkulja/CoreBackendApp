using CoreBackendApp.Application.Interface;

namespace CoreBackendApp.Application.Auth
{
    public class AuthService(IUserRepository userRepository, TokenService tokenService)
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly TokenService _tokenService = tokenService;

        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _userRepository.GetUserWithDetailsAsync(loginRequest.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            var roles = user.UserRoles.Select(ur => ur.Role.Name);
            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Code);

            var features = user.Tenant.TenantFeatures
                .Select(tf => tf.Feature.Key);

            var token = _tokenService.GenerateAccessToken(
                user,
                roles,
                permissions,
                features);

            return new LoginResponse
            {
                AccessToken = token
            };
        }
    }
}
