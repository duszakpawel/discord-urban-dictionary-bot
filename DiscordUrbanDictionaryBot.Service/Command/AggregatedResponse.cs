namespace DiscordUrbanDictionaryBot.Service.Command
{
    public class AggregatedResponse(bool success, IEnumerable<string> messages)
    {
        public bool Success { get; set; } = success;
        public IEnumerable<string> Messages { get; set; } = messages;
    }
}
