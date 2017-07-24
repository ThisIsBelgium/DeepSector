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
        public DiscordClient Client { get; set; }
        public CommandsNextModule Commands { get; set; }
        public VoiceNextClient Voice { get; set; }

        static void Main(string[] args)
        {
            //asynicing run method
            var prog = new Program();
            prog.RunBotAsync().GetAwaiter().GetResult();
        }
        public async Task RunBotAsync()
        {
            //config load
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();
            //loading values
            var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
            var cfg = new DiscordConfig
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LargeThreshold = 250,
                DiscordBranch = Branch.Stable,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true,
            };
            //Make Client
            this.Client = new DiscordClient(cfg);
            this.Client.Ready += this.Client_Ready;
            this.Client.GuildAvailable += this.Client_GuildAvailable;
            this.Client.ClientError += this.Client_ClientError;
            var ccfg = new CommandsNextConfiguration
            {
                StringPrefix = cfgjson.CommandPrefix,
                EnableDms = true,
                EnableDefaultHelp = true
            };
            this.Commands = this.Client.UseCommandsNext(ccfg);
            this.Commands.CommandExecuted += this.Commands_CommandExecuted;
            this.Commands.CommandErrored += this.Commands_CommandErrored;
            this.Commands.RegisterCommands<AdminCommands>();
            //Events
            Events events = new Events();
            events.Client = Client;
            events.json = json;
            //connect
            await this.Client.ConnectAsync();
            //stay open
            await events.run();
            await Task.Delay(-1);
        }
        private Task Client_Ready(ReadyEventArgs e)
        {
            //log event
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "DeepSector", "Client is stable", DateTime.Now);
            return Task.CompletedTask;
        }
        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            //log event
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "DeepSector", $"Guild available:{e.Guild.Name}", DateTime.Now);
            return Task.CompletedTask;
        }
        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "DeepSector", $"Exception occured:{e.Exception.GetType()}:{e.Exception.Message}", DateTime.Now);
            return Task.CompletedTask;
        }
        private Task Commands_CommandExecuted(CommandExecutedEventArgs e)
        {
            //log commands
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "DeepSector", $"{e.Context.User.Username} successfully executed'{e.Command.QualifiedName}'", DateTime.Now);
            return Task.CompletedTask;
        }
        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            //log errored commands
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "DeepSector", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}'but it failed:{e.Exception.GetType()}:{e.Exception.Message ?? "<no message>"}", DateTime.Now);
            if (e.Exception is ChecksFailedException)
            {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                var embed = new DiscordEmbed
                {
                    Title = "Nope",
                    Description = $"{emoji}Nope good try tho",
                    Color = 0xff0000
                };
                await e.Context.RespondAsync("", embed: embed);
            }
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
