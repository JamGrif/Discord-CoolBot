using System.Diagnostics;

namespace DiscordBot.Core
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var bot = new CoolBot();
            
            if (bot.InitBot())
                bot.RunBot().GetAwaiter().GetResult();
            
        }
    }
}