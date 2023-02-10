using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;

namespace DiscordBot.Core.Commands
{
    public class FunCommands :
        BaseCommandModule
    {
        [Command("ping")]
        [Description("Returns pong")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }

        [Command("add")]
        [Description("Adds two numbers together")]
        public async Task Add(CommandContext ctx,
            [Description("First Number")] int numberOne,
            [Description("Second Number")] int numberTwo)
        {
            await ctx.Channel.SendMessageAsync((numberOne + numberTwo).ToString()).ConfigureAwait(false);
        }

        [Command("responsemessage")]
        public async Task ResponseMessage(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Content);
        }

        [Command("responsereaction")]
        public async Task ResponseReaction(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForReactionAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Emoji);
        }



    }
}
