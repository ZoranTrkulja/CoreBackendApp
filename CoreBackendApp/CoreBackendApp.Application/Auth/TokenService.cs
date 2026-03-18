using CoreBackendApp.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CoreBackendApp.Application.Auth
{
    public class TokenService
    {
        public readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(
            User user,
            IEnumerable<string> roles,
            IEnumerable<string> permissions,
            IEnumerable<string> features)
        {

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new("tenantId", user.TenantId.ToString())
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            claims.AddRange(permissions.Select(p => new Claim("permission", p)));
            claims.AddRange(features.Select(f => new Claim("feature", f)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: cred
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public string GetHashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyToken(string token, string tokenHash) => GetHashToken(token) == tokenHash;
    }
}
