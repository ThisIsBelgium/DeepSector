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
    internal class AdminCommands
    {
        [Command("hello")]
        [Description("I'm still learning")]
        public async Task Test(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");
            await ctx.RespondAsync($"{emoji}HELLO WORLD Ping:{ctx.Client.Ping}ms");
        }
        [Command("giverole")]
        [Description("Update user roles")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task GiveAdmin(CommandContext ctx,DiscordMember member, DiscordRole role)
        {
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            List<DiscordRole> roles = new List<DiscordRole>();
            roles.Add(role);
            await member.ReplaceRolesAsync(roles);
            await ctx.RespondAsync($"{member.Username} role updated to {role}!");
            
        }
        [Command("removeroles")]
        [Description("strips specific users roles")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task RemoveRoles(CommandContext ctx, DiscordMember member)
        {
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            List<DiscordRole> roles = new List<DiscordRole>();
            await member.ReplaceRolesAsync(roles);
            await ctx.RespondAsync($"{member.Username} must've been really annoying");
        }
        [Command("sudo")]
        [Hidden]
        [Description("Executes command as user")]
        [RequireOwner]
        public async Task Sudo(CommandContext ctx,DiscordMember member,[RemainingText] string command)
        {
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            var cmds = ctx.Client.GetCommandsNext();
            await cmds.SudoAsync(member, ctx.Channel, command);
        }
        [Command("listroles")]
        [Description("lists roles + roles id's")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task ListRoles(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            var roles = ctx.Guild.Roles;
            string rolestring = null; 
            foreach(var role in roles)
            {
                rolestring += role + "\n";
            };
            var embed = new DiscordEmbed
            {
                Title = "Roles",
                Description = rolestring,
                Color = 0x6c1692,
            };
            await ctx.RespondAsync("", embed: embed);
        }
        [Command("prune")]
        [Description("removes a user determined amount of messages from a channel")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task prune(CommandContext ctx, int msgamt)
        {
            await ctx.Message.DeleteAsync();
            var messagesToDelete = await ctx.Channel.GetMessagesAsync(msgamt);
            await ctx.Channel.DeleteMessagesAsync(messagesToDelete);
        } 
        [Command("warn")]
        [Description("dms a user warning them")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task warn(CommandContext ctx, DiscordMember member,[RemainingText] string reason)
        {
            await ctx.Message.DeleteAsync();
            DiscordDmChannel channel = await member.CreateDmChannelAsync();
            await channel.TriggerTypingAsync();
            var emoji = DiscordEmoji.FromName(ctx.Client, ":radioactive:");
            var embed = new DiscordEmbed
            {
                Title = emoji + "Warning",
                Description = reason,
                Color = 0xf9ff0f
            };
            await channel.SendMessageAsync("", embed: embed); 
        }
        [Command("softban")]
        [Description("Puts user in a role that stops them from talking")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task softban(CommandContext ctx, DiscordMember member, [RemainingText] string reason)
        {
            await ctx.Message.DeleteAsync();
            List<DiscordRole> softban = new List<DiscordRole>();
            softban.Add(ctx.Guild.GetRole(337330093368279052));
            var emoji = DiscordEmoji.FromName(ctx.Client, ":no_entry_sign:");
            DiscordDmChannel channel = await member.CreateDmChannelAsync();
            await channel.TriggerTypingAsync();
            await member.ReplaceRolesAsync(softban);
            var embed = new DiscordEmbed
            {
                Title = emoji + "Softban",
                Description = reason + "\n" + "Please contact a mod or an admin to get your role back", 
                Color = 0xf9ff0f
            };
            await channel.SendMessageAsync("", embed: embed);
        }
    }
}