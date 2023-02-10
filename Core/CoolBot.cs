using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using DiscordBot.Core.Commands;

namespace DiscordBot.Core
{
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; private set; }
    }

    public class CoolBot
    {

        public DiscordClient? Client { get; private set; }
        public InteractivityExtension? Interactivity { get; private set; }
        public CommandsNextExtension? Commands { get; private set; }

        public bool InitBot()
        {
            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);


            var config = new DiscordConfiguration
            {
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };

            Client = new DiscordClient(config);

            // What to do when bot turns on
            Client.Ready += OnClientReady;

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableDms = false,
                EnableMentionPrefix = false,
                DmHelp = true,
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            // Register all commands in Commands classes
            Commands.RegisterCommands<FunCommands>();
            Commands.RegisterCommands<TeamCommands>();

            return true;
        }

        public async Task RunBot()
        {
            await Client.ConnectAsync();

            await Task.Delay(-1);
        }



        private Task OnClientReady(DiscordClient client, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
