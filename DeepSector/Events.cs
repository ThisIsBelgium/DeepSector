using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DeepSector
{

    public class Events
    {
        public DiscordClient Client;
        bool active = false;
        public string json;

        public async Task run()
        {
            var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
            Client.GuildMemberAdd += async e =>
                {
                    using (DBDataContext db = new DBDataContext())
                    {

                        await e.Client.SendMessageAsync(e.Guild.DefaultChannel, $"Welcome to the show {e.Member.Username}!");
                        await e.Member.GrantRoleAsync(e.Guild.GetRole(336934234016972810));
                        var useridstring = e.Member.Id.ToString();
                        tgbr user = new tgbr
                        {
                            userid = useridstring,
                            username = e.Member.Username,
                        };
                        var userquery = from tgbr in db.tgbrs
                                        where tgbr.userid == useridstring
                                        select tgbr;
                        var querylist = userquery.ToList();
                        if (querylist.Count() == 0)
                        {
                            db.tgbrs.InsertOnSubmit(user);
                            db.SubmitChanges();
                        }
                    }

                };
            Client.GuildMemberRemove += async e =>
            {
                await e.Client.SendMessageAsync(e.Guild.DefaultChannel, $"Peace {e.Member.Username}");
            };

            Client.MessageCreated += async e =>
            {
                DiscordRole admin = default(DiscordRole);
                DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
                var roles = e.Guild.Roles;
                foreach (var role in roles)
                {
                    if (role.Name.Contains("admin"))
                    {
                        admin = role;
                    }
                }

                if (e.Message.Content.ToLower() == "#adminon" && member.Roles.FirstOrDefault() == admin || active == true)
                {
                    if (e.Message.Content.ToLower() == "#adminon")
                    {
                        active = true;
                        await e.Message.DeleteAsync();
                    }
                    else if (e.Message.Content.ToLower() == "#adminoff" && member.Roles.FirstOrDefault() == admin)
                    {
                        active = false;
                        await e.Message.DeleteAsync();
                    }
                    else
                    {
                        var spammessages = await e.Channel.GetMessagesAsync(5);
                        var spamcheck = spammessages.ToList();
                        spamcheck.RemoveAt(0);
                        foreach (var message in spamcheck)
                        {
                            if (e.Author.Id == message.Author.Id && e.Message.Content == message.Content)
                            {
                                await e.Channel.DeleteMessageAsync(message);
                            }
                        }
                        string[] bannedwords = cfgjson.BannedWords;
                        foreach (var bannedword in bannedwords)
                        {
                            if (e.Message.Content.ToLower().Contains(bannedword))
                            {
                                await e.Message.DeleteAsync();
                                DiscordMember offender = await e.Guild.GetMemberAsync(e.Author.Id);
                                DiscordDmChannel warnChannel = await offender.CreateDmChannelAsync();
                                var emoji = DiscordEmoji.FromName(e.Client, ":radioactive:");
                                var embed = new DiscordEmbed
                                {
                                    Title = emoji + "Warning",
                                    Description = $"'{e.Message.Content}' was removed due to inappropriate language try being mature for once",
                                    Color = 0xf9ff0f
                                };
                                await warnChannel.SendMessageAsync("", embed: embed);

                            }
                        }
                    }
                }

            };
            Client.MessageCreated += async e =>
            {
                using (DBDataContext db = new DBDataContext())
                {
                    if (e.Message.Content.Contains("#") == false)
                    {
                        var useridstring = e.Author.Id.ToString();
                        tgbr user = new tgbr
                        {
                            userid = useridstring,
                            username = e.Author.Username,
                        };
                        var userquery = from tgbr in db.tgbrs
                                        where tgbr.userid == useridstring
                                        select tgbr;
                        var querylist = userquery.ToList();
                        if (querylist.Count() == 0 && e.Author.IsBot == false)
                        {
                            db.tgbrs.InsertOnSubmit(user);
                            db.SubmitChanges();
                        }
                        else if (querylist.Count() == 1 && e.Author.IsBot == false)
                        {
                            if (querylist[0].msg_count >= (3 * querylist[0].levels))
                            {
                                querylist[0].levels++;
                                await leveler(querylist[0].levels, e);
                                querylist[0].msg_count = 0;
                                db.SubmitChanges();
                            }
                            else
                            {
                                querylist[0].msg_count++;
                                db.SubmitChanges();
                            }
                        }
                    }
                }
            };
            await Task.Delay(-1);
        }
        public async Task leveler(int level, MessageCreateEventArgs e)
        {
            if (level == 2)
            {
                await RoleChanger(e, "beginner");
            }
            else if (level == 3)
            {
                await RoleChanger(e, "dj");
            }
        }
        public  async Task RoleChanger(MessageCreateEventArgs e, string name)
        {
            List<DiscordRole> roles = new List<DiscordRole>();
            var serverRoles = e.Guild.Roles;
            foreach (var role in serverRoles)
            {
                if (role.Name.Contains(name))
                {
                    roles.Add(role);
                    await e.Channel.SendMessageAsync($"Congrats {e.Author.Username} has been granted the role {role}");
                }
            }
            var memeber = await e.Guild.GetMemberAsync(e.Author.Id);
            await memeber.ReplaceRolesAsync(roles);
        }
        public struct ConfigJson
        {
            [JsonProperty("token")]
            public string Token { get; private set; }

            [JsonProperty("prefix")]
            public string CommandPrefix { get; private set; }

            [JsonProperty("bannedwords")]
            public string[] BannedWords { get; private set; }
        }
    }
}
