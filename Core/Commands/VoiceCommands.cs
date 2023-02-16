using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Diagnostics;

namespace DiscordBot.Core.Commands
{
    public class VoiceCommands :
        BaseCommandModule
    {
        [Command("join"), Description("Join voice channel user is in")]
        public async Task Join(CommandContext context)
        {
            // Ensure VoiceNext extension is enabled in CoolBot class
            if (context.Client.GetVoiceNext() == null)
            {
                await context.RespondAsync("VoiceNext extension is not enabled in CoolBot");
                return;
            }

            // Prevent joining channel already in
            if (context.Client.GetVoiceNext().GetConnection(context.Guild) != null)
            {
                await context.RespondAsync($"Already connected to {context.Member?.VoiceState.Channel.Name}.");
                return;
            }

            // Ensure user is connected to a voice channel
            if (context.Member?.VoiceState == null)
            {
                await context.RespondAsync($"Silly {context.Member?.DisplayName}, you're not in a voice channel!");
                return;
            }

            // Connect to voice channel
            await context.Client.GetVoiceNext().ConnectAsync(context.Member?.VoiceState.Channel);
            await context.RespondAsync($"Connected to `{context.Member?.VoiceState.Channel.Name}`");
        }


        [Command("leave"), Description("Leave voice channel currently connected to")]
        public async Task Leave(CommandContext context)
        {
            // Ensure VoiceNext extension is enabled in CoolBot class
            if (context.Client.GetVoiceNext() == null)
            {
                await context.RespondAsync("VoiceNext extension is not enabled in CoolBot");
                return;
            }

            // check whether we are connected
            if (context.Client.GetVoiceNext().GetConnection(context.Guild) == null)
            {
                // not connected
                await context.RespondAsync("Not connected in this guild.");
                return;
            }

            // disconnect
            context.Client.GetVoiceNext().GetConnection(context.Guild).Disconnect();
            await context.RespondAsync("Disconnected");
        }

        [Command("play"), Description("Plays an audio file.")]
        public async Task Play(CommandContext ctx, [RemainingText, Description("Full path to the file to play.")] string filename)
        {
            // check whether VNext is enabled
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("VNext is not enabled or configured.");
                return;
            }

            // check whether we aren't already connected
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                // already connected
                await ctx.RespondAsync("Not connected in this guild.");
                return;
            }

            // check if file exists
            if (!File.Exists(filename))
            {
                // file does not exist
                await ctx.RespondAsync($"File `{filename}` does not exist.");
                return;
            }

            // wait for current playback to finish
            while (vnc.IsPlaying)
                await vnc.WaitForPlaybackFinishAsync();

            // play
            Exception exc = null;
            await ctx.Message.RespondAsync($"Playing `{filename}`");

            try
            {
                await vnc.SendSpeakingAsync(true);

                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = $@"-i ""{filename}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                var ffmpeg = Process.Start(psi);
                var ffout = ffmpeg.StandardOutput.BaseStream;

                var txStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(txStream);
                await txStream.FlushAsync();
                await vnc.WaitForPlaybackFinishAsync();
            }
            catch (Exception ex) { exc = ex; }
            finally
            {
                await vnc.SendSpeakingAsync(false);
                await ctx.Message.RespondAsync($"Finished playing `{filename}`");
            }

            if (exc != null)
                await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
        }


    }
}
