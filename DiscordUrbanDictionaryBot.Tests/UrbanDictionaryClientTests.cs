using DiscordUrbanDictionaryBot.Service.Client;
using DiscordUrbanDictionaryBot.Service.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;

namespace DiscordUrbanDictionaryBot.Tests
{
    public class UrbanDictionaryClientTests
    {
        [Test]
        public async Task GetDefinitions_ValidPhrase_ReturnsResponse()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"definition\": \"test definition\"}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var logger = new Mock<ILogger<UrbanDictionaryClient>>().Object;
            var secretsOptions = Options.Create(new Secrets { UrbanDictEndpoint = "https://example.com" });
            var urbanDictionaryClient = new UrbanDictionaryClient(httpClient, logger, secretsOptions);

            var response = await urbanDictionaryClient.GetDefinitions("test");

            Assert.IsNotNull(response);
        }

        [Test]
        public async Task GetDefinitions_InvalidPhrase_ReturnsNullAndLogsError()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var capturedLogEntries = new List<string>();
            var loggerMock = new Mock<ILogger<UrbanDictionaryClient>>();
            loggerMock
                .Setup(x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()))
                .Callback(new InvocationAction(invocation =>
                {
                    var logMessage = invocation.Arguments[2].ToString();
                    capturedLogEntries.Add(logMessage);
                }));

            var logger = loggerMock.Object;
            var secretsOptions = Options.Create(new Secrets { UrbanDictEndpoint = "https://example.com" });
            var urbanDictionaryClient = new UrbanDictionaryClient(httpClient, logger, secretsOptions);

            var response = await urbanDictionaryClient.GetDefinitions("invalid_phrase");

            Assert.That(response, Is.Null);
            Assert.That(capturedLogEntries.Any(entry => entry.Contains("HTTP request failed with status code: InternalServerError")), Is.True);
        }



        [Test]
        public void ConfigureHttpClient_ValidSecrets_CorrectConfiguration()
        {
            var httpClient = new HttpClient();
            var logger = new Mock<ILogger<UrbanDictionaryClient>>().Object;
            var secretsOptions = Options.Create(new Secrets { UrbanDictEndpoint = "https://example.com" });
            _ = new UrbanDictionaryClient(httpClient, logger, secretsOptions);

            Assert.Multiple(() =>
            {
                Assert.That(httpClient.DefaultRequestHeaders.Accept.First().MediaType, Is.EqualTo("application/json"));
                Assert.That(httpClient.DefaultRequestHeaders.UserAgent.ToString(), Is.EqualTo("DiscordBot"));
                Assert.That(httpClient.BaseAddress, Is.EqualTo(new Uri(secretsOptions.Value.UrbanDictEndpoint)));
            });
        }
    }
}
