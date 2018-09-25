using CodeHollow.FeedReader;
using Discord;
using Discord.Commands;
using FoxBot.App.Modules.System.PreconditionAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FoxBot.App.Modules.System.Commands
{
    public class FunCommands : ModuleBase
    {
        [Command("laugh")]
        [RequireCommandChannel]
        public Task Laugh()
        {
            string fennekoLaugh = "https://www.luzfaltex.com/images/Fenneko_Laugh.gif";

            HttpWebRequest httpWebRequest = WebRequest.Create(fennekoLaugh) as HttpWebRequest;
            HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;

            var responseStream = httpWebResponse.GetResponseStream();

            return Context.Channel.SendFileAsync(responseStream, "Fenneko_Laugh.gif");
        }

        [Command("killme")]
        [RequireCommandChannel]
        public Task Killme() => ReplyAsync("Later.");

        [Command("xkcd")]
        [RequireCommandChannel]
        public async Task GetRandomXKCD()
        {
            int latestComicID = 5;
            var feed = await FeedReader.ReadAsync("https://xkcd.com/rss.xml");

            var firstItem = feed.Items.FirstOrDefault();

            if (firstItem != null)
            {
                if (!int.TryParse(firstItem.Link.Split('/', StringSplitOptions.RemoveEmptyEntries).Last(), out latestComicID))
                {
                    latestComicID = 2034;
                }
            }
            
            Random random = new Random();
            await GetXKCD(random.Next(1, latestComicID));
        }

        [Command("xkcd")]
        [RequireCommandChannel]
        public Task GetXKCD(int id) => ReplyAsync($"https://xkcd.com/{id}/");

        [Command("shrek")]
        [RequireCommandChannel]
        public async Task ShrekMeDaddy()
        {
            string[] videos = new string[]
            {
                "https://www.youtube.com/watch?v=g7_VlmEamUQ",
                "https://www.youtube.com/watch?v=-yfOsVrJLGs"
            };

            Random random = new Random();
            int videoId = random.Next(0, videos.Length - 1);
            await ReplyAsync(videos[videoId]);
        }
    }
}
