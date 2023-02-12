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
    public struct ConfigData
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
        private VoiceNextExtension _voiceExtension { get; set; }

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
                Console.WriteLine("Failed to initialise bot - {0}", e.Message );
                return false;
            }

            ConfigData configJson = JsonConvert.DeserializeObject<ConfigData>(configContents);

            var config = new DiscordConfiguration
            {
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };

            _discordBot = new DiscordClient(config);

            // Set function called when bot ready
            _discordBot.Ready += OnBotReady;

            // Enable Interactivity extension
            _discordBot.UseInteractivity(new InteractivityConfiguration
            {
                // Allow up to 2 minutes when waiting for user response
                Timeout = TimeSpan.FromMinutes(2)
            });
            
            // Configure and create the Commands extension
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableDms = false,
                EnableMentionPrefix = false,
                DmHelp = true,
            };
            _commandsExtension = _discordBot.UseCommandsNext(commandsConfig);

            _voiceExtension = _discordBot.UseVoiceNext();

            // Register all possible bot commands
            _commandsExtension.RegisterCommands<SimpleCommands>();
            _commandsExtension.RegisterCommands<TeamCommands>();

            return true;
        }

        public async Task RunBot()
        {
            await _discordBot.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnBotReady(DiscordClient client, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
