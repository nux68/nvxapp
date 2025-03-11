using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Infrastructure;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System;
using System.Text;
using System.Text.Json;


namespace nvxapp.server.service.ClientServer_Service.ChatAI
{
    public class ChatAIService: ServiceBase, IChatAIService
    {

        public ChatAIService(IMapper mapper,
                           UserManager<ApplicationUser> userManager,
                           IAspNetUsersRepository aspNetUsersRepository,
                           IOptions<JwtParameter> jwtParameter,
                           IHttpContextAccessor httpContextAccessor,
                           IConfiguration configuration

                           ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            
        }

        public virtual async Task<GenericResult<ChatAIOutModel>> SendMessage(GenericRequest<ChatAIInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                ChatAIOutModel retVal = new ChatAIOutModel();

                retVal.Responce = "AI responce :" +  model.Data.Request;
                

                ////////////// URL dell'API di Llama
                ////////////string url = "https://api.llama-api.com/generate"; // Modifica con l'endpoint corretto dell'API

                ////////////// La tua chiave API
                ////////////string apiKey = "a94db9de-c800-4d1e-af35-caa80de09396"; // Sostituisci con la tua chiave API

                ////////////// Dati della richiesta
                ////////////var requestData = new
                ////////////{
                ////////////    model = "llama3", // Specifica il modello da utilizzare
                ////////////    prompt = "Ciao Llama, come stai?"
                ////////////};

                ////////////// Serializzazione del corpo della richiesta in JSON
                ////////////string json = System.Text.Json.JsonSerializer.Serialize(requestData);
                ////////////var content = new StringContent(json, Encoding.UTF8, "application/json");

                ////////////using (HttpClient client = new HttpClient())
                ////////////{
                ////////////    try
                ////////////    {
                ////////////        // Aggiungi l'header di autorizzazione
                ////////////        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                ////////////        // Invia la richiesta POST
                ////////////        HttpResponseMessage response = await client.PostAsync(url, content);

                ////////////        // Assicurati che la risposta sia valida
                ////////////        response.EnsureSuccessStatusCode();

                ////////////        // Leggi il contenuto della risposta
                ////////////        string responseBody = await response.Content.ReadAsStringAsync();
                ////////////        Console.WriteLine("Risposta dall'API di Llama:");
                ////////////        Console.WriteLine(responseBody);
                ////////////    }
                ////////////    catch (Exception ex)
                ////////////    {
                ////////////        // Gestione degli errori
                ////////////        Console.WriteLine("Errore durante la chiamata all'API:");
                ////////////        Console.WriteLine(ex.Message);
                ////////////    }

                ////////////}



                    // LOCAL

                    //using HttpClient client = new();

                    // URL dell'endpoint API
                    //var prompt =
                    //            "Trasforma il seguente comando in JSON. Non aggiungere spiegazioni o testo extra.\n" +
                    //            "Rispondi solo con un JSON valido.\n\n" +
                    //            "Esempio:\n" +
                    //            "INPUT: \"Segna entrata alle 8 per Mario Rossi\"\n" +
                    //            "OUTPUT:\n" +
                    //            "{\n" +
                    //            "  \"action\": \"timbratura\",\n" +
                    //            "  \"type\": \"entrata\",\n" +
                    //            "  \"orario\": \"08:00\",\n" +
                    //            "  \"dipendenti\": [\"Mario Rossi\"]\n" +
                    //            "}\n\n" +
                    //            $"INPUT: \"{model.Data.Request}\"\n" +
                    //            "OUTPUT:";

                    //var requestBody = new
                    //{
                    //    model = "llama3",
                    //    prompt = prompt,
                    //    stream = false,
                    //    temperature = 0.2
                    //};




                    //string jsonString = JsonSerializer.Serialize(requestBody);
                    //var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    //HttpResponseMessage response = await client.PostAsync("http://localhost:11434/api/generate", content);

                    //if (response.IsSuccessStatusCode)
                    //{
                    //    var responseString = await response.Content.ReadAsStringAsync();

                    //    var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    //    if(ollamaResponse!=null)
                    //    {
                    //        var res = JsonSerializer.Deserialize<object>(ollamaResponse.Response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    //        if(res!=null)
                    //            retVal.Responce = JsonSerializer.Serialize(res, new JsonSerializerOptions { WriteIndented = true });


                    //    }


                    //    //if (responseString != null)
                    //    //    retVal.Responce = (string)responseString;
                    //    //var mm = JsonSerializer.Deserialize<object>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });    

                    //}




                    //eliminare
                    // Nessun 'await' qui
                    await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


    }

    public interface IChatAIService : IServiceBase
    {
        public Task<GenericResult<ChatAIOutModel>> SendMessage(GenericRequest<ChatAIInModel> model, Boolean isSubProcess);
    }


    public class OllamaResponse
    {
        public string Model { get; set; }
        public string CreatedAt { get; set; }
        public string Response { get; set; }  // Qui dentro ci sarà il JSON della timbratura
        public bool Done { get; set; }
    }

}
