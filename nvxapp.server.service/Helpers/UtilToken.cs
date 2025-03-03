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
        //public static string? GenerateJwtToken(ApplicationUser user,
        //                                    string Key,
        //                                    string Issuer,
        //                                    string Audience,
        //                                    int ExpireMinutes,
        //                                    string Tenant,
        //                                    string Dealer,
        //                                    string FinancialAdvisor,
        //                                    string Company)


        public static string? GenerateJwtToken(/*ApplicationUser user, */
                                               string Key ,
                                               string Issuer,
                                               string Audience,
                                               int ExpireMinutes,
                                               TokenProperty tokenProperty
                                                /*string Tenant,
                                                string Dealer,
                                                string FinancialAdvisor,
                                                string Company*/
                                                )
        {
            //string? retVal = null;

            //if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserName))
            //    return retVal;

            //valori inseriti nel token, saranno disponibili nelle API
            var claims = new[]
            {
                //new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Sub, tokenProperty.UserId),
                //new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("tenant", tokenProperty.Tenant),
                new Claim("dealer", tokenProperty.Dealer),
                new Claim("financialadvisor", tokenProperty.FinancialAdvisor),
                new Claim("company", tokenProperty.Company)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //var token = new JwtSecurityToken(_jwtParameter.Issuer, _jwtParameter.Audience, claims, expires: DateTime.UtcNow.AddHours(2), signingCredentials: creds);
            var token = new JwtSecurityToken(Issuer, Audience, claims, expires: DateTime.UtcNow.AddSeconds(ExpireMinutes), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);

            

        }
    }


    public  class TokenProperty
    {
        public string UserId { get; set; } = string.Empty;
        public string Tenant { get; set; } = string.Empty;
        public string Dealer { get; set; } = string.Empty;
        public string FinancialAdvisor { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;

    }

}
