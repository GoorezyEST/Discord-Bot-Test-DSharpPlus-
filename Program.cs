using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Creating a new instance of our bot
            Bot bot = new Bot();
            //Executing it to be online
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
