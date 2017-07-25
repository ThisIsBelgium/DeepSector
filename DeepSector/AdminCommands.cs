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
        DBDataContext db = new DBDataContext();
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
        public async Task GiveAdmin(CommandContext ctx, DiscordMember member, DiscordRole role)
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
        public async Task Sudo(CommandContext ctx, DiscordMember member, [RemainingText] string command)
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
            foreach (var role in roles)
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
        public async Task warn(CommandContext ctx, DiscordMember member, [RemainingText] string reason)
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
        [Command("selectusers")]
        [Description("selects random users from a role dependent on number")]
        [RequirePermissions(Permissions.MoveMembers)]
        public async Task selectusers(CommandContext ctx, DiscordRole role, int amount)
        {
            List<DiscordMember> roleselection = new List<DiscordMember>();
            List<DiscordMember> selectedmembers = new List<DiscordMember>();
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            Random random = new Random();
            var members = await ctx.Guild.GetAllMembersAsync();
            foreach (var member in members)
            {
                if (member.Roles.Contains(role))
                {
                    roleselection.Add(member);
                }
            }
            var amountinrole = roleselection.Count;
            for (var i = 0; i <= amount - 1; i++)
            {
                var selected = random.Next(amountinrole);
                var selectedmember = roleselection[selected];
                selectedmembers.Add(selectedmember);
            }
            var tosend = "Here is your list of members";
            foreach (var member in selectedmembers)
            {
                tosend += "\n" + $"{member.DisplayName}";
            }
            await ctx.Channel.SendMessageAsync(tosend);

        }
        [Command("givetokens")]
        [Description("gives specified user tokens")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task givetokens(CommandContext ctx, DiscordMember member, int amount)
        {
            await ctx.Message.DeleteAsync();
            var useridstring = member.Id.ToString();
            await ctx.TriggerTypingAsync();
            var userquery = from tgbr in db.tgbrs
                            where tgbr.userid == useridstring
                            select tgbr;
            foreach (var user in userquery)
            {
                if (amount > 100)
                {
                    amount = 100;
                    var channel = await ctx.Member.CreateDmChannelAsync();
                    await channel.SendMessageAsync("Bot is angry that you tried to spam tokens remember now MAX 100 tokens");
                }
                user.tokens += amount;
                if (user.tokens >= 100)
                {
                    user.levels++;
                    await ctx.Channel.SendMessageAsync($"{user.username} just leveled to level {user.levels} congrats");
                    user.tokens = 0;
                }
                else if (user.tokens == null)
                {
                    user.tokens = 0;
                    user.tokens += amount;
                    if (user.tokens >= 100)
                    {
                        user.levels++;
                        await ctx.Channel.SendMessageAsync($"{user.username} just leveled to level {user.levels} congrats");
                        user.tokens = 0;
                    }
                    await ctx.Channel.SendMessageAsync($"{member.DisplayName} was given {amount} tokens congrats!");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync($"{member.DisplayName} was given {amount} tokens congrats!");
                }
                db.SubmitChanges();
            }

        }
        [Command("addusers")]
        [Description("adds users to db")]
        [RequireOwner]
        public async Task addusers(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            foreach (var user in ctx.Guild.Members)
            {
                var useridstring = user.Id.ToString();
                tgbr member = new tgbr
                {
                    userid = useridstring,
                    username = user.Username,
                };
                var userquery = from tgbr in db.tgbrs
                                where tgbr.userid == useridstring
                                select tgbr;
                var querylist = userquery.ToList();
                if (querylist.Count() == 0 && user.IsBot == false && user.Username != "DeepSector")
                {
                    db.tgbrs.InsertOnSubmit(member);
                    db.SubmitChanges();
                }
            }
            await ctx.Channel.SendMessageAsync("Users added to db!");
        }
    }
}