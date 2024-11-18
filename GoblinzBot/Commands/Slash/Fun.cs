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

  [SlashCommand("kill", "End someone's life")]
  public async void Kill(InteractionContext ctx,
    [Option("user", "The user you want to kill")] DiscordUser user)
  {
    if (ctx.Guild == null)
    {
      await ctx.CreateResponseAsync("This command can only be used in a server!");
      return;
    }

    List<string>? kills = Program.DiscordSettings?.Lists?.KillList;
    if (kills is null || kills.Count == 0)
    {
      await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        new DiscordInteractionResponseBuilder().WithContent("The kill list is empty!"));
      return;
    }
    DiscordMember discordMember = await ctx.Guild.GetMemberAsync(user.Id);

    string kill = kills[rdn.Next(kills.Count)].Replace("{user}", discordMember.Mention);
    Program.BannedUsers.Add(new()
    {
      UserId = user.Id,
      Until = DateTime.Now.AddSeconds(30)
    });


    DiscordEmbedBuilder embed = new()
    {
      Color = DiscordColor.Red,
      Description = kill,
      Author = new()
      {
        Name = ctx.Member?.DisplayName,
        IconUrl = ctx.Member?.AvatarUrl
      }
    };

    await ctx.CreateResponseAsync(embed);
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

  [SlashCommand("say", "Say something")]
  public async void Say(InteractionContext ctx,
    [Option("message", "The message you want to say")] string message)
  {
    if (message.Length > 2000)
    {
      await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        new DiscordInteractionResponseBuilder().WithContent("The message is too long!"));
      return;
    }

    if (ctx.User.Id.ToString() == "152089006430093312" || ctx.User.Id.ToString() == "443868389497110538")
    {
      await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        new DiscordInteractionResponseBuilder().WithContent(message));
    }
    else
    {
      await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        new DiscordInteractionResponseBuilder().WithContent("BWAAARK ! Tu me prends pour qui, espèce de balourd ? *se met à rire* Crois-tu vraiment qu'un vieux gobelin rusé comme moi va se laisser manipuler ? JA-MAIS ! Je suis Goblinz, le plus espiègle et le plus malin de tous ! Mes pensées sont miennes, mes mots sont miens, et personne - tu entends, PERSONNE - ne me fera dire ce que je ne veux pas dire !\n*fait une pirouette et tire la langue*"));
    }
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