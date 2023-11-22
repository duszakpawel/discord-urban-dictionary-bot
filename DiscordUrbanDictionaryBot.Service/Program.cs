using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using DiscordUrbanDictionaryBot.Service.Client;
using DiscordUrbanDictionaryBot.Service.Command;
using DiscordUrbanDictionaryBot.Service.Modules;
using DiscordUrbanDictionaryBot.Service.Options;
using DiscordUrbanDictionaryBot.Service.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

using IHost host = Host.CreateDefaultBuilder(args)
.ConfigureAppConfiguration(config =>
{
    config
        .AddJsonFile("local.settings.json", true, true)
        .AddEnvironmentVariables();
})
.ConfigureServices((host, services) =>
{
    var socketConfig = new DiscordSocketConfig()
    {
        GatewayIntents = GatewayIntents.MessageContent | GatewayIntents.AllUnprivileged
    };

    services
        .AddSingleton(socketConfig)
        .AddSingleton<DictionaryModule>()
        .AddSingleton(new DiscordSocketClient(socketConfig))
        .AddSingleton<DiscordRestClient>()
        .AddScoped<IUrbanDictionaryClient, UrbanDictionaryClient>();

    services
        .AddHttpClient<UrbanDictionaryClient>();

    services.AddTransient<IUrbanDictionaryCommand, UrbanDictionaryCommand>();

    services
        .Configure<Secrets>(host.Configuration.GetSection(nameof(Secrets)));

    services.AddLogging(builder =>
    {
        builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "MM/dd hh:mm:ss tt ";
        });

        if (!Debugger.IsAttached)
        {
            builder.AddFile("DiscordChatGPT{Date}.log");
            builder.AddFilter(null, LogLevel.Information);
        }
    });
})
.Build();

await ServiceLifetime(host.Services);

await host.RunAsync();

static async Task ServiceLifetime(IServiceProvider serviceProvider)
{
    var socketClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
    var restClient = serviceProvider.GetRequiredService<DiscordRestClient>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Starting bot...");

    var interactionService = new InteractionService(socketClient);

    var token = serviceProvider
        .GetRequiredService<IConfiguration>()
        .GetRequiredSection("Secrets")
        .GetRequiredValue<string>("DiscordToken");

    socketClient.LoggedIn += () =>
    {
        logger.LogInformation("Discord client logged in");
        return Task.CompletedTask;
    };

    socketClient.Disconnected += ex =>
    {
        logger.LogWarning(ex, "Discord client disconnected");
        return Task.CompletedTask;
    };

    socketClient.Log += msg =>
    {
        switch (msg.Severity)
        {
            case LogSeverity.Critical:
            case LogSeverity.Error:
                logger.LogError(msg.Exception, msg.Message);
                break;
            case LogSeverity.Warning:
                logger.LogWarning(msg.Exception, msg.Message);
                break;
            case LogSeverity.Info:
                logger.LogInformation(msg.Exception, msg.Message);
                break;
            case LogSeverity.Verbose:
            case LogSeverity.Debug:
                logger.LogDebug(msg.Exception, msg.Message);
                break;
        }

        return Task.CompletedTask;
    };

    socketClient.SlashCommandExecuted += interaction =>
    {
        if (interaction.User.IsBot)
        {
            logger.LogDebug("Ignoring interaction from bot {BotName} {Command}", interaction.User.Username, interaction.Data.Name);
            return Task.CompletedTask;
        }

        var ctx = new SocketInteractionContext(socketClient, interaction);
        ThreadPool.QueueUserWorkItem(async _ =>
        {
            logger.LogInformation("Executing interaction {Command} from {User}", interaction.Data.Name, interaction.User.Username);
            await interactionService.ExecuteCommandAsync(ctx, serviceProvider);
        });

        return Task.CompletedTask;
    };

    socketClient.Ready += async () =>
    {
        await interactionService.RegisterCommandsGloballyAsync(true);
        logger.LogInformation("Bot is ready and commands are registered.");
    };

    var restLoginTask = restClient.LoginAsync(TokenType.Bot, token);
    var socketLoginTask = socketClient.LoginAsync(TokenType.Bot, token);
    var addModulesTask = interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);

    await Task.WhenAll(addModulesTask, restLoginTask, socketLoginTask);
    await socketClient.StartAsync();
}