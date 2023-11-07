using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

public class HelpCommands : ApplicationCommandModule
{
  [SlashCommand("help", "Get some help")]
  public static async void HelpCommand(InteractionContext ctx) =>
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
      new DiscordInteractionResponseBuilder().WithContent("TODO lol"));
}
