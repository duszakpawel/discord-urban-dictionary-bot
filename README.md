# Summary
This is a discord bot that retrieves definitions from Urban Dictionary based on user commands.

# Setup:
Before cloning the repo, register new application at `https://discord.com/developers/applications`, then clone the repo.

After cloning the repo, in `/DiscordUrbanDictionaryBot.Service` create a `local.settings.json` file with the following template:
```json
{
  "Secrets": {
    "DiscordToken": "your-discord-token",
    "UrbanDictEndpoint": "http://api.urbandictionary.com/v0/"
  }
}
```

At the root, create a `.env` file with the following contents:
```bash
    BASE_IMAGE_NAME=discord-urban-dictionary-bot
    VERSION=1.0.0
    DISCORD_TOKEN=your-discord-token
    URBAN_DICT_ENDPOINT=http://api.urbandictionary.com/v0/
```

After that you can run project in Visual Studio or run docker compose with docker compose up -d from a terminal at the root of the project.
If setup was successful you should now have a container named `DiscordUrbanDictionaryBot` running in docker.

# Demo
![ss1](https://github.com/duszakpawel/discord-urban-dictionary-bot/assets/17085237/758c0a6e-6f0e-4fc2-8ce7-46032345b62a)
![ss2](https://github.com/duszakpawel/discord-urban-dictionary-bot/assets/17085237/83a3b44d-e65b-4fba-8e75-8979dc79a488)
![ss3](https://github.com/duszakpawel/discord-urban-dictionary-bot/assets/17085237/45668032-16f4-4ffd-b666-0a5ee27f72fc)
![ss4](https://github.com/duszakpawel/discord-urban-dictionary-bot/assets/17085237/80ad0f9e-5243-4812-9a65-224b69cd0e16)


