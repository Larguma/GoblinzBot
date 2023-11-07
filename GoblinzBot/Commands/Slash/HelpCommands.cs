using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

public class HelpCommands : ApplicationCommandModule
{
  [SlashCommand("help", "Get some help")]
  public async Task HelpCommand(InteractionContext ctx)
  {
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("TODO lol"));
  }
}
