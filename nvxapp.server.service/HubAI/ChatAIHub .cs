using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.HubAI
{
    public class ChatAIHub : Hub
    {
        protected readonly JwtParameter _jwtParameter;

        public ChatAIHub(IOptions<JwtParameter> jwtParameter) {
            _jwtParameter = jwtParameter.Value;
        }

        //public override async Task OnConnectedAsync()
        //{
        //    // Estrai il nome utente dal token
        //    var userName = Context.User?.Identity?.Name;

        //    // Puoi anche leggere altri claim
        //    var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        //    Console.WriteLine($"Connessione stabilita: {userName} (ID: {userId}, Ruolo: {userRole})");

        //    await base.OnConnectedAsync();
        //}

        //[Authorize]
        public async Task SendMessage(string token, object message)
        {

            // Valida il token manualmente
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtParameter.Key));

            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
            };

            try
            {
                var claimsPrincipal = handler.ValidateToken(token, parameters, out _);

                TokenProperty tokenProperty = GetTokenProperty(handler, claimsPrincipal, token);

                //Console.WriteLine($"Messaggio ricevuto da: {userName}");
                await Clients.All.SendAsync("ReceiveMessage", new
                {
                    TipoRisposta = "tipo1",
                    ValoreRisposta = "valore risposta"
                });
            }
            catch (Exception ex)
            {
                throw new HubException("Token non valido: " + ex.Message);
            }



            //var userName = Context.User?.Identity?.Name;

            //await Clients.All.SendAsync("ReceiveMessage", new 
            //{
            //    TipoRisposta ="tipo1",
            //    ValoreRisposta = "valore risposta"

            //});
        }
    
        private TokenProperty GetTokenProperty(JwtSecurityTokenHandler handler, 
                                               ClaimsPrincipal? claimsPrincipal,
                                               string token)
        {
            TokenProperty tokenProperty = new TokenProperty();

            if(claimsPrincipal!=null)
                tokenProperty.UserId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value??"";

            // Decodifica il token
            var jwtToken = handler.ReadJwtToken(token);

            if(jwtToken!=null)
            {
                tokenProperty.Tenant = jwtToken.Claims.FirstOrDefault(c => c.Type == "tenant")?.Value??string.Empty;
                tokenProperty.Dealer = jwtToken.Claims.FirstOrDefault(c => c.Type == "dealer")?.Value ?? string.Empty;
                tokenProperty.Company = jwtToken.Claims.FirstOrDefault(c => c.Type == "company")?.Value ?? string.Empty;
                tokenProperty.FinancialAdvisor = jwtToken.Claims.FirstOrDefault(c => c.Type == "financialadvisor")?.Value ?? string.Empty;
            }

            return tokenProperty;
        }

    }
}
