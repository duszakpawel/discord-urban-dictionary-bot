namespace DiscordUrbanDictionaryBot.Service.Client
{
    public interface IUrbanDictionaryClient
    {
        Task<UrbanDictionaryResponse?> GetDefinitions(string phrase);
    }
}
