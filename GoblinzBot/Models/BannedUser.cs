public class BannedUser
{
  public ulong UserId { get; set; } = 0;

  public DateTime Until { get; set; } = DateTime.MinValue;
}