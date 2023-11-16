using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

public class GibberishModule : BaseCommandModule
{
  [Command("linker")]
  [Description("Print a linker error")]
  public async Task LinkerError(CommandContext ctx)
  {
    await DeleteMessageAsync(ctx);
    await ctx.RespondAsync("https://i.ibb.co/BrVR74f/Whats-App-Image-2023-11-09-at-14-30-48-1.jpg");
  }

  [Command("toast")]
  [Description("*Clink* a toast")]
  public async Task ClinkToast(CommandContext ctx)
  {
    await DeleteMessageAsync(ctx);
    await ctx.RespondAsync("https://i.pinimg.com/736x/3c/b5/5c/3cb55cd11d436eb4b5bdb8e8cfc3ac1a.jpg");
  }

  [Command("rat")]
  [Description("RRRRRRRRRRRRAT")]
  public async Task RRRAT(CommandContext ctx)
  {
    await DeleteMessageAsync(ctx);
    await ctx.RespondAsync("https://pbs.twimg.com/media/FPgA0D7XEAAtzr4.jpg");
  }

  [Command("spin")]
  [Description("Horizontally spinning rat")]
  public async Task RatSpin(CommandContext ctx)
  {
    await DeleteMessageAsync(ctx);
    await ctx.RespondAsync("https://media.tenor.com/RfJzepsDdmYAAAAC/rat-spinning.gif");
  }

  private async Task DeleteMessageAsync(CommandContext ctx)
  {
    if (ctx.Message.Author.IsBot) return;
    if (ctx.Channel.IsPrivate) return;
    await ctx.Message.DeleteAsync();
  }
}