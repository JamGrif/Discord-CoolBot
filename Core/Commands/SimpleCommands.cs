using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
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



    }
}
