namespace GoblinzBot.Model
{
    public class Config
    {
        public required string Token { get; set; }

        public required Lists Lists { get; set; }

    }

    public class Lists
    {
        public required List<string> ItsJoever { get; set; }
        public required List<string> RockAndStone { get; set; }
    }
}