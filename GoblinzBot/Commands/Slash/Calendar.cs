using System.Globalization;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;

public class CalendarCommands : ApplicationCommandModule
{
  private static readonly ItemsController _itemsController = Program.Services!.GetService<ItemsController>() ?? throw new ArgumentNullException(nameof(ItemsController));
  public enum CourseList
  {
    [ChoiceName("Admin")] Admin,
    [ChoiceName("Ado")] Ado,
    [ChoiceName("Ado (A)")] Ado_A,
    [ChoiceName("Ado (B)")] Ado_B,
    [ChoiceName("Algo")] Algo,
    [ChoiceName("Concurp")] Concurp,
    [ChoiceName("Concurp (A)")] Concurp_A,
    [ChoiceName("Concurp (B)")] Concurp_B,
    [ChoiceName("DevOps")] DevOps,
    [ChoiceName("Maths")] Maths,
    [ChoiceName("Mobile")] Mobile,
    [ChoiceName("Physique")] Physique,
    [ChoiceName("Projet")] Projet,
    [ChoiceName("Stats")] Stats,
    [ChoiceName("SysInfo")] SysInfo,
  }

  [SlashCommand("add", "Add a task")]
  public async void Add(InteractionContext ctx,
    [Option("course", "The course")] CourseList course,
    [Option("name", "The name of the task")] string name,
    [Option("date", "The date of the task (yyyy-MM-dd)")] string date,
    [Option("isExam", "Is it an exam?")] bool isExam = false)
  {
    if (ctx.Guild == null)
    {
      await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        new DiscordInteractionResponseBuilder().WithContent("This command can only be used in a server!"));
      return;
    }

    await ctx.DeferAsync();

    if (!DateTime.TryParse(date, out DateTime _))
    {
      await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Invalid date!"));
      return;
    }

    await _itemsController.Create(new Item()
    {
      Lesson = course.ToString(),
      Title = name,
      End = DateTime.Parse(date),
      IsExam = isExam,
      GuildId = ctx.Guild.Id.ToString()
    });

    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Task added! {course} - {name} ({date})"));
  }

  [SlashCommand("list", "List all tasks")]
  public async void List(InteractionContext ctx)
  {
    if (ctx.Guild == null)
    {
      await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        new DiscordInteractionResponseBuilder().WithContent("This command can only be used in a server!"));
      return;
    }

    await ctx.DeferAsync();

    StringBuilder sb = await GetFormatedListAsync(ctx.Guild.Id.ToString());

    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
    .AddComponents(Program.GetListButtonComponent())
    .WithContent(sb.ToString()));
  }

  [SlashCommand("del", "Delete a task")]
  public async void Delete(InteractionContext ctx)
  {
    if (ctx.Guild == null)
    {
      await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        new DiscordInteractionResponseBuilder().WithContent("This command can only be used in a server!"));
      return;
    }

    List<Item> items = await _itemsController.Index();
    if (items.Count == 0)
    {
      await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        new DiscordInteractionResponseBuilder().WithContent("No tasks!"));
      return;
    }

    await ctx.DeferAsync();
    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
      .AddComponents(await GetDropdownListAsync(ctx.Guild.Id.ToString(), "Select a task to delete (no confirmation)", "delete")));
  }

  internal static async Task<DiscordSelectComponent> GetDropdownListAsync(string guildId, string placeholder, string id)
  {
    List<Item> items = await _itemsController.Index();
    List<DiscordSelectComponentOption> options = [];

    if (items.Count == 0)
    {
      options.Add(new("I said there is no tasks", "no_tasks"));
      return new DiscordSelectComponent($"dropdown_tasks_{id}", "No tasks", options);
    }

    items.Sort((x, y) => x.End.CompareTo(y.End));

    items.ForEach(x =>
    {
      string end = x.End.ToString("dddd dd/MM", CultureInfo.CreateSpecificCulture("fr-CH"));
      if (x.GuildId == guildId)
      {
        options.Add(new($"{x.Lesson}: {x.Title} ({end})", $"task_{x.Id}"));
      }
    });

    // Make the dropdown
    return new DiscordSelectComponent($"dropdown_tasks_{id}", placeholder, options);
  }

  internal static async Task<StringBuilder> GetFormatedListAsync(string guildId)
  {
    // List cleanup
    List<Item> items = await _itemsController.Index();

    StringBuilder sb = new();

    if (items.Count == 0)
      return sb.AppendLine("```ansi\nNo tasks!\n```");

    items.Sort((x, y) => x.End.CompareTo(y.End));

    sb.AppendLine("```ansi");
    sb.AppendLine("\u001b[1;37mTasks:\u001b[0;0m");
    items.ForEach(x =>
    {
      string end = x.End.ToString("dddd dd/MM", CultureInfo.CreateSpecificCulture("fr-CH"));
      if (x.GuildId == guildId)
      {
        sb.Append("- ");
        if (x.IsExam)
          sb.Append($"\u001b[0;41mEXAM ");

        if (x.End < DateTime.Now)
          sb.Append($"\u001b[0;30m");

        sb.AppendLine($"{x.Lesson} - {x.Title} ({end})\u001b[0;0m");
      }
    });
    sb.AppendLine("```");
    return sb;

    // \u001b[1;0mBOLD\u001b[0;0m
    // \u001b[0;30mGray\u001b[0;0m
    // \u001b[0;31mRed\u001b[0;0m
    // \u001b[0;32mGreen\u001b[0;0m
    // \u001b[0;33mYellow\u001b[0;0m
    // \u001b[0;34mBlue\u001b[0;0m
    // \u001b[0;35mPink\u001b[0;0m
    // \u001b[0;36mCyan\u001b[0;0m
    // \u001b[0;37mWhite\u001b[0;0m
    // \u001b[0;40mFirefly dark blue background\u001b[0;0m
    // \u001b[0;41mOrange background\u001b[0;0m
    // \u001b[0;42mMarble blue background\u001b[0;0m
    // \u001b[0;43mGreyish turquoise background\u001b[0;0m
    // \u001b[0;44mGray background\u001b[0;0m
    // \u001b[0;45mIndigo background\u001b[0;0m
    // \u001b[0;46mLight gray background\u001b[0;0m
    // \u001b[0;47mWhite background\u001b[0;0m
  }

  internal static async Task DeleteObsolete()
  {
    // List cleanup
    List<Item> items = await _itemsController.Index();

    if (items.Count == 0)
      return;

    items.ForEach(async x =>
    {
      if (x.End < DateTime.Now.AddDays(-3))
      {
        await _itemsController.Delete(x.Id);
      }
    });
  }

  internal static async Task Delete(ObjectId id)
  {
    await _itemsController.Delete(id);
  }

  internal static async Task<Item> GetTaskById(string v)
  {
    return await _itemsController.Details(ObjectId.Parse(v)) ?? throw new Exception("Task not found");
  }

  internal static async Task UpdateTask(Item updatedItem)
  {
    await _itemsController.Update(updatedItem);
  }
}