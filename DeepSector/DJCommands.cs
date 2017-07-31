using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using WrapYoutubeDl;
using System.Collections.Generic;

namespace DeepSector
{

    public class DJCommands
    {
        System.IO.DirectoryInfo di = new DirectoryInfo(@"C:\Users\wheeler\Desktop\Music");
        CancellationTokenSource source = new CancellationTokenSource();
        List<Songs> playlist = new List<Songs>();
        bool paused;
        [Command("join")]
        [Description("joins the users current voice channel")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task join(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
            {
                await ctx.RespondAsync("Voice not enabled or configured");
                return;
            }
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                await ctx.RespondAsync("Already in guild!");
                return;
            }
            var vstat = ctx.Member?.VoiceState;
            if ((vstat == null || vstat.Channel == null) && vstat.Channel == null)
            {
                await ctx.RespondAsync("You are not in a channel");
                return;
            }
            vnc = await vnext.ConnectAsync(vstat.Channel);
        }
        [Command("leave")]
        [Description("leaves the voice channel")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task leave(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            var vnc = await NotInGuildVerification(ctx);
            vnc.Disconnect();
            playlist.Clear();
            await wipe(ctx);
        }
        [Command("add")]
        [Description("adds a track to the guilds music queue")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task add(CommandContext ctx, [RemainingText] string trackname)
        {
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyDZDEfX3R11DpdSJ5w7BvGDoZlNQwOZrrE",
                ApplicationName = this.GetType().ToString()
            });
            var searchlist = youtubeService.Search.List("snippet");
            searchlist.Q = trackname;
            searchlist.MaxResults = 1;
            var searchlistresponse = await searchlist.ExecuteAsync();
            string dl = "http://youtube.com/watch?v=" + searchlistresponse.Items[0].Id.VideoId;
            var filename = searchlistresponse.Items[0].Id.VideoId;
            var outputfolder = "C:/Users/wheeler/Desktop/Music/";
            Songs song = new Songs
            {
                filename = searchlistresponse.Items[0].Snippet.Title,
                filepath = outputfolder + filename + ".mp3"
            };
            try
            {
                var downloader = new AudioDownloader(dl, filename, outputfolder);
                downloader.FinishedDownload += finished(searchlistresponse, ctx, song);
                downloader.Download();
            }
            catch
            {
               await ctx.Channel.SendMessageAsync("File already in folder");
               playlist.Add(song);
            }


        }
        public AudioDownloader.FinishedDownloadEventHandler finished(SearchListResponse searchlistresponse, CommandContext ctx, Songs song)
        {
            ctx.RespondAsync($"{searchlistresponse.Items[0].Snippet.Title} has been downloaded");
            playlist.Add(song);
            return null;
        }
        [Command("play")]
        [Description("plays the currently queued songs")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task play(CommandContext ctx)
        {
            var token = source.Token;
            await ctx.Message.DeleteAsync();
            var vnc = await NotInGuildVerification(ctx);
            for (var i =0;i<=playlist.Count;i++)
                {
                    while (vnc.IsPlaying)
                    {

                        await vnc.WaitForPlaybackFinishAsync();
                    }
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    Exception exc = null;
                    await ctx.Message.RespondAsync($"Playing `{playlist[i].filename}`");
                    await vnc.SendSpeakingAsync(true);
                    try
                    {
                        await convert(vnc, playlist[i], token);
                    }
                    catch (Exception ex)
                    {
                        exc = ex;
                    }
                    finally
                    {
                        await vnc.SendSpeakingAsync(false);
                    }
                    if (exc != null)
                    {
                        await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
                    }

                }
            await wipe(ctx);
            playlist.Clear();
        }
        [Command("skip")]
        [Description("skips the currently playing track in the playlist")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task skip(CommandContext ctx)
        {
            var vnc = await NotInGuildVerification(ctx);
            if (vnc.IsPlaying == false)
            {
                await ctx.RespondAsync("No music currently being played");
            }
            else
            {
                source.Cancel();
                source = new CancellationTokenSource();
                var token = source.Token;
                playlist.RemoveAt(0);
                if (playlist.Count == 0)
                {
                    await wipe(ctx);
                }
                await play(ctx);
            }
        }
        public async Task convert(VoiceNextConnection vnc, Songs song, CancellationToken token)
        {
            var ffmpeg_inf = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{song.filepath}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var ffmpeg = Process.Start(ffmpeg_inf);
            var ffout = ffmpeg.StandardOutput.BaseStream;
            using (var ms = new MemoryStream())
            {
                await ffout.CopyToAsync(ms);
                ms.Position = 0;
                var buff = new byte[3840];
                var br = 0;
                while ((br = ms.Read(buff, 0, buff.Length)) > 0)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    if(paused == true)
                    {
                        while(paused == true)
                        {

                        }
                    }
                    if (br < buff.Length)
                        for (var i = br; i < buff.Length; i++)
                            buff[i] = 0;

                    await vnc.SendAsync(buff, 20);
                }
            }
        }
        [Command("viewplaylist")]
        [Description("Views current playlist")]
        public async Task viewplaylist(CommandContext ctx)
        {
            int songcount = 1;
            string songlist = null;
            foreach (var song in playlist)
            {
                songlist += $"{songcount}:{song.filename}" + "\n";
                songcount++;
            }
            await ctx.Channel.SendMessageAsync(songlist);

        }
        [Command("select")]
        [Description("selects any song from current playlist and makes a new list containing those songs")]
        public async Task select(CommandContext ctx, [RemainingText]string selection)
        {
            List<Songs> tempplaylist = new List<Songs>();
            List<int> tracks = new List<int>();
            foreach (char selected in selection)
            {
                if (selected == ' ')
                {

                }
                else
                {
                    string trackstring = selected.ToString();
                    int track = Convert.ToInt32(trackstring);
                    tracks.Add(track - 1);
                }
            }
            foreach (int track in tracks)
            {
                tempplaylist.Add(playlist[track]);
            }
            playlist = tempplaylist;
            source.Cancel();
            source = new CancellationTokenSource();
            await ctx.Channel.SendMessageAsync("Heres the new playlist!");
            await viewplaylist(ctx);
            await play(ctx);
        }
        [Command("pause")]
        [Description("pauses music playback")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task pause(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.Message.DeleteAsync();
            var vnc = await NotInGuildVerification(ctx);
            if (vnc.IsPlaying == false)
            {
                await ctx.RespondAsync("No music currently being played");
                return;
            }
            await vnc.SendSpeakingAsync(false);
            paused = true;
            await ctx.Channel.SendMessageAsync("Music has been paused!");    
        }
        [Command("resume")]
        [Description("resumes music playback")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task unpause(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.Message.DeleteAsync();
            var vnc = await NotInGuildVerification(ctx);
            if (vnc.IsPlaying == true)
            {
                await ctx.RespondAsync("Music already playing");
                return;
            }
            if(paused==false)
            {
                await ctx.RespondAsync("Music not paused!");
                return;
            }
            await vnc.SendSpeakingAsync(true);
            paused = false;
            await ctx.Channel.SendMessageAsync("Music has been resumed!");            
        }
        public async Task<VoiceNextConnection> NotInGuildVerification(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
            {
                await ctx.RespondAsync("Voice not enabled or configured");
                return null;
            }
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("Not in guild!");
                return null;
            }
            else
            {
                return vnc;
            }

        }
        [Command("wipe")]
        [Description("wipes the music folder ONLY TO BE USED POST PLAYBACK")]
        [RequirePermissions(Permissions.ManageGuild)]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task wipe (CommandContext ctx)
        {
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            playlist.Clear();
           await ctx.Channel.SendMessageAsync("Music folder cleared!");
        }
        [Command("stop")]
        [Description("stops music playback and wipes playlist and music folder")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task stop(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            source.Cancel();
            source = new CancellationTokenSource();
            await wipe(ctx);
            await ctx.Channel.SendMessageAsync("Playlist cleared");
        }
    }
}


