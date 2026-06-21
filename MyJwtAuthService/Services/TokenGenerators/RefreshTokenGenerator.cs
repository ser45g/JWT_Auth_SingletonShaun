using Microsoft.Extensions.Options;
using MyJwtAuthService.Options;

namespace MyJwtAuthService.Services.TokenGenerators
{
    public class RefreshTokenGenerator
    {
        private readonly IOptions<AuthenticationOptions> _configuration;
        private readonly TokenGenerator _tokenGenerator;

        public RefreshTokenGenerator(IOptions<AuthenticationOptions> configuration, TokenGenerator tokenGenerator)
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
