using Microsoft.Extensions.Options;
using MyJwtAuthService.Models;

namespace MyJwtAuthService.Services.TokenGenerators
{
    public class RefreshTokenGenerator
    {
        private readonly IOptions<AuthenticationConfiguration> _configuration;
        private readonly TokenGenerator _tokenGenerator;

        public RefreshTokenGenerator(IOptions<AuthenticationConfiguration> configuration, TokenGenerator tokenGenerator)
        {
            _configuration = configuration;
            _tokenGenerator = tokenGenerator;
        }

        public string GenerateToken()
        {
            var config = _configuration.Value;
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(config.RefreshTokenExpirationMinutes);

            return _tokenGenerator.GenerateToken(
                config.RefreshTokenSecret,
                config.Issuer,
                config.Audience,
                expirationTime);
        }
    }
}
