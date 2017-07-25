using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.Codec;
using Google.Apis.Services;
using Google.Apis.Discovery;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using WrapYoutubeDl;
using System.Collections.Generic;
using NAudio;
using NAudio.Wave;
using MP3Sharp;

namespace DeepSector
{

    public class DJCommands
    {
        List<Songs> playlist = new List<Songs>();
        [Command("join")]
        [Description("joins the users current voice channel")]
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
                await ctx.RespondAsync("already in guild");
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
        public async Task leave(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
            {
                await ctx.RespondAsync("Voice not enabled or configured");
                return;
            }
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("not in guild");
                return;
            }
            vnc.Disconnect();
        }
        [Command("add")]
        [Description("adds a track to the guilds music queue")]
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
                filepath = outputfolder + filename+".mp3"
            };
            playlist.Add(song);
            var downloader = new AudioDownloader(dl, filename, outputfolder);
            downloader.FinishedDownload += finished(searchlistresponse, ctx);
            downloader.Download();
            
        }
        static AudioDownloader.FinishedDownloadEventHandler finished(SearchListResponse searchlistresponse, CommandContext ctx)
        {
            ctx.RespondAsync($"{searchlistresponse.Items[0].Snippet.Title} has been downloaded");
            return null;
        }
        [Command("play")]
        [Description("plays the currently queued songs")]
        public async Task play(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            var vnext = ctx.Client.GetVoiceNextClient();
            if (vnext == null)
            {
                await ctx.RespondAsync("Voice not enabled or configured");
                return;
            }
            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("not in guild");
                return;
            }
            foreach (var song in playlist)
            {
                while (vnc.IsPlaying)
                {
                    await vnc.WaitForPlaybackFinishAsync();
                }
                Exception exc = null;
                await ctx.Message.RespondAsync($"Playing `{song.filename}`");
                await vnc.SendSpeakingAsync(true);

                try
                {
                    await convert(vnc, song);
                }
                catch (Exception ex)
                {
                    exc = ex;
                }
                finally
                {
                    await vnc.SendSpeakingAsync(false);
                    File.Delete(song.filepath);
                    playlist.Remove(song);

                }
                if (exc != null)
                {
                    await ctx.RespondAsync($"An exception occured during playback: `{exc.GetType()}: {exc.Message}`");
                }

            }
        }
        public async Task convert(VoiceNextConnection vnc, Songs song)
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
                    if (br < buff.Length)
                        for (var i = br; i < buff.Length; i++)
                            buff[i] = 0;

                    await vnc.SendAsync(buff, 20);
                }
            }

        }
    }
}


