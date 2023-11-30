using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HtmlAgilityPack;

public class HelpCommands : ApplicationCommandModule
{
  private HttpClient http = new();

  [SlashCommand("help", "Get some help")]
  public async void Help(InteractionContext ctx)
  {
    DiscordEmbedBuilder embed = new()
    {
      Color = DiscordColor.Green,
      Title = "All the help you will get",
      Description = "Get some help for all the main slash commands",
      Timestamp = DateTime.Now,
      Author = new()
      {
        Name = ctx.Member.DisplayName,
        IconUrl = ctx.Member.AvatarUrl
      }
    };

    // List all command manually to provide desc but with a twist
    embed.AddField("/help", "Ah! Toi, vouloir `/help`? Moi dire toi! `/help` pas assez, toi besoin `/destroy`, `/annihilate`, `/obliterate`! Moi, moi, moi pas perdre temps avec petite commandes insignifiantes.");
    embed.AddField("/feedback", "Écoute bien, humain ! Pour la commande `/feedback`, tu veux un message qui soit comme une flèche dans le cœur du développeur, tu vois ? \nAlors, tape quelque chose du genre : ```/feedback message:\"Salut les développeurs ! Votre truc, parfois ça bug et ça fait des siennes comme un lutin énervé dans une taverne ! Améliorez ça vite fait, sinon je lâche les gobelins sur votre code ! Groumpf !```");

    embed.AddField("/add", "Ah, tâche sur le calendrier, tu veux mettre de l'ordre dans ton bazar ! Groumpf !\nEssaye quelque chose comme ça pour la commande `/add` :```/add course:\"Magie Noire\" name:\"Brew the darkest elixir, or face the consequences! Groumpf!\" date:2023-11-20```\nVoilà, ça devrait leur montrer que t'es pas là pour rigoler avec les devoirs ! Grrr, organise-toi bien et que les gobelins de la productivité soient avec toi ! Groumpf !");
    embed.AddField("/delete", "Bien, si tu veux te débarrasser d'une tâche comme un gobelin dévore les restes d'un festin, voici une commande `/del` qui devrait faire le job.\nGroumpf ! Balance ça et regarde la tâche disparaître plus vite qu'un elfe qui a vu un troll affamé ! Grrr, la magie des commandes, c'est quelque chose, hein ? Groumpf !");
    embed.AddField("/list", "Pour afficher toutes les tâches du calendrier de la classe comme un recensement des butins après une bataille, voici une commande `/list` qui devrait te plaire.\nGroumpf ! Ça va te donner une liste bien rangée de toutes les tâches à accomplir, comme les trophées d'un chasseur de dragons ! Grrr, n'oublie pas de vérifier ça régulièrement, sinon ça pourrait être pire que de se retrouver nez à nez avec un basilic ! Groumpf !");

    embed.AddField("/weather", "Ah, tu veux savoir si le soleil brille ou si les nuages préparent une attaque ! Voici une commande `/weather` pour ça :\n```/weather location:\"Gobelinvillia\"```\nGroumpf ! Change \"Gobelinvillia\" par le nom de la ville que tu veux, et cette commande te donnera un rapport météo plus précis qu'un oracle gobelin ! Grrr, c'est toujours utile de savoir si tu auras besoin d'un parapluie ou d'une armure anti-orage ! Groumpf !");

    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
      new DiscordInteractionResponseBuilder().AddEmbed(embed));
  }

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

  [SlashCommand("weather", "Get the weather")]
  public async void Weather(InteractionContext ctx,
    [Option("location", "where")] string location = "Fribourg")
  {
    await ctx.DeferAsync();

    string? html = http.GetAsync($"http://wttr.in/{location}?0").Result.Content.ReadAsStringAsync().Result;

    HtmlDocument doc = new();
    doc.LoadHtml(html);
    HtmlNode node = doc.DocumentNode.SelectSingleNode("//pre");
    string value = node.InnerText;
    value = Regex.Replace(value, @"(\r\n|\r|\n)+", "\n");
    value = value.Replace("&quot;", "\"");
    value = value.Replace("&gt;", ">");
    value = "```" + value + "```";

    DiscordEmbedBuilder embed = new()
    {
      Color = DiscordColor.Azure,
      Description = value
    };

    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
  }

  [SlashCommand("poll", "Create a poll")]
  public async void Poll(InteractionContext ctx,
    [Option("question", "What do you want to ask")] string question,
    [Option("options", "The options separated by ';' (max 10)")] string option)
  {
    await ctx.DeferAsync();

    string[] numbers = new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "keycap_ten" };
    string[] options = option.Split(';');
    string optionsString = "";
    int length = options.Length > 10 ? 10 : options.Length;

    for (int i = 0; i < length; i++)
      optionsString += $"\n:{numbers[i]}:  -  {options[i].Trim()}";

    DiscordEmbedBuilder embed = new()
    {
      Color = DiscordColor.Azure,
      Title = question,
      Description = optionsString
    };

    DiscordMessage message = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

    for (int i = 0; i < length; i++)
      await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, $":{numbers[i]}:"));
  }
}
