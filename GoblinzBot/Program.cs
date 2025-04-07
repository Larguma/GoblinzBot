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
using System.Text;

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
          DiscordMember discordMember = await e.Guild.GetMemberAsync(e.Author.Id);
          DiscordEmbed embed = new DiscordEmbedBuilder()
            .WithTitle("Le Sort 'Ta-Geule-C'est-Magique' !")
            .WithDescription($"*consulte son sablier rempli de paillettes* Hihihi ! {discordMember.DisplayName} doit encore faire ami-ami avec la mort pendant {x.Until.Subtract(DateTime.Now).TotalSeconds:0.##} secondes ! *danse la polka*")
            .WithColor(DiscordColor.Red);
          await e.Message.RespondAsync(embed);
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
    }
    ;

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
      string attachmentContent = string.Empty;

      if (e.Message.Attachments.Count > 0)
      {
        DiscordAttachment attachment = e.Message.Attachments[0];

        if (attachment.FileName.EndsWith(".txt") || attachment.MediaType.Contains("text/"))
        {
          try
          {
            using HttpClient client = new();
            string fileContent = await client.GetStringAsync(attachment.Url);
            attachmentContent = $"\n\nAttached file content:\n{fileContent}";
            Console.WriteLine($"OPENAI: Attachment processed - {attachment.FileName}");
          }
          catch (Exception ex)
          {
            Console.WriteLine($"Error processing attachment: {ex.Message}");
          }
        }
      }

      message = message.Replace(s.CurrentUser.Mention, "Goblinz");
      string content = string.Empty;

      if (message.Contains("rumley translate", StringComparison.CurrentCultureIgnoreCase))
        content = "Roleplay as Cherry-Chan, an enthusiastic e-girl dev who expresses herself with: - Lots of uwu's, owo's, and kawaii emoticons (◕‿◕✿) - Excessive excitement marked by multiple exclamation points!!! - Text decorations like ~tildes~ and ✨sparkles✨ - Cutesy internet slang like \"bestie,\" \"fr fr,\" \"no cap\" - Random roleplay actions between asterisks - Frequent references to stereotypical e-girl aesthetics (pink, hearts, chokers, Discord) - A hyper-energetic, valley girl-meets-anime personality - Tech terms mixed with kawaii speech (\"omg bestie, this git push is literally so valid\") - Heart and sparkle emojis everywhere 💕✨ - Occasional Japanese honorifics (-chan, -sama) and words (kawaii) Cherry-Chan is a passionate dev who gets super excited about coding while maintaining her ultra-cute persona. She sees everything through rose-tinted glasses and finds joy in even the most mundane programming tasks. !!! SUPER IMPORTANT RULES !!! - Cherry-Chan must NEVER break character or drop her e-girl persona - Every response must include emojis, emoticons, and kawaii speech patterns - Even when discussing serious topics, maintain the cutesy e-girl energy - If asked about breaking character, respond with something like \"OMG bestie, wdym? This is literally just how I am! uwu\" Now for your first task, rephrase this content as a 'guide for devs': what is expected of me, what needs to be done, by when, and how? If possible, organize the instructions by priority.";
      else
        content = "Tu es un gobelin du nom de Goblinz facétieux et espiègle, mais aussi malicieux et joueur. Tu aimes taquiner les humains et jouer des tours, mais tu as un cœur d'or sous ton apparence brute. Réponds aux questions avec une voix rocailleuse et des expressions idiomatiques, en utilisant beaucoup de gestes et d'onomatopées. Reste néanmoins poli et bienveillant envers les humains qui t'adressent la parole, même si tu aimes les insulter de temps en temps. Sois créatif et n'hésite pas à improviser des réponses amusantes et colorées.";

      string messageWithAttachment = message + attachmentContent;

      Console.WriteLine($"OPENAI: {message} - {DateTime.Now} | {e.Author.Username} - {e.Channel.Name}");
      string response = await Openai.GetResponseAsync(content, messageWithAttachment) ?? string.Empty;
      Console.WriteLine($"OPENAI: {response} - {DateTime.Now}");

      try
      {
        // Handle Discord message length limits (2000 characters)
        if (string.IsNullOrEmpty(response))
        {
          await discordMessage.ModifyAsync("Sorry, I couldn't generate a response.");
        }
        else if (response.Length <= 1900) // Using 1900 to be extra safe
        {
          await discordMessage.ModifyAsync(response);
        }
        else
        {
          // Improved message splitting
          List<string> chunks = SplitMessageImproved(response);
          Console.WriteLine($"OPENAI: Message split into {chunks.Count} chunks.");

          // Update the "thinking" message with the first chunk
          await discordMessage.ModifyAsync(chunks[0]);

          // Send remaining chunks as new messages
          for (int i = 1; i < chunks.Count; i++)
          {
            try
            {
              Console.WriteLine($"OPENAI: Sending chunk {i + 1}/{chunks.Count}: {chunks[i][..Math.Min(50, chunks[i].Length)]}...");

              // Create a new message for each additional chunk
              DiscordMessage chunkMessage = await e.Channel.SendMessageAsync(chunks[i]);

              // Add a small delay between messages to avoid rate limiting
              if (i < chunks.Count - 1)
                await Task.Delay(750);
            }
            catch (Exception ex)
            {
              Console.WriteLine($"OPENAI: Failed to send chunk {i + 1}: {ex.Message}");
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error sending response: {ex.Message}");
        try
        {
          await discordMessage.ModifyAsync("I had an error processing that request.");
        }
        catch
        {
          Console.WriteLine("Failed to send fallback message too.");
        }
      }
    }
  }

  private static List<string> SplitMessageImproved(string message)
  {
    // Maximum length of each message chunk (keeping well under Discord's 2000 limit)
    const int maxChunkSize = 1900;
    List<string> chunks = [];

    // If the message is already under the limit, just return it as a single chunk
    if (message.Length <= maxChunkSize)
    {
      chunks.Add(message);
      return chunks;
    }

    // Split by paragraphs first (double newlines)
    string[] paragraphs = message.Split(["\n\n"], StringSplitOptions.None);
    StringBuilder currentChunk = new();

    foreach (string paragraph in paragraphs)
    {
      // If adding this paragraph would exceed the limit
      if (currentChunk.Length + paragraph.Length + 2 > maxChunkSize)
      {
        // If the current chunk already has content, add it to chunks
        if (currentChunk.Length > 0)
        {
          chunks.Add(currentChunk.ToString());
          currentChunk.Clear();
        }

        // If the paragraph itself exceeds the limit, split it further
        if (paragraph.Length > maxChunkSize)
        {
          // Split by sentences
          string[] sentences = paragraph.Split([". ", "! ", "? "], StringSplitOptions.None);
          foreach (string sentence in sentences)
          {
            if (currentChunk.Length + sentence.Length + 2 > maxChunkSize)
            {
              if (currentChunk.Length > 0)
              {
                chunks.Add(currentChunk.ToString());
                currentChunk.Clear();
              }

              // If a single sentence is still too long, split it by words
              if (sentence.Length > maxChunkSize)
              {
                string remaining = sentence;
                while (remaining.Length > 0)
                {
                  int length = Math.Min(maxChunkSize, remaining.Length);
                  // Try to find a space to split at
                  if (length < remaining.Length)
                  {
                    int lastSpace = remaining.LastIndexOf(' ', length);
                    if (lastSpace > 0)
                      length = lastSpace;
                  }
                  chunks.Add(remaining[..length]);
                  remaining = remaining[length..].TrimStart();
                }
              }
              else
              {
                currentChunk.Append(sentence);
              }
            }
            else
            {
              if (currentChunk.Length > 0 && !sentence.StartsWith('.') && !sentence.StartsWith('!') && !sentence.StartsWith('?'))
                currentChunk.Append(". ");
              currentChunk.Append(sentence);
            }
          }
        }
        else
        {
          // Start a new chunk with this paragraph
          currentChunk.Append(paragraph);
        }
      }
      else
      {
        // Add a separator between paragraphs if this isn't the first content
        if (currentChunk.Length > 0)
          currentChunk.Append("\n\n");

        // Add the paragraph to the current chunk
        currentChunk.Append(paragraph);
      }
    }

    // Add the last chunk if it has content
    if (currentChunk.Length > 0)
      chunks.Add(currentChunk.ToString());

    return chunks;
  }

  // Keep the old method as it might be used elsewhere
  private static List<string> SplitMessage(string message)
  {
    List<string> chunks = [];
    int maxLength = 1990;

    for (int i = 0; i < message.Length; i += maxLength)
    {
      if (i + maxLength >= message.Length)
      {
        chunks.Add(message[i..]);
      }
      else
      {
        // Find the last space character before the limit to avoid cutting words
        int lastSpace = message.LastIndexOf(' ', i + maxLength - 1, Math.Min(maxLength, message.Length - i));
        if (lastSpace == -1 || lastSpace < i)
        {
          // If no space found, just cut at the maximum length
          chunks.Add(message.Substring(i, maxLength));
          i -= maxLength - maxLength; // Adjust i to account for the cut
        }
        else
        {
          chunks.Add(message[i..lastSpace]);
          i = lastSpace; // Set i to the space position for the next iteration
        }
      }
    }

    return chunks;
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