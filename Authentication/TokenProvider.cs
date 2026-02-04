

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using b1.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace b1.Authentication
{
    public class TokenProvider
    {
        private IOptions<JwtSettings> Cfg { get; set; }
        public TokenProvider(IOptions<JwtSettings> cfg)
        {
            Cfg = cfg;
        }

        public string CreateToken(User user)
        {
            var token = "";
            string secretKey = Cfg.Value.Key;
            var symKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            //check HMac and AES and other stuff, arbitrarily chose SHA256 cuz I heard of it.
            var creds = new SigningCredentials(symKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Audience = Cfg.Value.Audience,
                Subject = new ClaimsIdentity(
                    [
                        new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub,user.Id)
                    ]
                ),
                Expires = DateTime.UtcNow.AddMinutes(Cfg.Value.ExpirationTime),
                Issuer = Cfg.Value.Issuer,
                SigningCredentials = creds
            };
            JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
            var handler = new JsonWebTokenHandler();
            token = handler.CreateToken(tokenDescriptor);
            return token;
        }
    }
}