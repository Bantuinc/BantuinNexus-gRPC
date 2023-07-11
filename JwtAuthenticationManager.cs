using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BantuinNexus_gRPC
{
    public static class JwtAuthenticationManager
    {
        public static LoginRes Login(String name)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes(SettingsConfigHelper.AppSetting("Jwt:SecretKey"));
            var tokenExpires = DateTime.Now.AddMinutes(int.Parse(SettingsConfigHelper.AppSetting("Jwt:Validity")));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("name", name),
                    new Claim(ClaimTypes.Role, "Administrator"),
                }),
                Expires = tokenExpires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };
            var securityToken = jwtSecurityTokenHandler.CreateToken(tokenDescriptor);
            var token = jwtSecurityTokenHandler.WriteToken(securityToken);

            return new LoginRes
            {
                Name = name,
                AccessToken = token,
                ExpiredIn = (int)tokenExpires.Subtract(DateTime.Now).TotalSeconds
            };
        }
    }
}
