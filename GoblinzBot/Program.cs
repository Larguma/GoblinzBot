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
using Microsoft.Extensions.Logging;
using Serilog;
using DSharpPlus.ModalCommands;
using MongoDB.Driver;
using System.Globalization;

internal class Program
{
  public static IServiceProvider? Services { get; set; }
  public static DiscordSettings? DiscordSettings { get; set; }

  private static async Task Main(string[] args)
  {
    // Read config file
    IConfigurationBuilder builder = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: false);

    IConfiguration config = builder.Build();
    DiscordSettings = config.GetSection(nameof(DiscordSettings)).Get<DiscordSettings>();

    Random rdn = new();

    Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

    ILoggerFactory logFactory = new LoggerFactory().AddSerilog();

    IHost host = CreateHostBuilder(args).Build();
    IServiceScope scope = host.Services.CreateScope();

    Services = scope.ServiceProvider;

    IHostBuilder CreateHostBuilder(string[] strings)
    {
      return Host.CreateDefaultBuilder()
          .ConfigureServices((_, Services) =>
          {
            Services.Configure<DatabaseSettings>(config.GetSection(nameof(DatabaseSettings)));
            Services.AddSingleton<ItemsService>();
            Services.AddSingleton<ItemsController>();
            Services.AddSingleton<OpenaiService>();
          });
    }

    // Get if prod or dev
    string token;
    if (Environment.MachineName == "warp")
      token = DiscordSettings.TokenDev;
    else
      token = DiscordSettings.Token;

    DiscordClient discord = new(new DiscordConfiguration()
    {
      Token = token,
      TokenType = TokenType.Bot,
      Intents = DiscordIntents.All,
      LoggerFactory = logFactory
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
    commands.RegisterCommands<PrefixCommandsModule>();

    SlashCommandsExtension slash = discord.UseSlashCommands();
    slash.RegisterCommands<HelpCommands>();
    slash.RegisterCommands<CalendarCommands>();

    discord.UseVoiceNext();

    OpenaiController openai = new(DiscordSettings.OpenaiToken);

    // On new message
    discord.MessageCreated += async (s, e) =>
    {
      if (e.Author.IsBot) return;

      // Check word by word
      string message = e.Message.Content.Trim().ToLower();
      string[] msgContent = message.Split(" ");

      foreach (string msg in msgContent)
      {
        if (DiscordSettings.Lists.ItsJoever.Contains(msg) && rdn.Next(0, 101) >= 75)
          await e.Message.RespondAsync("https://i.kym-cdn.com/photos/images/newsfeed/002/360/758/f0b.jpg");
      };

      // Check full message
      if (DiscordSettings.Lists.RockAndStone.Any(rock => message.Contains(rock.ToLower())))
        await e.Message.RespondAsync(DiscordSettings.Lists.RockAndStone[rdn.Next(0, DiscordSettings.Lists.RockAndStone.Count)]);

      if (rdn.Next(0, 101) == 100)
        await e.Message.RespondAsync("Y t'faut une 'tite bière");

      // Good/Bad bot
      if (message.ToLower() == "good bot")
        await e.Message.RespondAsync(DiscordSettings.Lists.GoodBot[rdn.Next(0, DiscordSettings.Lists.GoodBot.Count)]);
      if (message.ToLower() == "bad bot")
        await e.Message.RespondAsync(DiscordSettings.Lists.BadBot[rdn.Next(0, DiscordSettings.Lists.BadBot.Count)]);

      // Mention with openai
      if (message.Contains(s.CurrentUser.Mention))
      {
        DiscordMessage discordMessage = await e.Message.RespondAsync("Goblinz is thinking...");
        Console.WriteLine($"OPENAI: {message} - {DateTime.Now} | {e.Author.Username} - {e.Channel.Name}");
        message = message.Replace(s.CurrentUser.Mention, "");
        string response = await openai.GetResponseAsync(message);
        Console.WriteLine($"OPENAI: {response} - {DateTime.Now}");
        await discordMessage.ModifyAsync(response);
      }
    };

    // On button press
    discord.ComponentInteractionCreated += async (s, e) =>
    {
      if (e.Id == "btn_delete_obsolete")
      {
        await CalendarCommands.DeleteObsolete();
      }

      if (e.Id == "btn_edit_task")
      {
        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
        .AddComponents(await CalendarCommands.GetDropdownListAsync(e.Guild.Id.ToString(), "Select a task to edit", "edit")));
      }

      if (e.Id == "btn_delete_obsolete" ||
          e.Id == "btn_refresh_list")
      {
        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
          .AddComponents(GetListButtonComponent())
          .WithContent(CalendarCommands.GetFormatedListAsync(e.Guild.Id.ToString()).Result.ToString()));
      }

      if (e.Id == "dropdown_tasks_delete")
      {
        if (e.Values[0] != "no_tasks")
          await CalendarCommands.Delete(ObjectId.Parse(e.Values[0].Replace("task_", "")));

        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
          .AddComponents(GetListButtonComponent())
          .WithContent(CalendarCommands.GetFormatedListAsync(e.Guild.Id.ToString()).Result.ToString()));
      }

      if (e.Id == "dropdown_tasks_edit")
      {
        Item item = CalendarCommands.GetTaskById(e.Values[0].Replace("task_", "")).Result;
        ObjectId id = item.Id;
        string end = item.End.ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("fr-CH"));

        DiscordInteractionResponseBuilder modal = ModalBuilder.Create("modal_edit_task")
          .WithTitle("Edit a task")
          .AddComponents(new TextInputComponent("Course", "lesson", item.Lesson, item.Lesson))
          .AddComponents(new TextInputComponent("Name", "title", item.Title, item.Title))
          .AddComponents(new TextInputComponent("Date", "end", "yyyy-MM-dd", end))
          .AddComponents(new TextInputComponent("Is Exam", "isExam", item.IsExam.ToString(), item.IsExam.ToString()));
        await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);

        var response = await s.GetInteractivity().WaitForModalAsync(">modal_edit_task");
        item.Lesson = response.Result.Values["lesson"];
        item.Title = response.Result.Values["title"];
        item.End = DateTime.Parse(response.Result.Values["end"]);
        item.IsExam = bool.Parse(response.Result.Values["isExam"]);

        CalendarCommands.UpdateTask(item);
      }
    };

    // On modal submit
    discord.ModalSubmitted += async (s, e) =>
    {
      await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
      await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
        .AddComponents(GetListButtonComponent())
        .WithContent(CalendarCommands.GetFormatedListAsync(e.Interaction.GuildId.ToString()).Result.ToString()));
    };

    await discord.ConnectAsync();
    await Task.Delay(-1);
  }

  internal static DiscordButtonComponent[] GetListButtonComponent()
  {
    return new DiscordButtonComponent[]
    {
      new (ButtonStyle.Primary, "btn_refresh_list", "Refresh list", false, new DiscordComponentEmoji("🔄")),
      new (ButtonStyle.Secondary, "btn_edit_task", "Edit a task", false, new DiscordComponentEmoji("📝")),
      new (ButtonStyle.Danger, "btn_delete_obsolete", "Delete old tasks", false, new DiscordComponentEmoji("🗑️"))
    };
  }
}