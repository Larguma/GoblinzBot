using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

public class HelpCommands : ApplicationCommandModule
{
  [SlashCommand("help", "Get some help")]
  public async void Help(InteractionContext ctx) =>
    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
      new DiscordInteractionResponseBuilder().WithContent("TODO: implement help command"));

  [SlashCommand("feedback", "Send a feedback to the dev")]
  public async void Feedback(InteractionContext ctx,
    [Option("message", "The message you want to send")] string fb)
  {
    if (ctx.Guild == null)
    {
      await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
      new DiscordInteractionResponseBuilder().WithContent("This command can only be used in a server!"));
      return;
    }
    
    DiscordEmbedBuilder embed = new()
    {
      Color = DiscordColor.Green,
      Description = fb,
      Timestamp = DateTime.Now,
      Author = new()
      {
        Name = $"{ctx.Member.DisplayName} ({ctx.Member.Id})",
        IconUrl = ctx.Member.AvatarUrl
      }
    };

    embed.AddField("Guild", $"{ctx.Guild.Name} ({ctx.Guild.Id})", true);

    DiscordMember discordMember = await ctx.Guild.GetMemberAsync(152089006430093312);    
    await discordMember.SendMessageAsync(embed);

    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
      new DiscordInteractionResponseBuilder().WithContent("Feedback sent!"));
  }
}
