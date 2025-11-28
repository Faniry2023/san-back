using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SAN_API.Helper
{
    public class JwtHelper
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtHelper(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt key is not set");
            _issuer = "My_issuer-_number--4258ff";
                        
            _audience = "My-audience-_numberPrimarycode__4560ML5P7";
        }
        public string GenerateToken(Guid id, string? email)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, email!),
                new Claim("UserId",id.ToString())
            };

            var tokent = new JwtSecurityToken(
                issuer: "My_issuer-_number--4258ff",
                audience: "My-audience-_numberPrimarycode__4560ML5P7",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(tokent);
        }
    }
}
