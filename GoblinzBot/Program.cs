using System.Text.Json;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using GoblinzBot.Model;

// Read config file
string text = File.ReadAllText(@"../config/config.json");
Config config = JsonSerializer.Deserialize<Config>(text);

Random rdn = new();

DiscordClient discord = new(new DiscordConfiguration()
{
  Token = config.Token,
  TokenType = TokenType.Bot,
  Intents = DiscordIntents.AllUnprivileged |
    DiscordIntents.MessageContents |
    DiscordIntents.Guilds |
    DiscordIntents.GuildMessages
});

discord.UseInteractivity(new InteractivityConfiguration()
{
  PollBehaviour = PollBehaviour.KeepEmojis,
  Timeout = TimeSpan.FromSeconds(30)
});

CommandsNextExtension commands = discord.UseCommandsNext(new CommandsNextConfiguration()
{
  StringPrefixes = new[] { "•" }
});
commands.RegisterCommands<GibberishModule>();


SlashCommandsExtension slash = discord.UseSlashCommands();
slash.RegisterCommands<HelpCommands>();

discord.UseVoiceNext();

discord.MessageCreated += async (s, e) =>
{
  if (e.Author.IsBot) return;

  // Check word by word
  string message = e.Message.Content.Trim().ToLower();
  string[] msgContent = message.Split("\\s+");

  foreach (string msg in msgContent)
  {
    if (config.Lists.ItsJoever.Contains(msg))
      await e.Message.RespondAsync("https://i.kym-cdn.com/photos/images/newsfeed/002/360/758/f0b.jpg");
  };

  // Check full message
  if (config.Lists.RockAndStone.Any(rock => message.Contains(rock.ToLower())))
    await e.Message.RespondAsync(config.Lists.RockAndStone[rdn.Next(0, config.Lists.RockAndStone.Count)]);

  if (rdn.Next(0, 101) == 100)
    await e.Message.RespondAsync("Ptite bière !?");
};

await discord.ConnectAsync();
await Task.Delay(-1);