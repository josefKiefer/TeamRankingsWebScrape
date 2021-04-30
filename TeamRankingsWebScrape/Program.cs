using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TeamRankingsWebScrape
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            // get todays games

            //var todaysGames = CallUrl("https://www.teamrankings.com/nba/").Result;
            //GetTodaysGames(todaysGames);

            //var page = CallUrl("https://www.teamrankings.com/nba/trends/ats_trends/").Result;
            //var trends = GetAtsCoverTrends(page);
            var potdThread = CallUrl("https://www.reddit.com/r/sportsbook/?f=flair_name%3A%22POTD%22").Result;
            var topThreadUrl = GetTopPotdPost(potdThread);
            var topThreadHtml = CallUrl(topThreadUrl).Result;
            GetCusPhenom(topThreadHtml);
        }

        private static void GetCusPhenom(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var cusePhenomPick = htmlDoc.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("data-author", "").Contains("cusephenom"))
                ;

            var text = cusePhenomPick.FirstOrDefault().InnerText;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("email", ""),
                EnableSsl = true,
            };
            smtpClient.UseDefaultCredentials = false;

            smtpClient.Send("email", "email", "subject", text);
        }

        private static string GetTopPotdPost(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var thread = htmlDoc.DocumentNode.Descendants("a")
                .Where(node => node.GetAttributeValue("class", "").Contains("SQnoC3ObvgnGjWt90zD9Z _2INHSNB8V5eaWp4P0rY_mE"))
                .FirstOrDefault().GetAttributeValue("href", "");

            return "https://www.reddit.com/" + thread;
        }

        private static void GetTodaysGames(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var allTeams = htmlDoc.DocumentNode.Descendants("td")
                .Where(node => !node.GetAttributeValue("class", "").Contains("text"))
                .Where(node => node.FirstChild.Name == "a")
                .Select(node => node.InnerText)
                .ToList();

            foreach(var team in allTeams)
            {
                team.Replace("(", "").Replace(")", "");
            }
        }
        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }

        private static Dictionary<int, AtsCoverTrend> GetAtsCoverTrends(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var allTds = htmlDoc.DocumentNode.Descendants("td").Select(child => child.FirstChild.InnerHtml).ToList();

            var coverTrends = new Dictionary<int, AtsCoverTrend>();
            int index = 1;
            for (int i = 0; i < allTds.Count; i++)
            {
                var currentAtsTrend = new AtsCoverTrend();
                if (i % 5 == 0)
                {
                    currentAtsTrend.Team = allTds[i];
                    currentAtsTrend.CoverPercentage = allTds[i + 2];
                    coverTrends.Add(index, currentAtsTrend);
                    index++;
                }
            }

            return coverTrends;
        }
    }
}
