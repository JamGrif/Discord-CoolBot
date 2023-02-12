using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;


namespace DiscordBot.Core.Commands
{
    public class SimpleCommands :
        BaseCommandModule
    {
        [Command("ping")]
        [Description("Returns pong")]
        public async Task Ping(CommandContext context)
        {
            //await context.Channel.SendMessageAsync(context.Channel.Name);
            await context.Channel.SendMessageAsync("Pong at " + context.Channel.Name);
        }

        [Command("add")]
        [Description("Adds two numbers together")]
        public async Task Add(CommandContext context,
            [Description("First Number")] int numberOne,
            [Description("Second Number")] int numberTwo)
        {
            await context.Channel.SendMessageAsync((numberOne + numberTwo).ToString()).ConfigureAwait(false);
        }

        [Command("responsemessage")]
        public async Task ResponseMessage(CommandContext context)
        {
            var interactivity = context.Client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == context.Channel).ConfigureAwait(false);

            await context.Channel.SendMessageAsync(message.Result.Content);
        }

        [Command("responsereaction")]
        public async Task ResponseReaction(CommandContext context)
        {
            var interactivity = context.Client.GetInteractivity();

            var message = await interactivity.WaitForReactionAsync(x => x.Channel == context.Channel).ConfigureAwait(false);

            await context.Channel.SendMessageAsync(message.Result.Emoji);
        }

        [Command("poll")]
        public async Task Poll(CommandContext context, TimeSpan duration, params DiscordEmoji[] emojiOptions)
        {
            var interactivity = context.Client.GetInteractivity();

            var options = emojiOptions.Select(x => x.ToString());

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = "Poll",
                Description = string.Join(" ", options),
            };

            var pollMessage = await context.Channel.SendMessageAsync(embed: pollEmbed);

            foreach (var item in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(item);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration);
            var results = result.Select(x => $"{x.Emoji}: {x.Total}");

            await context.Channel.SendMessageAsync(string.Join("\n", results));
        }
    }
}
