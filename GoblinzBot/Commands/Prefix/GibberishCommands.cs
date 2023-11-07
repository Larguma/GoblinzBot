using System.Diagnostics;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

public class GibberishModule : BaseCommandModule
{
  [Command("gibberish")]
  public static async Task Gibberish(CommandContext ctx)
  {
    await ctx.Message.DeleteAsync();
    DiscordChannel channel = ctx.Member.VoiceState.Channel;
    await channel.ConnectAsync();

    var vnext = ctx.Client.GetVoiceNext();
    var connection = vnext.GetConnection(ctx.Guild);

    var transmit = connection.GetTransmitSink();
    var pcm = ConvertAudioToPcm(Directory.GetCurrentDirectory() + "/Sounds/gibberish.mp3");
    await pcm.CopyToAsync(transmit);
    await pcm.DisposeAsync();
    connection.Disconnect();
  }


  private static Stream ConvertAudioToPcm(string filePath)
  {
    var ffmpeg = Process.Start(new ProcessStartInfo
    {
      FileName = "ffmpeg",
      Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
      RedirectStandardOutput = true,
      UseShellExecute = false
    });

    return ffmpeg.StandardOutput.BaseStream;
  }
}