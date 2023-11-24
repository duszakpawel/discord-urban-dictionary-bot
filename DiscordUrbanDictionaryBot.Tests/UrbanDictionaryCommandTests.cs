using DiscordUrbanDictionaryBot.Service.Client;
using DiscordUrbanDictionaryBot.Service.Command;
using Moq;

namespace DiscordUrbanDictionaryBot.Tests
{
    [TestFixture]
    public class UrbanDictionaryCommandTests
    {
        private Mock<IUrbanDictionaryClient> _clientMock;

        [SetUp]
        public void Setup()
        {
            _clientMock = new Mock<IUrbanDictionaryClient>();
        }

        [Test]
        public async Task ExecuteAsync_EmptyResponse_ReturnsNoDefinitionFound()
        {
            var phrase = "test";
            _clientMock.Setup(x => x.GetDefinitions(phrase)).ReturnsAsync(new UrbanDictionaryResponse { List = null });

            var command = new UrbanDictionaryCommand(_clientMock.Object);

            var result = await command.ExecuteAsync(phrase);

            Assert.NotNull(result);
            Assert.IsFalse(result.Success);
            Assert.That(result.Messages.Count(), Is.EqualTo(1));
            Assert.Contains($"No definition found for {phrase}", result.Messages.ToList());
        }

        [Test]
        public async Task ExecuteAsync_SingleResult_ReturnsFormattedResponse()
        {
            var phrase = "test";
            var definition = new UrbanDictionaryItem { Definition = "This is a test definition" };
            var response = new UrbanDictionaryResponse { List = new List<UrbanDictionaryItem>() { definition } };
            _clientMock.Setup(x => x.GetDefinitions(phrase)).ReturnsAsync(response);

            var command = new UrbanDictionaryCommand(_clientMock.Object);

            var result = await command.ExecuteAsync(phrase);

            Assert.NotNull(result);
            Assert.IsTrue(result.Success);
            Assert.That(result.Messages.Count(), Is.EqualTo(2));
            Assert.Contains(definition.Definition, result.Messages.ToList());
        }

        [Test]
        public async Task ExecuteAsync_HttpRequestException_ReturnsError()
        {
            var phrase = "test";
            _clientMock.Setup(x => x.GetDefinitions(phrase)).ThrowsAsync(new HttpRequestException());

            var command = new UrbanDictionaryCommand(_clientMock.Object);

            var result = await command.ExecuteAsync(phrase);

            Assert.NotNull(result);
            Assert.IsFalse(result.Success);
            Assert.That(result.Messages.Count(), Is.EqualTo(1));
            Assert.Contains("Error making the request to Urban Dictionary API", result.Messages.ToList());
        }

        [Test]
        public async Task ExecuteAsync_OtherException_ReturnsError()
        {
            var phrase = "test";
            _clientMock.Setup(x => x.GetDefinitions(phrase)).ThrowsAsync(new Exception("Test exception"));

            var command = new UrbanDictionaryCommand(_clientMock.Object);

            var result = await command.ExecuteAsync(phrase);

            Assert.NotNull(result);
            Assert.IsFalse(result.Success);
            Assert.That(result.Messages.Count(), Is.EqualTo(1));
            Assert.Contains("An error occurred: Test exception", result.Messages.ToList());
        }
    }
}