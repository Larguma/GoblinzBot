using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;


internal class Program
{
  public static IServiceProvider? Services { get; set; }

  private static async Task Main(string[] args)
  {
    // Read config file
    IConfigurationBuilder builder = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: false);

    IConfiguration config = builder.Build();
    DiscordSettings discordSettings = config.GetSection(nameof(DiscordSettings)).Get<DiscordSettings>();

    Random rdn = new();

    using IHost host = CreateHostBuilder(args).Build();
    using IServiceScope scope = host.Services.CreateScope();

    Services = scope.ServiceProvider;

    IHostBuilder CreateHostBuilder(string[] strings)
    {
      return Host.CreateDefaultBuilder()
          .ConfigureServices((_, Services) =>
          {
            Services.Configure<DatabaseSettings>(config.GetSection(nameof(DatabaseSettings)));
            Services.AddSingleton<ItemsService>();
            Services.AddSingleton<ItemsController>();
          });
    }

    DiscordClient discord = new(new DiscordConfiguration()
    {
      Token = discordSettings.TokenDev,
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
      StringPrefixes = new[] { "•", "!" }
    });
    commands.RegisterCommands<GibberishModule>();
    commands.RegisterCommands<UsefulModule>();


    SlashCommandsExtension slash = discord.UseSlashCommands();
    slash.RegisterCommands<HelpCommands>();
    slash.RegisterCommands<CalendarCommands>();

    discord.UseVoiceNext();

    // On new message
    discord.MessageCreated += async (s, e) =>
    {
      if (e.Author.IsBot) return;

      // Check word by word
      string message = e.Message.Content.Trim().ToLower();
      string[] msgContent = message.Split("\\s+");

      foreach (string msg in msgContent)
      {
        if (discordSettings.Lists.ItsJoever.Contains(msg))
          await e.Message.RespondAsync("https://i.kym-cdn.com/photos/images/newsfeed/002/360/758/f0b.jpg");
      };

      // Check full message
      if (discordSettings.Lists.RockAndStone.Any(rock => message.Contains(rock.ToLower())))
        await e.Message.RespondAsync(discordSettings.Lists.RockAndStone[rdn.Next(0, discordSettings.Lists.RockAndStone.Count)]);

      if (rdn.Next(0, 101) == 100)
        await e.Message.RespondAsync("Y t'faut une 'tite bière");
    };

    // On button press
    discord.ComponentInteractionCreated += async (s, e) =>
    {
      await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

      if (e.Id == "btn_delete_obsolete")
      {
        await CalendarCommands.DeleteObsolete();
      }

      if (e.Id == "btn_delete_obsolete" ||
          e.Id == "btn_refresh_list")
      {
        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
          .AddComponents(new DiscordButtonComponent[]
          {
            new (ButtonStyle.Primary, "btn_refresh_list", "Refresh list", false, new DiscordComponentEmoji("📝")),
            new (ButtonStyle.Danger, "btn_delete_obsolete", "Delete old tasks", false, new DiscordComponentEmoji("🗑️"))
          }).WithContent(CalendarCommands.GetFormatedListAsync(e.Guild.Id.ToString()).Result.ToString()));
      }

      if (e.Id == "dropdown_tasks") 
      {
        await CalendarCommands.Delete(ObjectId.Parse(e.Values[0].Replace("task_", "")));

        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
          .AddComponents(new DiscordButtonComponent[]
          {
            new (ButtonStyle.Primary, "btn_refresh_list", "Refresh list", false, new DiscordComponentEmoji("📝")),
            new (ButtonStyle.Danger, "btn_delete_obsolete", "Delete old tasks", false, new DiscordComponentEmoji("🗑️"))
          }).WithContent(CalendarCommands.GetFormatedListAsync(e.Guild.Id.ToString()).Result.ToString()));
      
      }
    };


    await discord.ConnectAsync();
    await Task.Delay(-1);
  }
}