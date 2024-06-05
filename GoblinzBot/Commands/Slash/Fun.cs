using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

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
  public static async void Surrender(InteractionContext ctx)
  {
    await ctx.DeferAsync();

    StringBuilder sb = GetFormatedSurrender("init");

    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
      .AddComponents(Program.GetSurrenderButtonComponent())
      .WithContent(sb.ToString()));
  }

  internal static StringBuilder GetFormatedSurrender(string choice, string oldMessage = "")
  {
    StringBuilder sb = new();

    sb.AppendLine(oldMessage);

    if (choice == "init")
    {
      sb.AppendLine("      Surrender      ");
      sb.AppendLine("―――――――――――――――――――――");
    }
    else if (choice == "yes")
    {
      sb.Append('✅');
    }
    else if (choice == "no")
    {
      sb.Append('❌');
    }

    return sb;
  }
}