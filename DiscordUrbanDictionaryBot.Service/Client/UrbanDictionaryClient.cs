using DiscordUrbanDictionaryBot.Service.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DiscordUrbanDictionaryBot.Service.Client
{
    public class UrbanDictionaryClient : IUrbanDictionaryClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UrbanDictionaryClient> _logger;
        private readonly IOptions<Secrets> _secrets;

        public UrbanDictionaryClient(HttpClient httpClient, ILogger<UrbanDictionaryClient> logger, IOptions<Secrets> secrets)
        {
            _httpClient = httpClient;
            _logger = logger;
            _secrets = secrets;

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DiscordBot");
            _httpClient.BaseAddress = new Uri(_secrets.Value.UrbanDictEndpoint);
        }

        public async Task<UrbanDictionaryResponse?> GetDefinitions(string phrase)
        {
            phrase = Uri.EscapeDataString(phrase);

            var requestUri = $"define?term={phrase}";
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri));

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UrbanDictionaryResponse>();
            }
            else
            {
                _logger.LogError("HTTP request failed with status code: {StatusCode}", response.StatusCode);

                return null;
            }
        }
    }
}
