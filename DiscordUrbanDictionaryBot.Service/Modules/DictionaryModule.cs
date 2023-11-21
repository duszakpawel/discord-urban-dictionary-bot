using Discord.Commands;
using Discord.Interactions;
using DiscordUrbanDictionaryBot.Service.Command;
using Summary = Discord.Interactions.SummaryAttribute;

namespace DiscordUrbanDictionaryBot.Service.Modules;

public class DictionaryModule(IUrbanDictionaryCommand command) : InteractionModuleBase
{

    [SlashCommand("urbandictionary", "Print the first definition found on urban dictionary for a given phrase")]
    public async Task UrbanDictionary([Summary("phrase", "the phrase you want to look up in the dictionary")] string phrase)
    {
        var deferTask = DeferAsync(false);
        
        var dictionaryResult = await command.ExecuteAsync(phrase);
        var messageToPrint = string.Join(Environment.NewLine, dictionaryResult.Messages);

        await deferTask;

        await ModifyOriginalResponseAsync(r =>
        {
            r.Content = messageToPrint;
        });
    }
}