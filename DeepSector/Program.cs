using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.Codec;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace DeepSector
{
    class Program
    {    
        static void Main(string[] args)
        {
            //asynicing run method
            Bot deepsector = new Bot();
            deepsector.RunBotAsync().GetAwaiter().GetResult();
        }
    }
}
        
