using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using FeatureFlags.APIs.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FeatureFlags.APIs.Services
{
    
    public interface IJwtUtilsService{
        public string GenerateToken(string identity);
        public string ValidateToken(string token);

    }
    public class JwtUtilsService: IJwtUtilsService
    {
        private readonly JWTSettings _settings;

        public JwtUtilsService(IOptions<JWTSettings> settings){
            _settings = settings.Value;
        }

        public string GenerateToken(string identity)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_settings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", identity) }),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string ValidateToken(string token)
        {
            if (token == null) 
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_settings.Secret);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = false,
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var identity = jwtToken.Claims.First(x=>x.Type == "id").Value;

                // return user id from JWT token if validation successful
                return identity;
            }
            catch(Exception)
            {
                return null;
            }
        }
    }
}