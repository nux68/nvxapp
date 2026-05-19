using Newtonsoft.Json;
using nvxapp.server.service.Interfaces;
using Serilog;
using System.Net;
using System.Net.Http.Headers;




namespace nvxapp.server.service.Helpers
{

    public static class WebApiService
{
    public static async Task<WebApiResult<TOut>> Post<TIn, TOut>(
        IHttpClientFactory httpClientFactory,
        string serverUrl,
        AuthenticationHeaderValue? authentication,
        string action,
        TIn body,
        WebApiBodyType bodyType,
        IDictionary<string, string> headers,
        bool returnRawData = false)
    {
        return await Fetch<TIn, TOut>(httpClientFactory, serverUrl, authentication, action, body, bodyType, headers, WebApiRequestType.Post, returnRawData);
    }

    public static async Task<WebApiResult<TOut>> Put<TIn, TOut>(
        IHttpClientFactory httpClientFactory,
        string serverUrl,
        AuthenticationHeaderValue? authentication,
        string action,
        TIn body,
        WebApiBodyType bodyType,
        IDictionary<string, string> headers)
    {
        return await Fetch<TIn, TOut>(httpClientFactory, serverUrl, authentication, action, body, bodyType, headers, WebApiRequestType.Put);
    }

    public static async Task<WebApiResult<TOut>> Get<TOut>(
        IHttpClientFactory httpClientFactory,
        string serverUrl,
        AuthenticationHeaderValue? authentication,
        string action,
        IDictionary<string, string> headers)
    {
        return await Fetch<string, TOut>(httpClientFactory, serverUrl, authentication, action, "", WebApiBodyType.none, headers, WebApiRequestType.Get);
    }

    public static async Task<WebApiResult<TOut>> Delete<TOut>(
        IHttpClientFactory httpClientFactory,
        string serverUrl,
        AuthenticationHeaderValue? authentication,
        string action,
        IDictionary<string, string> headers)
    {
        return await Fetch<string, TOut>(httpClientFactory, serverUrl, authentication, action, "", WebApiBodyType.none, headers, WebApiRequestType.Delete);
    }

    private static async Task<WebApiResult<TOut>> Fetch<TIn, TOut>(
        IHttpClientFactory httpClientFactory,
        string serverUrl,
        AuthenticationHeaderValue? authentication,
        string action,
        TIn body,
        WebApiBodyType bodyType,
        IDictionary<string, string> headers,
        WebApiRequestType type,
        bool returnRawData = false)
    {
        var r = new WebApiResult<TOut>
        {
            StatusCode = HttpStatusCode.OK,
            ReasonPhrase = string.Empty
        };

        try
        {
            var serverUri = new Uri(serverUrl);

            var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(serverUri, action);
            httpClient.MaxResponseContentBufferSize = int.MaxValue;
            httpClient.DefaultRequestHeaders.Clear();

            if (headers != null)
            {
                foreach (var h in headers)
                    httpClient.DefaultRequestHeaders.Add(h.Key, h.Value);
            }

            if (authentication != null)
                httpClient.DefaultRequestHeaders.Authorization = authentication;

            HttpContent? content;

            switch (bodyType)
            {
                case WebApiBodyType.none:
                    content = null;
                    break;

                case WebApiBodyType.x_www_form_urlencoded:
                    if (body is IEnumerable<KeyValuePair<string, string>> formData)
                    {
                        content = new FormUrlEncodedContent(formData);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    }
                    else
                    {
                        throw new InvalidOperationException("Body must be IEnumerable<KeyValuePair<string,string>> for x-www-form-urlencoded.");
                    }
                    break;

                case WebApiBodyType.raw:
                default:
                    string json = body == null ? string.Empty : JsonConvert.SerializeObject(body);
                    content = new StringContent(json);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    content.Headers.ContentType.CharSet = "UTF-8";
                    break;
            }

            HttpResponseMessage result = type switch
            {
                WebApiRequestType.Get    => await httpClient.GetAsync(httpClient.BaseAddress.AbsoluteUri),
                WebApiRequestType.Put    => await httpClient.PutAsync(httpClient.BaseAddress.AbsoluteUri, content),
                WebApiRequestType.Delete => await httpClient.DeleteAsync(httpClient.BaseAddress.AbsoluteUri),
                _                        => await httpClient.PostAsync(httpClient.BaseAddress.AbsoluteUri, content),
            };

            r = new WebApiResult<TOut>
            {
                StatusCode   = result.StatusCode,
                ReasonPhrase = result.ReasonPhrase
            };

            if (!result.IsSuccessStatusCode) return r;

            if (typeof(TOut) == typeof(byte[]))
            {
                r.GetType().GetProperty(nameof(r.Data))?.SetValue(r, await result.Content.ReadAsByteArrayAsync());
            }
            else
            {
                var data = await result.Content.ReadAsStringAsync();

                if (typeof(TOut) == typeof(string) && !data.StartsWith("\"") && !data.EndsWith("\""))
                    data = $"\"{data}\"";

                if (returnRawData)
                    r.RawData = data;
                else
                    r.Data = JsonConvert.DeserializeObject<TOut>(data);
            }

            return r;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "WebApiService - Fetch: Errore generico");
            System.Diagnostics.Debug.WriteLine(ex.Message);
            throw;
        }
    }
}
    //public class WebApiService : IWebApiService
    //{
    //    //private readonly AppSetting _appSetting;
    //    private readonly IHttpClientFactory _httpClientFactory;

    //    //private string _currentUser;
    //    //public string CurrentUser
    //    //{
    //    //    get { return _currentUser; }
    //    //    set { _currentUser = value; }
    //    //}


    //    public WebApiService(
    //        //IOptions<AppSetting> appSetting,
    //        IHttpClientFactory httpClientFactory
    //        )
    //    {
    //        //_appSetting = appSetting.Value;
    //        _httpClientFactory = httpClientFactory;
    //    }


    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <typeparam name="TIn"></typeparam>
    //    /// <typeparam name="TOut"></typeparam>
    //    /// <param name="serverUrl"></param>
    //    /// <param name="authentication"></param>
    //    /// <param name="action"></param>
    //    /// <param name="body"></param>
    //    /// <param name="bodyType"></param>
    //    /// <param name="headers"></param>
    //    /// <param name="returnRawData">Restituisce i dati grezzi senza deserializzarli dal json</param>
    //    /// <returns></returns>
    //    public async Task<WebApiResult<TOut>> Post<TIn, TOut>(string serverUrl, AuthenticationHeaderValue? authentication,
    //                                                  string action,
    //                                                  TIn body,
    //                                                  WebApiBodyType bodyType,
    //                                                  IDictionary<string, string> headers,
    //                                                  bool returnRawData = false)
    //    {
    //        return await Fetch<TIn, TOut>(serverUrl, authentication, action, body, bodyType, headers, WebApiRequestType.Post, returnRawData);
    //    }


    //    public async Task<WebApiResult<TOut>> Put<TIn, TOut>(string serverUrl, AuthenticationHeaderValue? authentication,
    //                                                  string action,
    //                                                  TIn body,
    //                                                  WebApiBodyType bodyType,
    //                                                  IDictionary<string, string> headers)
    //    {
    //        return await Fetch<TIn, TOut>(serverUrl, authentication,
    //                                      action,
    //                                      body,
    //                                      bodyType,
    //                                      headers,
    //                                      WebApiRequestType.Put);
    //    }


    //    public async Task<WebApiResult<TOut>> Get<TOut>(string serverUrl, AuthenticationHeaderValue? authentication,
    //                                                     string action,
    //                                                     IDictionary<string, string> headers)
    //    {

    //        return await Fetch<string, TOut>(serverUrl, authentication,
    //                                          action,
    //                                          "",
    //                                          WebApiBodyType.none,
    //                                          headers,
    //                                          WebApiRequestType.Get);
    //    }

    //    public async Task<WebApiResult<TOut>> Delete<TOut>(string serverUrl, AuthenticationHeaderValue? authentication,
    //                                             string action,
    //                                             IDictionary<string, string> headers)
    //    {

    //        return await Fetch<string, TOut>(serverUrl, authentication,
    //                                          action,
    //                                          "",
    //                                          WebApiBodyType.none,
    //                                          headers,
    //                                          WebApiRequestType.Delete);
    //    }



    //    private async Task<WebApiResult<TOut>> Fetch<TIn, TOut>(string serverUrl, AuthenticationHeaderValue? authentication,
    //        string action, TIn body, WebApiBodyType bodyType, IDictionary<string, string> headers, WebApiRequestType type, bool returnRawData = false)
    //    {

    //        var r = new WebApiResult<TOut>
    //        {
    //            StatusCode = HttpStatusCode.OK,
    //            ReasonPhrase = string.Empty
    //        };

    //        try
    //        {

    //            var _serverUri = new Uri(serverUrl);


    //            // instance the http client
    //            var httpClient = _httpClientFactory.CreateClient();


    //            httpClient.BaseAddress = new Uri(_serverUri, action);
    //            httpClient.MaxResponseContentBufferSize = int.MaxValue;

    //            // clean the headers
    //            httpClient.DefaultRequestHeaders.Clear();

    //            // add custom headers
    //            if (headers != null)
    //            {
    //                foreach (var h in headers)
    //                {
    //                    httpClient.DefaultRequestHeaders.Add(h.Key, h.Value);
    //                }
    //            }

    //            // set the authentifications
    //            if (authentication != null)
    //                httpClient.DefaultRequestHeaders.Authorization = authentication;


    //            HttpResponseMessage result;
    //            HttpContent? content;

    //            switch (bodyType)
    //            {
    //                case WebApiBodyType.none:
    //                    content = null;
    //                    break;

    //                case WebApiBodyType.x_www_form_urlencoded:
    //                    if (body is IEnumerable<KeyValuePair<string, string>> formData)
    //                    {
    //                        content = new FormUrlEncodedContent(formData);
    //                        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
    //                    }
    //                    else
    //                    {
    //                        throw new InvalidOperationException("Body must be IEnumerable<KeyValuePair<string,string>> for x-www-form-urlencoded.");
    //                    }
    //                    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
    //                    break;

    //                case WebApiBodyType.raw:
    //                default:

    //                    string json = body == null ? string.Empty : JsonConvert.SerializeObject(body);
    //                    content = new StringContent(json);
    //                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
    //                    content.Headers.ContentType.CharSet = "UTF-8";
    //                    break;
    //            }



    //            switch (type)
    //            {
    //                case WebApiRequestType.Get:
    //                    result = httpClient.GetAsync(httpClient.BaseAddress.AbsoluteUri).Result;
    //                    break;

    //                case WebApiRequestType.Put:
    //                    result = httpClient.PutAsync(httpClient.BaseAddress.AbsoluteUri, content).Result;
    //                    break;

    //                case WebApiRequestType.Delete:
    //                    result = httpClient.DeleteAsync(httpClient.BaseAddress.AbsoluteUri).Result;
    //                    break;

    //                case WebApiRequestType.Post:
    //                default:
    //                    result = httpClient.PostAsync(httpClient.BaseAddress.AbsoluteUri, content).Result;
    //                    break;
    //            }

    //            r = new WebApiResult<TOut>
    //            {
    //                StatusCode = result!.StatusCode,
    //                ReasonPhrase = result?.ReasonPhrase
    //            };


    //            if (result == null || !result.IsSuccessStatusCode) return r;


    //            if (typeof(TOut) == typeof(byte[]))
    //            {
    //                // set a byte array response
    //                r?.GetType()?.GetProperty(nameof(r.Data))?.SetValue(r, await result.Content.ReadAsByteArrayAsync());
    //            }
    //            else
    //            {
    //                // get the response
    //                var data = await result.Content.ReadAsStringAsync();

    //                // deserialize the response
    //                if (typeof(TOut) == typeof(string) && !data.StartsWith("\"") && !data.EndsWith("\""))
    //                    data = $"\"{data}\"";

    //                if (returnRawData)
    //                    r.RawData = data;
    //                else
    //                    r.Data = JsonConvert.DeserializeObject<TOut>(data);
    //            }

    //            return r;

    //        }
    //        catch (Exception ex)
    //        {
    //            Log.Error(ex, "WebApiService - Fetch: Errore generico");
    //            System.Diagnostics.Debug.WriteLine(ex.Message);
    //            throw;
    //        }

    //    }






    //}




    //public interface IWebApiService : IServiceBase
    //{

    //    public Task<WebApiResult<TOut>> Post<TIn, TOut>(string serverUrl, AuthenticationHeaderValue? authentication,
    //                                                  string action,
    //                                                  TIn body,
    //                                                  WebApiBodyType bodyType,
    //                                                  IDictionary<string, string> headers,
    //                                                  bool returnRawData = false);

    //    public Task<WebApiResult<TOut>> Put<TIn, TOut>(string serverUrl, AuthenticationHeaderValue? authentication,
    //                                                  string action,
    //                                                  TIn body,
    //                                                  WebApiBodyType bodyType,
    //                                                  IDictionary<string, string> headers);

    //    public Task<WebApiResult<TOut>> Get<TOut>(string serverUrl, AuthenticationHeaderValue? authentication,
    //                                             string action,
    //                                             IDictionary<string, string> headers);

    //    public Task<WebApiResult<TOut>> Delete<TOut>(string serverUrl, AuthenticationHeaderValue? authentication,
    //                                     string action,
    //                                     IDictionary<string, string> headers);
    //}


    public class WebApiResult<T>
    {

        public HttpStatusCode? StatusCode { get; set; }

        public string? ReasonPhrase { get; set; }

        public T? Data { get; set; }

        public string? RawData { get; set; } // dati grezzi 
    }

    public enum WebApiBodyType
    {
        none,
        x_www_form_urlencoded,
        raw,
    }
    public enum WebApiRequestType
    {
        Get,
        Post,
        Put,
        Delete
    }

}
