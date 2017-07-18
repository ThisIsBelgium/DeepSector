using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Collections.Generic;

namespace DeepSector
{
    [Hidden]
    [RequirePermissions(Permissions.ManageGuild)]
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
        [Command("giveadmin")]
        [Description("Update user roles")]
        public async Task GiveAdmin(CommandContext ctx,DiscordMember member)
        {
            await ctx.TriggerTypingAsync();
            var role = ctx.Guild.GetRole(336906767109849107);
            List<DiscordRole> roles = new List<DiscordRole>();
            roles.Add(role);
            await member.ReplaceRolesAsync(roles);
            await ctx.RespondAsync($"{member.Mention} role updated to Admin!");
            
        }
        [Command("removeroles")]
        [Description("strips specific users roles")]
        public async Task RemoveRoles(CommandContext ctx, DiscordMember member)
        {
            await ctx.TriggerTypingAsync();
            List<DiscordRole> roles = new List<DiscordRole>();
            await member.ReplaceRolesAsync(roles);
            await ctx.RespondAsync($"{member.Mention} must've been really annoying");
        }
        [Command("sudo")]
        [Description("Executes command as user")]
        [RequireOwner]
        public async Task Sudo(CommandContext ctx,DiscordMember member,[RemainingText] string command)
        {
            await ctx.TriggerTypingAsync();
            var cmds = ctx.Client.GetCommandsNext();
            await cmds.SudoAsync(member, ctx.Channel, command);
        } 
    }
}