using blazorSBIFS.Shared.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace blazorSBIFS.Server.Tools
{
    public class JwtTools
    {
        public static string CreateToken(User u)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, u.UserID.ToString()),
                new Claim(ClaimTypes.Role, u.Privilege.ToString())
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(new SaltAdapter().GetSalt()));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: cred
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
