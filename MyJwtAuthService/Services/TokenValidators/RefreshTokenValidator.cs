using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyJwtAuthService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace MyJwtAuthService.Services.TokenValidators
{
    public class RefreshTokenValidator
    {
        private readonly IOptions<AuthenticationConfiguration> _configuration;

        public RefreshTokenValidator(IOptions<AuthenticationConfiguration> configuration)
        {
            _configuration = configuration;
        }

        public bool Validate(string refreshToken)
        {
            var config = _configuration.Value;

            TokenValidationParameters validationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.RefreshTokenSecret)),
                ValidIssuer = config.Issuer,
                ValidAudience = config.Audience,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ClockSkew = TimeSpan.Zero
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
