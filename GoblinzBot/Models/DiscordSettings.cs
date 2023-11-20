public class DiscordSettings
{
    public required string Token { get; set; }

    public required string TokenDev { get; set; }

    public required string ClientId { get; set; }

    public required string GuildId { get; set; }

    public required string OpenaiToken { get; set; }

    public required Lists Lists { get; set; }
}

public class Lists
{
    public required List<string> ItsJoever { get; set; }

    public required List<string> RockAndStone { get; set; }

    public required List<string> GoodBot { get; set; }

    public required List<string> BadBot { get; set; }
}