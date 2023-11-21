using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

public class FunCommands : ApplicationCommandModule
{
  private HttpClient http = new();
  private Random rdn = new();

  [SlashCommand("roast", "Insult someone")]
  public async void Help(InteractionContext ctx,
    [Option("user", "The user you want to insult")] DiscordUser user)
  {
    List<string> insults = Program.DiscordSettings.Lists.Insult;
    string insult = insults[rdn.Next(insults.Count)];

    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
      new DiscordInteractionResponseBuilder().WithContent($"{user.Mention}{insult}").AddMentions(new IMention[] { new UserMention(user) }));
  }
}