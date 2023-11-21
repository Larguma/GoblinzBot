using System.Text.RegularExpressions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HtmlAgilityPack;

public class PrefixCommandsModule : BaseCommandModule
{
  private Random rdn = new();
  private HttpClient http = new();

  [Command("linker")]
  [Description("Print a linker error")]
  public async Task LinkerError(CommandContext ctx)
  {
    await DeleteMessageAsync(ctx);
    await ctx.RespondAsync("https://i.ibb.co/BrVR74f/Whats-App-Image-2023-11-09-at-14-30-48-1.jpg");
  }

  [Command("toast")]
  [Description("*Clink* a toast")]
  public async Task ClinkToast(CommandContext ctx)
  {
    await DeleteMessageAsync(ctx);
    await ctx.RespondAsync("https://i.pinimg.com/736x/3c/b5/5c/3cb55cd11d436eb4b5bdb8e8cfc3ac1a.jpg");
  }

  [Command("rat")]
  [Description("RRRRRRRRRRRRAT")]
  public async Task RRRAT(CommandContext ctx)
  {
    await DeleteMessageAsync(ctx);
    await ctx.RespondAsync("https://pbs.twimg.com/media/FPgA0D7XEAAtzr4.jpg");
  }

  [Command("spin")]
  [Description("Horizontally spinning rat")]
  public async Task RatSpin(CommandContext ctx)
  {
    await DeleteMessageAsync(ctx);
    await ctx.RespondAsync("https://media.tenor.com/RfJzepsDdmYAAAAC/rat-spinning.gif");
  }

  [Command("russian")]
  [Description("Chance to kill yourself")]
  public async Task Russian(CommandContext ctx)
  {
    if (ctx.Guild == null)
    {
      await ctx.RespondAsync("This command can only be used in a server!");
      return;
    }

    await DeleteMessageAsync(ctx);

    List<string> russianDead = Program.DiscordSettings.Lists.RussianDead;
    List<string> russianAlive = Program.DiscordSettings.Lists.RussianAlive;
    DiscordColor color = DiscordColor.Green;

    int draw = rdn.Next(1, 7);
    string quote;
    if (draw == 6)
    {
      quote = russianDead[rdn.Next(0, russianDead.Count)];
      color = DiscordColor.Red;
    }
    else
      quote = russianAlive[rdn.Next(0, russianAlive.Count)];

    DiscordEmbedBuilder embed = new()
    {
      Color = color,
      Description = quote,
      Author = new()
      {
        Name = ctx.Member.DisplayName,
        IconUrl = ctx.Member.AvatarUrl
      }
    };

    await ctx.RespondAsync(embed);
  }

  [Command("inspirobot")]
  [Description("Get an image from inspirobot")]
  public async Task Inspirobot(CommandContext ctx)
  {
    await DeleteMessageAsync(ctx);
    string url = http.GetAsync("http://inspirobot.me/api?generate=true").Result.Content.ReadAsStringAsync().Result;
    await ctx.RespondAsync(url);
  }

  [Command("weather")]
  [Description("Get the weather for Fribourg")]
  public async Task GetWeather(CommandContext ctx)
  {
    await DeleteMessageAsync(ctx);
    string? html = http.GetAsync("http://wttr.in/Fribourg?0").Result.Content.ReadAsStringAsync().Result;

    HtmlDocument doc = new ();
    doc.LoadHtml(html);

    HtmlNode node = doc.DocumentNode.SelectSingleNode("//pre");

    string value = node.InnerText;
    value = Regex.Replace(value, @"(\r\n|\r|\n)+", "\n");
    value = value.Replace("&quot;", "\"");
    value = "```" + value + "```";

    DiscordEmbedBuilder embed = new()
    {
      Color = DiscordColor.Azure,
      Description = value
    };

    await ctx.RespondAsync(embed);
  }

  [Command("time")]
  [Description("Print the IL timetable")]
  public async Task ILTimetable(CommandContext ctx) {
    await ctx.Message.DeleteAsync();
    await ctx.RespondAsync("https://i.ibb.co/Ws0ZxFy/image.png");
  }

  private async Task DeleteMessageAsync(CommandContext ctx)
  {
    if (ctx.Message.Author.IsBot) return;
    if (ctx.Channel.IsPrivate) return;
    await ctx.Message.DeleteAsync();
  }
}