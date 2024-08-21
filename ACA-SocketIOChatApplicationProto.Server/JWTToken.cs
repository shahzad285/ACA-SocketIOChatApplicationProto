using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ACA_SocketIOChatApplicationProto.Server
{
    public class JWTToken
    {
        public static string GenerateJwtToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("hhkhgjfhhiugbvfd38364k4kghgj765347653gfsdhsgfdhs"); // Use the same key as above
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.Name, userId)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string GetUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("hhkhgjfhhiugbvfd38364k4kghgj765347653gfsdhsgfdhs"); // Ensure your key is base64 encoded

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false, // Ignore token expiration (for testing or token renewal)
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                // Extract the UserID claim
                var userIdClaim = principal?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                return userIdClaim?.Value;
            }
            catch (Exception ex)
            {
                // Handle token validation failure (e.g., invalid token, signature issues)
                throw new Exception("Token validation failed", ex);
            }
        }     
    }
}
