using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using nvxapp.server.service.ServerModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.service.Helpers
{
    public static class UtilToken
    {
        public static string? GenerateJwtToken(ApplicationUser user, 
                                               string Key ,
                                               string Issuer,
                                               string Audience,
                                               int ExpireMinutes,
                                               string CurrentTenat)
        {
            string? retVal = null;

            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserName))
                return retVal;

            //valori inseriti nel token, saranno disponibili nelle API
            var claims = new[]
            {
                //new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("tenant", CurrentTenat)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //var token = new JwtSecurityToken(_jwtParameter.Issuer, _jwtParameter.Audience, claims, expires: DateTime.UtcNow.AddHours(2), signingCredentials: creds);
            var token = new JwtSecurityToken(Issuer, Audience, claims, expires: DateTime.UtcNow.AddSeconds(ExpireMinutes), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);

            

        }
    }
}
