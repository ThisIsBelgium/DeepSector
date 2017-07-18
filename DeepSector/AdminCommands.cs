using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DeepSector
{
    internal class AdminCommands
    {
        [Command("hello")]
        [Description("I'm still learning")]
        public async Task Test(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");
            await ctx.RespondAsync($"{emoji}HELLO WORLD Ping:{ctx.Client.Ping}ms");
        }
    }
}