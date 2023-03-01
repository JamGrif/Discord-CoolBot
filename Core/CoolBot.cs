using DiscordBot.Core.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using DSharpPlus.VoiceNext;


namespace DiscordBot.Core
{
    // Used to deserialize config.json to connect bot to Discord
    public struct ConfigBotData
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
    }

    public class CoolBot
    {
        private DiscordClient? _discordBot { get; set; }
        private InteractivityExtension? _interactivityExtension { get; set; }
        private CommandsNextExtension? _commandsExtension { get; set; }
        private VoiceNextExtension? _voiceNextExtension { get; set; }

        public static VoiceNextConnection? CurrentConnection { get; set; }

        public bool InitBot()
        {
            string configContents = string.Empty;
            try
            {
                // Parse config.json data into configContents
                FileStream fileStream = File.OpenRead("config.json");
                StreamReader streamReader = new StreamReader(fileStream, new UTF8Encoding(false));

                configContents = streamReader.ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to initialise bot - {e.Message}");
                return false;
            }

            ConfigBotData configBotData = JsonConvert.DeserializeObject<ConfigBotData>(configContents);

            // Create the bot client and set initial configuration
            _discordBot = new DiscordClient(new DiscordConfiguration
            {
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
                Token = configBotData.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            });

            // Set function which is called when bot is ready
            _discordBot.Ready += OnBotReady;

            // Enable Interactivity extension
            _discordBot.UseInteractivity(new InteractivityConfiguration
            {
                // Allow up to 2 minutes when waiting for user response
                Timeout = TimeSpan.FromMinutes(2)
            });
            
            // Enable Commands extension and set initial configuration
            _commandsExtension = _discordBot.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configBotData.Prefix },
                EnableDms = false,
                EnableMentionPrefix = false,
                DmHelp = true,
            });

            // Enable Voice extension
            _voiceNextExtension = _discordBot.UseVoiceNext();

            // Register all possible bot commands
            _commandsExtension.RegisterCommands<SimpleCommands>();
            _commandsExtension.RegisterCommands<VoiceCommands>();

            VoiceCommands.songQueue = new Queue<string>();

            return true;
        }

        public async Task RunBot()
        {
            if (_discordBot == null)
                return;

            await _discordBot.ConnectAsync();

            // Keep bot online until told to close by another source
            while (true)
            {
                // Check song queue every three seconds
                if (!VoiceCommands.songPlaying && VoiceCommands.songQueue?.Count > 0)
                    await VoiceCommands.PlayNextInQueue();
                
                await Task.Delay(2000);
            }
        }

        private Task OnBotReady(DiscordClient client, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
