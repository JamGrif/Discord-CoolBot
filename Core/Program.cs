

namespace DiscordBot.Core
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var bot = new CoolBot();

            if (bot.InitBot())
                bot.RunBot().GetAwaiter().GetResult();

            Console.WriteLine("hi2");
        }
    }
}