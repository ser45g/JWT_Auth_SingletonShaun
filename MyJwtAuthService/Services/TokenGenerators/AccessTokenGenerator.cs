using Microsoft.AspNetCore.Identity;
using MyJwtAuthService.Models;
using System.Security.Claims;

namespace MyJwtAuthService.Services.TokenGenerators
{
    public class AccessTokenGenerator
    {
        private readonly AuthenticationConfiguration _configuration;
        private readonly TokenGenerator _tokenGenerator;
        private readonly UserManager<ApplicationUser> _userRepository;
        public AccessTokenGenerator(AuthenticationConfiguration configuration, TokenGenerator tokenGenerator, UserManager<ApplicationUser> userRepository)
        {
            _configuration = configuration;
            _tokenGenerator = tokenGenerator;
            _userRepository = userRepository;
        }

        public AccessToken GenerateToken(ApplicationUser user)
        {
            var roles = _userRepository.GetRolesAsync(user).GetAwaiter().GetResult();
         
            var roleClaims = roles.Select(r=>new Claim(ClaimTypes.Role, r)).ToList();

            var claims = new List<Claim>()
            {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName)
                
            };
            claims.AddRange(roleClaims);

            DateTime expirationTime = DateTime.UtcNow.AddMinutes(_configuration.AccessTokenExpirationMinutes);
            string value = _tokenGenerator.GenerateToken(_configuration.AccessTokenSecret, _configuration.Issuer, _configuration.Audience, expirationTime, claims);

            return new AccessToken()
            {
                Value = value,
                ExpirationTime = expirationTime
            };
        }
    }
}
