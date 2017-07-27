using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using PUBGSharp;

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
        public T CallRestMethod<T>(string baseaddress, string url, T type)
        {

            T result = default(T);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(baseaddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<T>(jsonString.Result, settings);
                }
            }
            return result;
        }
        [Command("ow")]
        [Description("returns stats about a specific ow account")]
        public async Task ow(CommandContext ctx, string gameType, [RemainingText] string battletag)
        {
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            OWGameStatsRootObject owgamestats = new OWGameStatsRootObject();
            OWProfileStatsRootObject owprofilestats = new OWProfileStatsRootObject();
            string baseaddress = "http://ow-api.herokuapp.com/";
            string url = "/stats/pc/us/" + battletag;
            var gameStats = CallRestMethod<OWGameStatsRootObject>(baseaddress, url, owgamestats);
            string url2 = "profile/pc/us/" + battletag;
            var profileStats = CallRestMethod<OWProfileStatsRootObject>(baseaddress, url2, owprofilestats);
            if (gameType == "competitive")
            {
                var sr = new DiscordEmbedField()
                {
                    Name = "SR",
                    Value = profileStats.competitive.rank.ToString()
                };
                var wins = new DiscordEmbedField()
                {
                    Name = "Wins",
                    Value = profileStats.games.competitive.won.ToString()
                };
                var losses = new DiscordEmbedField()
                {
                    Name = "Losses",
                    Value = profileStats.games.competitive.lost.ToString()
                };
                List<DiscordEmbedField> fields = new List<DiscordEmbedField>();
                fields.Add(sr);
                fields.Add(wins);
                fields.Add(losses);
                var embed = new DiscordEmbed()
                {
                    Fields = fields,
                    Title = $"{profileStats.username}'s competitive Overwatch stats",
                    Color = 0x704196

                };
                await ctx.Channel.SendMessageAsync("", embed: embed);
            }
            else if (gameType == "quickplay")
            {
                var level = new DiscordEmbedField()
                {
                    Name = "Level",
                    Value = profileStats.level.ToString()
                };
                var wins = new DiscordEmbedField()
                {
                    Name = "Wins",
                    Value = profileStats.games.quickplay.won.ToString()
                };
                var timeplayed = new DiscordEmbedField()
                {
                    Name = "Time Played",
                    Value = profileStats.playtime.quickplay.ToString()
                };
                List<DiscordEmbedField> fields = new List<DiscordEmbedField>();
                fields.Add(level);
                fields.Add(wins);
                fields.Add(timeplayed);
                var embed = new DiscordEmbed()
                {
                    Fields = fields,
                    Title = $"{profileStats.username}'s quickplay Overwatch stats",
                    Color = 0x704196

                };
                await ctx.Channel.SendMessageAsync("", embed: embed);
            }
        }
        [Command("pubg")]
        [Description("returns stats about a pubg account")]
        public async Task pubg(CommandContext ctx,[RemainingText] string username)
        {
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            var statsclient = new PUBGStatsClient("e2ac240b-3f00-404a-87db-d3f57d2f3c09");
            var stats = await statsclient.GetPlayerStatsAsync(username);
        }
    }
}
