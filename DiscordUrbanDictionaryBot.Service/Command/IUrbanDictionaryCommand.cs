namespace DiscordUrbanDictionaryBot.Service.Command
{
    public interface IUrbanDictionaryCommand
    {
        Task<AggregatedResponse> ExecuteAsync(string phrase);
    }
}
