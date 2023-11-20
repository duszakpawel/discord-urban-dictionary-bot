using DiscordUrbanDictionaryBot.Service.Client;

namespace DiscordUrbanDictionaryBot.Service.Command
{
    public class UrbanDictionaryCommand(IUrbanDictionaryClient client) : IUrbanDictionaryCommand
    {
        private readonly IUrbanDictionaryClient _client = client;

        public async Task<AggregatedResponse> ExecuteAsync(string phrase)
        {
            try
            {
                var result = await _client.GetDefinitions(phrase);

                var responses = new List<string>();

                if (result?.List == null || result.List.Count == 0)
                {
                    return new AggregatedResponse(false, [$"No definition found for {phrase}"]);
                }
                else
                {
                    responses.Add($"_What is {phrase}? According to urban dictionary:_");
                    responses.Add(result.List[0].Definition);

                    return new AggregatedResponse(true, responses);
                }
            }
            catch (HttpRequestException)
            {
                return new AggregatedResponse(false, ["Error making the request to Urban Dictionary API"]);
            }
            catch (Exception ex)
            {
                return new AggregatedResponse(false, [$"An error occurred: {ex.Message}"]);
            }
        }
    }
}
