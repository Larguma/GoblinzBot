public class DiscordSettings
{
    public string Token { get; set; } = string.Empty;

    public string TokenDev { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string GuildId { get; set; } = string.Empty;

    public string OpenaiToken { get; set; } = string.Empty;

    public Lists Lists { get; set; } = new();
}

public class Lists
{
    public List<string> ItsJoever { get; set; } = [];

    public List<string> RockAndStone { get; set; } = [];

    public List<string> GoodBot { get; set; } = [];

    public List<string> BadBot { get; set; } = [];

    public List<string> RussianDead { get; set; } = [];

    public List<string> RussianAlive { get; set; } = [];

    public List<string> Insult { get; set; } = [];

    public List<string> JavaWord { get; set; } = [];

    public List<string> KillList { get; set; } = [];
}