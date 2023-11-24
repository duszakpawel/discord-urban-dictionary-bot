using DiscordUrbanDictionaryBot.Service.Utility;
using Microsoft.Extensions.Configuration;

namespace DiscordUrbanDictionaryBot.Tests
{
    public class Tests
    {
        [TestFixture]
        public class ConfigurationExtensionsTests
        {
            private IConfiguration _configuration;

            [SetUp]
            public void Setup()
            {
                var configBuilder = new ConfigurationBuilder()
                    .AddInMemoryCollection(
                    new Dictionary<string, string>()
                    {
                    { "MySection:MyKey", "SomeValue" }
                    });

                _configuration = configBuilder.Build();
            }

            [Test]
            public void GetRequiredValue_ExistingKey_ReturnsValue()
            {
                var section = _configuration.GetSection("MySection");
                var key = "MyKey";

                var value = section.GetRequiredValue<string>(key);

                Assert.That(value, Is.EqualTo("SomeValue"));
            }

            [Test]
            public void GetRequiredValue_MissingKey_ThrowsConfigurationException()
            {
                var section = _configuration.GetSection("MySection");
                var key = "MissingKey";

                Assert.Throws<ConfigurationException>(() => section.GetRequiredValue<string>(key));
            }

            [Test]
            public void GetRequiredValue_MissingSection_ThrowsConfigurationException()
            {
                var section = _configuration.GetSection("MissingSection");
                var key = "AnyKey";

                Assert.Throws<ConfigurationException>(() => section.GetRequiredValue<string>(key));
            }

            [Test]
            public void GetRequiredValue_IncorrectValueType_ThrowsInvalidOperationException()
            {
                var section = _configuration.GetSection("MySection");
                var key = "MyKey";

                Assert.Throws<InvalidOperationException>(() => section.GetRequiredValue<int>(key));
            }
        }
    }
}