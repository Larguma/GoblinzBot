using System.Diagnostics;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

public class GibberishModule : BaseCommandModule
{
  /// <summary>
  /// Plays a gibberish sound effect in the user's current voice channel.
  /// </summary>
  /// <param name="ctx"></param>
  [Command("gibberish")]
  [Description("Play a gibberish sound effect")]
  public async Task Gibberish(CommandContext ctx)
  {
    if (ctx.Member == null || ctx.Member.VoiceState.Channel == null)
    {
      await ctx.RespondAsync("You must be in a voice channel!");
      return;
    }

    await ctx.Message.DeleteAsync();
    DiscordChannel channel = ctx.Member.VoiceState.Channel;
    await channel.ConnectAsync();

    VoiceNextExtension vnext = ctx.Client.GetVoiceNext();
    VoiceNextConnection connection = vnext.GetConnection(ctx.Guild);

    VoiceTransmitSink transmit = connection.GetTransmitSink();
    Stream pcm = ConvertAudioToPcm(Directory.GetCurrentDirectory() + "/Sounds/gibberish.mp3");
    await pcm.CopyToAsync(transmit);
    await pcm.DisposeAsync();
    connection.Disconnect();
  }

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

  private Stream ConvertAudioToPcm(string filePath)
  {
    Process? ffmpeg = Process.Start(new ProcessStartInfo
    {
      FileName = "ffmpeg",
      Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
      RedirectStandardOutput = true,
      UseShellExecute = false
    });

    return ffmpeg.StandardOutput.BaseStream;
  }

  private async Task DeleteMessageAsync(CommandContext ctx)
  {
    if (ctx.Message.Author.IsBot) return;
    if (ctx.Channel.IsPrivate) return;
    await ctx.Message.DeleteAsync();
  }
}