using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using DSharpPlus.ModalCommands;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;
using System.Globalization;

internal class Program
{
  //Main Discord Properties
  public static DiscordClient Client { get; set; } = null!;
  private static CommandsNextExtension Commands { get; set; } = null!;

  public static IServiceProvider? Services { get; set; } = null!;
  public static DiscordSettings DiscordSettings { get; set; } = null!;
  public static List<BannedUser> BannedUsers { get; set; } = [];
  private static OpenaiController Openai { get; set; } = null!;
  private static readonly Random Random = new();

  private static async Task Main(string[] args)
  {
    // Read config file
    IConfigurationBuilder builder = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: false);

    IConfiguration config = builder.Build();
    DiscordSettings = config.GetSection(nameof(DiscordSettings)).Get<DiscordSettings>() ?? throw new ArgumentNullException(nameof(DiscordSettings));

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
    string? token;
    if (Environment.MachineName == "warp")
      token = DiscordSettings?.TokenDev;
    else
      token = DiscordSettings?.Token;

    DiscordClient Client = new(new DiscordConfiguration()
    {
      Token = token,
      TokenType = TokenType.Bot,
      Intents = DiscordIntents.All,
      LoggerFactory = logFactory
    });

    Client.UseInteractivity(new InteractivityConfiguration()
    {
      PollBehaviour = PollBehaviour.KeepEmojis,
      Timeout = TimeSpan.FromSeconds(30)
    });

    CommandsNextExtension Commands = Client.UseCommandsNext(new CommandsNextConfiguration()
    {
      StringPrefixes = ["•", "!"]
    });
    SlashCommandsExtension slashCommandsConfig = Client.UseSlashCommands();

    //Prefix Based Commands
    Commands.RegisterCommands<PrefixCommandsModule>();

    //Slash Commands
    slashCommandsConfig.RegisterCommands<HelpCommands>();
    slashCommandsConfig.RegisterCommands<CalendarCommands>();
    slashCommandsConfig.RegisterCommands<FunCommands>();

    //ERROR EVENT HANDLERS
    Commands.CommandErrored += OnCommandError;

    //EVENT HANDLERS
    Client.Ready += OnClientReady;
    Client.ComponentInteractionCreated += InteractionEventHandler;
    Client.MessageCreated += MessageSendHandler;
    Client.ModalSubmitted += ModalEventHandler;
    Client.GuildMemberAdded += UserJoinHandler;

    Openai = new(DiscordSettings!.OpenaiToken, Services);

    await Client.ConnectAsync();
    await Task.Delay(-1);
  }

  private static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
  {
    return Task.CompletedTask;
  }

  private static async Task UserJoinHandler(DiscordClient s, GuildMemberAddEventArgs e)
  {
    DiscordChannel defaultChannel = e.Guild.GetDefaultChannel();

    DiscordEmbedBuilder welcomeEmbed = new()
    {
      Color = DiscordColor.Gold,
      Title = $"Welcome {e.Member.Username} to the server",
      Description = "Bli bla blo"
    };

    await defaultChannel.SendMessageAsync(embed: welcomeEmbed);
  }

  private static async Task ModalEventHandler(DiscordClient s, ModalSubmitEventArgs e)
  {
    await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    string guildId = e.Interaction.GuildId.ToString() ?? string.Empty;
    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
      .AddComponents(GetListButtonComponent())
      .WithContent(CalendarCommands.GetFormatedListAsync(guildId).Result.ToString()));
  }

  private static async Task MessageSendHandler(DiscordClient s, MessageCreateEventArgs e)
  {
    if (e.Author.IsBot) return;

    // Check if user is banned
    if (BannedUsers.Count > 0)
      BannedUsers.ForEach(async x =>
      {
        if (x.Until < DateTime.Now)
          BannedUsers.Remove(x);
        if (x.UserId == e.Author.Id)
        {
          await e.Message.DeleteAsync();
          DiscordEmbed embed = new DiscordEmbedBuilder()
            .WithTitle("You are dead to me")
            .WithDescription($"You are still dead for {x.Until.Subtract(DateTime.Now).TotalMinutes:0.##} minutes")
            .WithColor(DiscordColor.Red);
          DiscordMember discordMember = await e.Guild.GetMemberAsync(e.Author.Id);
          await discordMember.SendMessageAsync(embed);
          return;
        }
      });

    // Check word by word
    string message = e.Message.Content.Trim().ToLower();
    string[] msgContent = message.Split(" ");

    foreach (string msg in msgContent)
    {
      if (DiscordSettings.Lists.ItsJoever.Contains(msg) && Random.Next(0, 101) >= 75)
        await e.Message.RespondAsync("https://i.kym-cdn.com/photos/images/newsfeed/002/360/758/f0b.jpg");

      if (msg.Equals("java", StringComparison.CurrentCultureIgnoreCase))
        await e.Message.RespondAsync(DiscordSettings.Lists.JavaWord[Random.Next(DiscordSettings.Lists.JavaWord.Count)]);
    };

    // Check full message
    if (DiscordSettings.Lists.RockAndStone.Any(rock => message.Contains(rock, StringComparison.CurrentCultureIgnoreCase)))
      await e.Message.RespondAsync(DiscordSettings.Lists.RockAndStone[Random.Next(DiscordSettings.Lists.RockAndStone.Count)]);

    if (Random.Next(0, 101) == 100)
      await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":beers:"));
    else if (Random.Next(0, 101) == 100)
      await e.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":cool:"));

    // Good/Bad bot
    if (message.Equals("good bot", StringComparison.CurrentCultureIgnoreCase))
      await e.Message.RespondAsync(DiscordSettings.Lists.GoodBot[Random.Next(DiscordSettings.Lists.GoodBot.Count)]);
    if (message.Equals("bad bot", StringComparison.CurrentCultureIgnoreCase))
      await e.Message.RespondAsync(DiscordSettings.Lists.BadBot[Random.Next(DiscordSettings.Lists.BadBot.Count)]);

    // Suicide
    if (message.Contains("suicide", StringComparison.CurrentCultureIgnoreCase) || message.Contains("unalive", StringComparison.CurrentCultureIgnoreCase))
      await e.Message.RespondAsync("027 321 21 21");

    // APÉROOOOOO
    if (message.Contains("apero", StringComparison.CurrentCultureIgnoreCase) || message.Contains("apéro", StringComparison.CurrentCultureIgnoreCase))
      await e.Message.RespondAsync("APÉROOOO!!");

    // Mention with openai
    if (message.Contains(s.CurrentUser.Mention))
    {
      DiscordMessage discordMessage = await e.Message.RespondAsync("Goblinz is thinking...");
      message = message.Replace(s.CurrentUser.Mention, "Goblinz");
      Console.WriteLine($"OPENAI: {message} - {DateTime.Now} | {e.Author.Username} - {e.Channel.Name}");
      string response = await Openai.GetResponseAsync(message) ?? string.Empty;
      Console.WriteLine($"OPENAI: {response} - {DateTime.Now}");
      await discordMessage.ModifyAsync(response);
    }
  }

  private static async Task InteractionEventHandler(DiscordClient s, ComponentInteractionCreateEventArgs e)
  {
    if (e.Id == "btn_delete_obsolete")
    {
      await CalendarCommands.DeleteObsolete();
    }

    if (e.Id == "btn_edit_task")
    {
      await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
      .AddComponents(GetListButtonComponent())
      .AddComponents(CalendarCommands.GetDropdownListAsync(e.Guild.Id.ToString(), "Select a task to edit", "edit").Result));
    }

    if (e.Id == "btn_delete_task")
    {
      await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
      .AddComponents(GetListButtonComponent())
      .AddComponents(CalendarCommands.GetDropdownListAsync(e.Guild.Id.ToString(), "Select a task to delete (no confirmation)", "delete").Result));
    }

    if (e.Id == "btn_delete_obsolete" ||
        e.Id == "btn_refresh_list")
    {
      await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
        .AddComponents(GetListButtonComponent())
        .WithContent(CalendarCommands.GetFormatedListAsync(e.Guild.Id.ToString()).Result.ToString()));
    }

    if (e.Id == "btn_refresh_list_show_all")
    {
      await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
        .AddComponents(GetListButtonComponent())
        .WithContent(CalendarCommands.GetFormatedListAsync(e.Guild.Id.ToString(), true).Result.ToString()));
    }

    if (e.Id == "dropdown_tasks_delete")
    {
      await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

      if (e.Values[0] != "no_tasks")
        await CalendarCommands.Delete(ObjectId.Parse(e.Values[0].Replace("task_", "")));

      await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
        .AddComponents(GetListButtonComponent())
        .WithContent(CalendarCommands.GetFormatedListAsync(e.Guild.Id.ToString()).Result.ToString()));
    }

    if (e.Id == "dropdown_tasks_edit")
    {
      if (e.Values[0] == "no_tasks")
      {
        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
          new DiscordInteractionResponseBuilder().WithContent("There is no tasks for the love of gods!"));
        return;
      }

      Item item = CalendarCommands.GetTaskById(e.Values[0].Replace("task_", "")).Result;
      ObjectId id = item.Id;
      string end = item.End.ToString("yyyy-MM-dd", CultureInfo.CreateSpecificCulture("fr-CH"));

      DiscordInteractionResponseBuilder modal = ModalBuilder.Create("modal_edit_task")
        .WithTitle("Edit a task")
        .AddComponents(new TextInputComponent("Course", "lesson", item.Lesson, item.Lesson))
        .AddComponents(new TextInputComponent("Name", "title", item.Title, item.Title))
        .AddComponents(new TextInputComponent("Date", "end", "yyyy-MM-dd", end))
        .AddComponents(new TextInputComponent("Is Exam", "isExam", item.IsExam.ToString(), item.IsExam.ToString()))
        .AddComponents(new TextInputComponent("Backgroung color", "colorOption", "Firefly (0), Orange (1), Marble (2) or Indigo (3)", item.Color.ToString()));
      await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);

      InteractivityResult<ModalSubmitEventArgs> response = await s.GetInteractivity().WaitForModalAsync(">modal_edit_task");
      item.Lesson = response.Result.Values["lesson"];
      item.Title = response.Result.Values["title"];
      item.End = DateTime.Parse(response.Result.Values["end"]);
      item.IsExam = bool.Parse(response.Result.Values["isExam"]);
      item.Color = int.Parse(response.Result.Values["colorOption"]);

      await CalendarCommands.UpdateTask(item);
    }

    if (e.Id == "btn_surrender_yes")
    {
      await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
        new DiscordInteractionResponseBuilder()
        .AddComponents(GetSurrenderButtonComponent())
        .WithContent(FunCommands.GetFormatedSurrender("yes", oldMessage: e.Message.Content, username: e.User.Mention).ToString()));
    }

    if (e.Id == "btn_surrender_no")
    {
      await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
        new DiscordInteractionResponseBuilder()
        .AddComponents(GetSurrenderButtonComponent())
        .WithContent(FunCommands.GetFormatedSurrender("no", oldMessage: e.Message.Content, username: e.User.Mention).ToString()));
    }
  }

  private static async Task OnCommandError(CommandsNextExtension s, CommandErrorEventArgs e)
  {
    //Casting my ErrorEventArgs as a ChecksFailedException
    if (e.Exception is ChecksFailedException castedException)
    {
      string cooldownTimer = string.Empty;

      foreach (CheckBaseAttribute check in castedException.FailedChecks)
      {
        CooldownAttribute cooldown = (CooldownAttribute)check; //The cooldown that has triggered this method
        TimeSpan timeLeft = cooldown.GetRemainingCooldown(e.Context); //Getting the remaining time on this cooldown
        cooldownTimer = timeLeft.ToString(@"hh\:mm\:ss");
      }

      DiscordEmbedBuilder cooldownMessage = new()
      {
        Title = "Wait for the Cooldown to End",
        Description = "Remaining Time: " + cooldownTimer,
        Color = DiscordColor.Red
      };

      await e.Context.Channel.SendMessageAsync(embed: cooldownMessage);
    }
  }

  internal static DiscordButtonComponent[] GetListButtonComponent()
  {
    return
    [
      new (ButtonStyle.Primary, "btn_refresh_list", "Refresh list", false, new DiscordComponentEmoji("🔄")),
      new (ButtonStyle.Primary, "btn_refresh_list_show_all", "Refresh list (show all)", false, new DiscordComponentEmoji("🔄")),
      new (ButtonStyle.Secondary, "btn_edit_task", "Edit a task", false, new DiscordComponentEmoji("📝")),
      new (ButtonStyle.Danger, "btn_delete_obsolete", "Delete old tasks", false, new DiscordComponentEmoji("🗑️")),
      new (ButtonStyle.Danger, "btn_delete_task", "Delete a task", false, new DiscordComponentEmoji("✖️")),
    ];
  }

  internal static DiscordButtonComponent[] GetSurrenderButtonComponent()
  {
    return
    [
      new (ButtonStyle.Success, "btn_surrender_yes", "Yes"),
      new (ButtonStyle.Danger, "btn_surrender_no", "No")
    ];
  }
}