using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Collections.Generic;

namespace DeepSector
{
    internal class ClientCommands
    {

        [Command("viewlevels")]
        [Description("allows clients to see their levels")]
        public async Task viewlevels(CommandContext ctx)
        {
            using (DBDataContext db = new DBDataContext())
            {
                await ctx.Message.DeleteAsync();
                await ctx.TriggerTypingAsync();
                var useridstring = ctx.Member.Id.ToString();
                var userquery = from tgbr in db.tgbrs
                                where tgbr.userid == useridstring
                                select tgbr;
                foreach (var user in userquery)
                {
                    await ctx.Channel.SendMessageAsync($"{user.username} is currently level {user.levels}!");
                }
            }
        }
        [Command("viewtokens")]
        [Description("allows clients to see their tokens")]
        public async Task viewtokens(CommandContext ctx)
        {
            using (DBDataContext db = new DBDataContext())
            {
                await ctx.Message.DeleteAsync();
                await ctx.TriggerTypingAsync();
                var useridstring = ctx.Member.Id.ToString();
                var userquery = from tgbr in db.tgbrs
                                where tgbr.userid == useridstring
                                select tgbr;
                foreach (var user in userquery)
                {
                    await ctx.Channel.SendMessageAsync($"{user.username} currently has {user.tokens} tokens!");
                }

            }
        }
        [Command("stats")]
        [Description("Shows various stats about the current guild")]
        public async Task stats(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            var amountofuser = new DiscordEmbedField()
            {
                Name = "Users",
                Value = ctx.Guild.MemberCount.ToString()
            };
            var guildregion = new DiscordEmbedField()
            {
                Name = "Region",
                Value = ctx.Guild.RegionId
            };
            var creation = new DiscordEmbedField()
            {
                Name = "Date created",
                Value = ctx.Guild.CreationDate.ToString()
            };
            DiscordEmbedImage image = new DiscordEmbedImage();
            List<DiscordEmbedField> fields = new List<DiscordEmbedField>();
            fields.Add(amountofuser);
            fields.Add(guildregion);
            fields.Add(creation);
            var embed = new DiscordEmbed()
            {
                Fields = fields,
                Title = $"{ctx.Guild.Name} Stats",
                Color = 0x704196

            };
            await ctx.Channel.SendMessageAsync("", embed: embed);
        }
    }
}
