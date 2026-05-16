using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace MyJwtAuthService.Services.TokenGenerators
{
    public class TokenGenerator
    {
        public string GenerateToken(string secretKey, string issuer, string audience, DateTime utcExpirationTime, 
            IEnumerable<Claim>? claims = null)
        {
            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                DateTime.UtcNow,
                utcExpirationTime,
                credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
