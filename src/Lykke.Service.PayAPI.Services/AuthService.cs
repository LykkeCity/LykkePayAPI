using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JetBrains.Annotations;
using Lykke.Service.PayAPI.Core.Services;
using Lykke.Service.PayAPI.Core.Settings.ServiceSettings;
using Microsoft.IdentityModel.Tokens;

namespace Lykke.Service.PayAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtSecuritySettings _securitySettings;

        public AuthService([NotNull] JwtSecuritySettings securitySettings)
        {
            _securitySettings = securitySettings ?? throw new ArgumentNullException(nameof(securitySettings));
        }

        public string CreateToken(string email)
        {
            var token = new JwtSecurityToken(
                _securitySettings.Issuer,
                _securitySettings.Audience,
                claims: new[] {new Claim(JwtRegisteredClaimNames.Email, email)},
                expires: DateTime.UtcNow.Add(_securitySettings.TokenLifetime),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_securitySettings.Key)),
                    SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
