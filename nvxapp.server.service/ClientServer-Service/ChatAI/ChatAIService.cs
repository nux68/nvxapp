using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
using System.Text.RegularExpressions;
//using Microsoft.ML;
//using Microsoft.ML.Data;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using RabbitMQ.Client;




namespace nvxapp.server.service.ClientServer_Service.ChatAI
{
    public class ChatAIService: ServiceBase, IChatAIService
    {

        protected readonly ConnectionFactory? _factory = null;
        protected IConnection? _connection;
        protected IChannel? _channel;
        private readonly CancellationTokenSource _cts = new(); // Aggiungi questo campo alla classe


        public ChatAIService(IMapper mapper,
                           UserManager<ApplicationUser> userManager,
                           IAspNetUsersRepository aspNetUsersRepository,
                           IOptions<JwtParameter> jwtParameter,
                           IHttpContextAccessor httpContextAccessor,
                           IConfiguration configuration

                           ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {

            //////////Rabbit
            _cts = new CancellationTokenSource(); // Inizializza il token
            
            if (configuration != null)
            {
                int port = 5672;
                string? sPort = configuration["RabbitQm:Connection:Port"];
                if (!string.IsNullOrEmpty(sPort))
                    port = int.Parse(sPort);


                _factory = new ConnectionFactory
                {
                    HostName = configuration["RabbitQm:Connection:HostName"] ?? "localhost",
                    Port = port,
                    UserName = configuration["RabbitQm:Connection:UserName"] ?? "guest",
                    Password = configuration["RabbitQm:Connection:Password"] ?? "guest"
                };
            }

        }

        public virtual async Task<GenericResult<ChatAIOutModel>> SendMessage(GenericRequest<ChatAIInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                ChatAIOutModel retVal = new ChatAIOutModel();

                retVal.Responce = "AI responce :" +  model.Data.Request;

                FakeAI_Regex fakeAI = new FakeAI_Regex();
                var resAi = fakeAI.GetClockignCommand(model.Data.Request);
                retVal.Responce = "AI responce :" + JsonSerializer.Serialize(resAi) ;


                if(_factory!=null)
                {
                    _connection = await _factory.CreateConnectionAsync();
                    _channel = await _connection.CreateChannelAsync();

                    var body = Encoding.UTF8.GetBytes(model.Data.Request);

                    // Dichiarazione dell'exchange
                    await _channel.ExchangeDeclareAsync(exchange: "logs", type: ExchangeType.Fanout);

                    // Pubblicazione del messaggio
                    await _channel.BasicPublishAsync(exchange: "logs", routingKey: string.Empty, body: body);

                }
                



                //FakeAI_Nlp fakeAI_Nlp = new FakeAI_Nlp();
                //var cmdObj = fakeAI_Nlp.EstrarreDati(model.Data.Request);
                //var d = 0;

                //var nlp = new NlpService();
                //var risultato = nlp.AnalizzaTesto(model.Data.Request);

                //Console.WriteLine($"Entità riconosciute: {string.Join(", ", risultato.Entita)}");
                //Console.WriteLine($"Tipi di entità: {string.Join(", ", risultato.Etichette)}");


                ////////// LLAma api
                ////////// URL dell'API di Llama
                ////////string url = "https://api.llama-api.com/chat/completions"; // Modifica con l'endpoint corretto dell'API

                ////////// La tua chiave API
                ////////string apiKey = "a94db9de-c800-4d1e-af35-caa80de09396"; // Sostituisci con la tua chiave API




                //////////Dati della richiesta
                //////////var requestData = new
                //////////{
                //////////    model = "llama3.3-70b", // Specifica il modello da utilizzare
                //////////    prompt = "Ciao Llama, come stai?"
                //////////};

                ////////var requestData = new
                ////////{
                ////////    model = "llama3.3-70b",
                ////////    messages = new[]
                ////////            {
                ////////                new { role = "system", content = "You are a helpful assistant." },
                ////////                new { role = "user", content = "Give me 10 orcish names" }
                ////////            }
                ////////};


                ////////// Serializzazione del corpo della richiesta in JSON
                ////////string json = System.Text.Json.JsonSerializer.Serialize(requestData);
                ////////var content = new StringContent(json, Encoding.UTF8, "application/json");

                ////////using (HttpClient client = new HttpClient())
                ////////{

                ////////    try
                ////////    {
                ////////        // Aggiungi l'header di autorizzazione
                ////////        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                ////////        // Invia la richiesta POST
                ////////        HttpResponseMessage response = await client.PostAsync(url, content);

                ////////        // Assicurati che la risposta sia valida
                ////////        response.EnsureSuccessStatusCode();

                ////////        // Leggi il contenuto della risposta
                ////////        string responseBody = await response.Content.ReadAsStringAsync();
                ////////        Console.WriteLine("Risposta dall'API di Llama:");
                ////////        Console.WriteLine(responseBody);
                ////////    }
                ////////    catch (Exception ex)
                ////////    {
                ////////        // Gestione degli errori
                ////////        Console.WriteLine("Errore durante la chiamata all'API:");
                ////////        Console.WriteLine(ex.Message);
                ////////    }

                ////////}



                // LLama LOCAL
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



    #region npl file model
    //public class NlpService
    //{
    //    private readonly MLContext _mlContext;
    //    private readonly PredictionEngine<NlpInput, NlpOutput> _predictionEngine;

    //    public NlpService()
    //    {
    //        _mlContext = new MLContext();
    //        var modelPath = "modello_ner.onnx"; // 🔹 Usa il modello NLP in ONNX
    //        var pipeline = _mlContext.Transforms.ApplyOnnxModel(modelFile: modelPath);

    //        var model = pipeline.Fit(_mlContext.Data.LoadFromEnumerable(new List<NlpInput>()));
    //        _predictionEngine = _mlContext.Model.CreatePredictionEngine<NlpInput, NlpOutput>(model);
    //    }

    //    public NlpOutput AnalizzaTesto(string testo)
    //    {
    //        return _predictionEngine.Predict(new NlpInput { Testo = testo });
    //    }
    //}

    //public class NlpInput
    //{
    //    [ColumnName("input_text")]
    //    public string Testo { get; set; }
    //}

    //public class NlpOutput
    //{
    //    [ColumnName("predicted_label")]
    //    public string[] Etichette { get; set; }

    //    [ColumnName("predicted_entities")]
    //    public string[] Entita { get; set; }
    //}
    #endregion



    #region Nlp




    //public class FakeAI_Nlp
    //{

    //    private readonly MLContext _mlContext;
    //    private PredictionEngine<NlpInput, NlpOutput> _predictionEngine;

    //    public FakeAI_Nlp()
    //    {
    //        _mlContext = new MLContext();
    //        AddestraModello();
    //    }


    //    private void AddestraModello()
    //    {
    //        var dati = new List<NlpInput>
    //        {
    //            new() { Testo = "Paolo Rossi entra alle 9:00", TipoComando = "timbratura", NomePersona = "Paolo Rossi", Data = "", Orario = "09:00" },
    //            new() { Testo = "Mario Rossi entra alle 11:00", TipoComando = "timbratura", NomePersona = "Mario Rossi", Data = "", Orario = "11:00" },
    //            new() { Testo = "Luca Bianchi esce alle 18:30", TipoComando = "uscita", NomePersona = "Luca Bianchi", Data = "", Orario = "18:30" },
    //            new() { Testo = "Francesca Verdi ferie dal 15/03/2025 al 20/03/2025", TipoComando = "ferie", NomePersona = "Francesca Verdi", Data = "15/03/2025-20/03/2025", Orario = "" },
    //            new() { Testo = "Giulia Bianchi permesso il 10/04/2025", TipoComando = "permesso", NomePersona = "Giulia Bianchi", Data = "10/04/2025", Orario = "" },
    //            new() { Testo = "Stefano Neri malattia il 05/05/2025", TipoComando = "malattia", NomePersona = "Stefano Neri", Data = "05/05/2025", Orario = "" },
    //            // Righe aggiuntive
    //            new() { Testo = "Anna Rossi entra alle 8:15", TipoComando = "timbratura", NomePersona = "Anna Rossi", Data = "", Orario = "08:15" },
    //            new() { Testo = "Marco Verdi esce alle 17:45", TipoComando = "uscita", NomePersona = "Marco Verdi", Data = "", Orario = "17:45" },
    //            new() { Testo = "Giovanni Gialli malattia il 02/02/2025", TipoComando = "malattia", NomePersona = "Giovanni Gialli", Data = "02/02/2025", Orario = "" },
    //            new() { Testo = "Elisa Bianchi ferie dal 01/07/2025 al 10/07/2025", TipoComando = "ferie", NomePersona = "Elisa Bianchi", Data = "01/07/2025-10/07/2025", Orario = "" },
    //            new() { Testo = "Riccardo Neri permesso il 25/12/2025", TipoComando = "permesso", NomePersona = "Riccardo Neri", Data = "25/12/2025", Orario = "" },
    //            new() { Testo = "Silvia Rossi entra alle 14:00", TipoComando = "timbratura", NomePersona = "Silvia Rossi", Data = "", Orario = "14:00" },
    //            new() { Testo = "Davide Verdi esce alle 20:30", TipoComando = "uscita", NomePersona = "Davide Verdi", Data = "", Orario = "20:30" },
    //            new() { Testo = "Claudia Blu malattia il 15/03/2025", TipoComando = "malattia", NomePersona = "Claudia Blu", Data = "15/03/2025", Orario = "" },
    //            new() { Testo = "Luisa Gialli ferie dal 05/08/2025 al 20/08/2025", TipoComando = "ferie", NomePersona = "Luisa Gialli", Data = "05/08/2025-20/08/2025", Orario = "" },
    //            new() { Testo = "Andrea Rossi permesso il 30/06/2025", TipoComando = "permesso", NomePersona = "Andrea Rossi", Data = "30/06/2025", Orario = "" },

    //        };



    //        var dataView = _mlContext.Data.LoadFromEnumerable(dati);

    //        var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(NlpInput.Testo))
    //            // 🔹 Mappiamo le stringhe a chiavi numeriche
    //            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(NlpInput.TipoComando)))
    //            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label_Nome", nameof(NlpInput.NomePersona)))
    //            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label_Data", nameof(NlpInput.Data)))
    //            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label_Orario", nameof(NlpInput.Orario)))

    //            // 🔹 Un solo modello per tutti i campi
    //            .Append(_mlContext.Transforms.Concatenate("Features", "Features"))
    //            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())

    //            // 🔹 Riconvertiamo i numeri in stringhe
    //            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"))
    //            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel_Nome", "Label_Nome"))
    //            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel_Data", "Label_Data"))
    //            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel_Orario", "Label_Orario"))

    //            .Append(_mlContext.Transforms.Text.FeaturizeText("TestoFeatures", nameof(NlpInput.Testo)))
    //            .Append(_mlContext.Transforms.Categorical.OneHotEncoding("OrarioEncoded", nameof(NlpInput.Orario)))
    //            .Append(_mlContext.Transforms.Concatenate("Features", "TestoFeatures", "OrarioEncoded"));

    //        var modello = pipeline.Fit(dataView);
    //        _predictionEngine = _mlContext.Model.CreatePredictionEngine<NlpInput, NlpOutput>(modello);
    //    }


    //    public NlpOutput Predici(string testo)
    //    {
    //        var input = new NlpInput { Testo = testo };
    //        return _predictionEngine.Predict(input);
    //    }

    //    public object EstrarreDati(string testo)
    //    {
    //        var risultato = this.Predici(testo);

    //        // Formattiamo il JSON in base al tipo di comando
    //        object jsonRisultato = risultato.TipoComando switch
    //        {
    //            "timbratura" => new
    //            {
    //                Tipo = "timbratura",
    //                NomePersona = risultato.NomePersona,
    //                OrarioEntrata = risultato.Orario
    //            },
    //            "ferie" => new
    //            {
    //                Tipo = "ferie",
    //                NomePersona = risultato.NomePersona,
    //                DataInizio = risultato.Data.Split('-')[0],
    //                DataFine = risultato.Data.Split('-')[1]
    //            },
    //            "permesso" => new
    //            {
    //                Tipo = "permesso",
    //                NomePersona = risultato.NomePersona,
    //                Data = risultato.Data
    //            },
    //            _ => new { Errore = "Comando non riconosciuto" }
    //        };

    //        return jsonRisultato;
    //    }

    //}

    //public class NlpInput
    //{
    //    [LoadColumn(0)] public string Testo { get; set; } = string.Empty;
    //    [LoadColumn(1)] public string TipoComando { get; set; } = string.Empty;
    //    [LoadColumn(2)] public string NomePersona { get; set; } = string.Empty;
    //    [LoadColumn(3)] public string Data { get; set; } = string.Empty;
    //    [LoadColumn(4)] public string Orario { get; set; } = string.Empty;
    //}

    //public class NlpOutput
    //{
    //    [ColumnName("PredictedLabel")] public string TipoComando { get; set; } = string.Empty;
    //    [ColumnName("PredictedLabel_Nome")] public string NomePersona { get; set; } = string.Empty;
    //    [ColumnName("PredictedLabel_Data")] public string Data { get; set; } = string.Empty;
    //    [ColumnName("PredictedLabel_Orario")] public string Orario { get; set; } = string.Empty;
    //}

    #endregion


    public class FakeAI_Regex
    {
        private List<Regex> Clockign_ENT; 

        public FakeAI_Regex()
        {
            Clockign_ENT = new List<Regex>();

            // mario rossi entra alle 
            //Clockign_ENT.Add(new Regex(@"(?i)([a-zA-Z\s]+)\s(?:entrata|ent|uscita|usc)(?:\salle)?\s(\d{1,2}[:\.]\d{2})"));
            //Clockign_ENT.Add(new Regex(@"([a-zA-Z\s]+)\s(entrata|ent|uscita|usc)(?:\salle)?\s(\d{1,2}[:\.]\d{2})"));
            Clockign_ENT.Add(new Regex(@"([a-zA-Z\s]+)\s(entrata|entra|ent|uscita|esce|usc)(?:\salle)?\s(\d{1,2}(?:([:\.]\d{2})|(?:\se\s\d{1,2})))"));
            //Clockign_ENT.Add(new Regex(@"([a-zA-Z\s]+)\s(entrata|ent|uscita|usc)(?:\salle)?\s(\d{1,2}(([:\.]\d{2})|(\se\s\d{1,2})))"));
        }

        public List<ClockignCommand>  GetClockignCommand(string text)
        {
            List<ClockignCommand> clockignCommands = new List<ClockignCommand>();

            foreach(var item in Clockign_ENT)
            {
                if (item.IsMatch(text))
                {
                    var match = item.Match(text);
                    clockignCommands.Add(new ClockignCommand()
                    {
                         action = match.Groups[2].Value.ToString(),
                         dipendente = match.Groups[1].Value.ToString(),
                         orario = match.Groups[3].Value.ToString(),
                         type="timbratura"

                    });
                }
            }


            return clockignCommands;
        }

    }

    public class ClockignCommand()
    {
        public string action { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string orario { get; set; } = string.Empty;
        public string dipendente { get; set; } = string.Empty;

    }



    public class OllamaResponse
    {
        public string Model { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty; // Qui dentro ci sarà il JSON della timbratura
        public bool Done { get; set; } = false;
    }

}
