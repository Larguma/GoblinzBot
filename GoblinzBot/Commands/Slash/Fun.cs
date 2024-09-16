using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Serilog;

public class FunCommands : ApplicationCommandModule
{
  private readonly Random rdn = new();

  [SlashCommand("roast", "Insult someone")]
  public async void Roast(InteractionContext ctx,
    [Option("user", "The user you want to insult")] DiscordUser user)
  {
    List<string>? insults = Program.DiscordSettings?.Lists?.Insult;
    if (insults is null || insults.Count == 0)
    {
      await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        new DiscordInteractionResponseBuilder().WithContent("The insult list is empty!"));
      return;
    }

    string insult = insults[rdn.Next(insults.Count)];

    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
      new DiscordInteractionResponseBuilder().WithContent($"{user.Mention}{insult}").AddMentions([new UserMention(user)]));
  }

  [SlashCommand("ff", "Surrender")]
  public static async void Surrender(InteractionContext ctx,
    [Option("title", "The title")] string title = "Surrender https://cdn.discordapp.com/attachments/1158728447510708234/1285162697926901781/exit_form_heia.pdf")
  {
    await ctx.DeferAsync();

    StringBuilder sb = GetFormatedSurrender("init", title: title);

    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
      .AddComponents(Program.GetSurrenderButtonComponent())
      .WithContent(sb.ToString()));
  }

  internal static StringBuilder GetFormatedSurrender(string choice, string oldMessage = "", string title = "", string username = "")
  {
    StringBuilder sb = new();

    sb.AppendLine(oldMessage);

    Log.Logger.Information($"oldMessage: {oldMessage} - title: {title} - username: {username} - choice: {choice}");

    if (oldMessage != "" && oldMessage.Contains(username, StringComparison.CurrentCultureIgnoreCase))
    {
      return sb;
    }

    Log.Logger.Information($"oldMessage: {oldMessage} - title: {title} - username: {username} - choice: {choice}");

    if (choice == "init")
    {
      sb.AppendLine(title);
      sb.AppendLine("―――――――――――――――――――――");
      Log.Logger.Information("init");
    }
    else if (choice == "yes")
    {
      sb.Append($"✅ ({username})");
      Log.Logger.Information($"yes - {username} - {sb}");
    }
    else if (choice == "no")
    {
      sb.Append($"❌ ({username})");
      Log.Logger.Information($"no - {username} - {sb}");
    }

    return sb;
  }
}