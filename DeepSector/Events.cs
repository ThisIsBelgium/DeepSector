﻿using System;
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

    public class Events
    {
        public DiscordClient Client;
        DBDataContext db = new DBDataContext();
        bool active = false;
        public string json;
        
        public async Task run()
        {
            var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
            Client.GuildMemberAdd += async e =>
                {
                    await e.Client.SendMessageAsync(e.Guild.DefaultChannel, $"Welcome to the shit show {e.Member.Username}!");
                    await e.Member.GrantRoleAsync(e.Guild.GetRole(336934234016972810));
                    var useridstring = e.Member.Id.ToString();
                    tgbr user = new tgbr
                    {
                        userid = useridstring,
                        username = e.Member.Username,
                        tokens = 0
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

                };
            Client.GuildMemberRemove += async e =>
            {
                await e.Client.SendMessageAsync(e.Guild.DefaultChannel, $"Peace {e.Member.Username}, must've been a nerd");
            };

            Client.MessageCreated += async e =>
            {
                DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
                var admin = e.Guild.GetRole(336906767109849107);

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
            await Task.Delay(-1);

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