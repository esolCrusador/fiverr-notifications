using FiverrNotifications.Client.Models;
using FiverrNotifications.Logic.Clients;
using FiverrNotifications.Logic.Exceptions;
using FiverrNotifications.Logic.Models;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FiverrNotifications.Client
{
    public class FiverrClient : IFiverrClient
    {
        private readonly HttpClient _httpClient;
        private readonly CookieContainer _cookieContainer;
        public FiverrClient() => _httpClient = new HttpClient();

        public void Dispose() => _httpClient.Dispose();

        public async Task<IReadOnlyCollection<FiverrRequest>> GetRequsts(string userName, Guid session, string token)
        {
            string url = $"https://www.fiverr.com/users/{userName}/requests?_=1600093026817";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetHeaders(request, userName, session, token);
            
            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.InternalServerError)
                    throw new WrongCredentialsException();

                throw new HttpRequestException($"Request {HttpMethod.Get} ${url} failed.\r\nWith status: {response.StatusCode}.\r\nBody: {await response.Content.ReadAsStringAsync()}");
            }

            var responseText = await response.Content.ReadAsStringAsync();
            var responseModel = JsonConvert.DeserializeObject<FiverrResponse>(responseText);
            return responseModel.Map();
        }

        private static void SetHeaders(HttpRequestMessage request, string userName, Guid session, string token)
        {
            var cookies = new[]
                {
                    new KeyValuePair<string, string>("was_logged_in", Uri.EscapeDataString($"1;{userName}")),
                    new KeyValuePair<string, string>("_fiverr_session_key", session.ToString("N")),
                    new KeyValuePair<string, string>("hodor_creds", token),
                };

            request.Headers.TryAddWithoutValidation(HeaderNames.Accept, "text/javascript");
            request.Headers.TryAddWithoutValidation(HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.83 Safari/537.36");
            request.Headers.TryAddWithoutValidation(HeaderNames.AcceptLanguage, "en-US");
            request.Headers.TryAddWithoutValidation(HeaderNames.Cookie, string.Join("; ", cookies.Select(kvp => $"{kvp.Key}={kvp.Value}")));
        }
    }
}
