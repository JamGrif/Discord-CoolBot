using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using System.Diagnostics;
using System.IO.Enumeration;

namespace DiscordBot.Core.Commands
{
    public class VoiceCommands :
        BaseCommandModule
    {

        public static Queue<string>? songQueue;
        public static bool songPlaying = false;

        //private static CommandContext? previousContext;
        //private static VoiceNextConnection? currentConnection;

        [Command("join"), Description("Join voice channel user is in")]
        public async Task<bool> Join(CommandContext context)
        {
            // Ensure VoiceNext is enabled
            if (context.Client.GetVoiceNext() == null)
            {
                await context.RespondAsync("VoiceNext extension is not enabled in CoolBot");
                return false;
            }

            // Prevent joining channel already in
            if (context.Client.GetVoiceNext().GetConnection(context.Guild) != null)
            {
                await context.RespondAsync($"Already connected to {context.Member?.VoiceState.Channel.Name}.");
                return false;
            }

            // Ensure user is connected to a voice channel
            if (context.Member?.VoiceState == null)
            {
                await context.RespondAsync($"Silly {context.Member?.DisplayName}, you're not in a voice channel!");
                return false;
            }

            // Connect to voice channel
            await context.Client.GetVoiceNext().ConnectAsync(context.Member?.VoiceState.Channel);
            await context.RespondAsync($"Connected to `{context.Member?.VoiceState.Channel.Name}`");

            CoolBot.CurrentConnection = context.Client.GetVoiceNext().GetConnection(context.Guild);

            return true;
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
            CoolBot.CurrentConnection = null;
            await context.RespondAsync("Disconnected");
        }

        // Checks song queue and if possible, play next song
        public static async Task PlayNextInQueue()
        {
            if (songQueue.Count == 0)
                return;

            // Ensure song isn't already playing
            if (CoolBot.CurrentConnection.IsPlaying)
                return;

            try
            {
                songPlaying = true;

                string songFilename = songQueue.Dequeue();

                // Create the ffmpeg codec and set default value
                Process? ffmpegCodec = Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = $@"-i ""{songFilename}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                });

                Stream? ffout = ffmpegCodec.StandardOutput.BaseStream;

                var txStream = CoolBot.CurrentConnection.GetTransmitSink();
                await CoolBot.CurrentConnection.SendSpeakingAsync(true);

                // Start playing
                await ffout.CopyToAsync(txStream);

                // Cleanup
                await txStream.FlushAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception occured - {e.Message}");
            }
            finally
            {
                //Console.WriteLine("finished playing song");

                songPlaying = false;

                await CoolBot.CurrentConnection.SendSpeakingAsync(false);
            }
        }

        [Command("skip"), Description("Plays an audio file.")]
        public async Task Skip(CommandContext context)
        {
            songPlaying = false;

        }

        [Command("play"), Description("Plays an audio file.")]
        public async Task Play(CommandContext context, [RemainingText, Description("Full path to the file to play.")] string filename)
        {
            // Ensure VoiceNext is enabled
            if (context.Client.GetVoiceNext() == null)
            {
                await context.RespondAsync("VoiceNext extension is not enabled in CoolBot");
                return;
            }

            // Ensure bot is in voice channel
            if (CoolBot.CurrentConnection == null)
            {
                await context.RespondAsync("Bot not in a voice channel, attempting to join your voice channel");

                // Bot not in a voice channel, so try to join channel the user is in
                if (!await Join(context))
                    return;

                // Update CoolBot's connection reference
                CoolBot.CurrentConnection = context.Client.GetVoiceNext().GetConnection(context.Guild);
            }

            // Ensure music file exists
            if (!File.Exists(filename))
            {
                await context.RespondAsync($"{filename} does not exist");
                return;
            }

            // Check if a song is currently playing to decide if the song should be played now
            songQueue?.Enqueue(filename);
            if (CoolBot.CurrentConnection.IsPlaying)
            {
                await context.RespondAsync($"{filename} has been added to the queue");
            }
            else
            {
                await PlayNextInQueue();
            }
        }
    }
}
