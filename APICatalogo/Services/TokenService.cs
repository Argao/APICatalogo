using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace APICatalogo.Services;

public class TokenService : ITokenService
{
    public JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration configuration)
    {
        var key = configuration.GetSection("JWT").GetValue<string>("SecretKey") ??
                  throw new InvalidOperationException("JWT Secret Key not found");
        
        var privateKey = Encoding.UTF8.GetBytes(key);
        
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(privateKey),
            SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(configuration.GetSection("JWT")
                .GetValue<double>("TokenValidityInMinutes")),
            Audience = configuration.GetSection("JWT").GetValue<string>("ValidAudience"),
            Issuer = configuration.GetSection("JWT").GetValue<string>("ValidIssuer"),
            SigningCredentials = signingCredentials
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
    }

    public string GenerateRefreshToken()
    {
        var secureRandomBytes = new byte[128];

        using var randomNumberGenerator = RandomNumberGenerator.Create();
        
        randomNumberGenerator.GetBytes(secureRandomBytes);
        
        return Convert.ToBase64String(secureRandomBytes);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration configuration)
    {
         var secretKey = configuration.GetSection("JWT").GetValue<string>("SecretKey") ??
                         throw new InvalidOperationException("JWT Secret Key not found");
         var tokenValidationParameters = new TokenValidationParameters
         {
             ValidateAudience = false,
             ValidateIssuer = false,
             ValidateIssuerSigningKey = true,
             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
             ValidateLifetime = false
         };
         
         var tokenHandler = new JwtSecurityTokenHandler();
         
         var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

         if (securityToken is not JwtSecurityToken jwtSecurityToken ||
             !jwtSecurityToken.Header.Alg.Equals(
                 SecurityAlgorithms.HmacSha256,
                 StringComparison.InvariantCultureIgnoreCase))
         {
             throw new SecurityTokenException("Invalid token");
         }
         
         return principal;
    }
}