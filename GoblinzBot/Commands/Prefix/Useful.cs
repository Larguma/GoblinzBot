using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

public class UsefulModule : BaseCommandModule
{
  [Command("time")]
  [Description("Print the IL timetable")]
  public async Task ILTimetable(CommandContext ctx) {
    await ctx.Message.DeleteAsync();
    await ctx.RespondAsync("https://i.ibb.co/Ws0ZxFy/image.png");
  }
}