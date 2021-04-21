using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TeamRankingsWebScrape
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var page = CallUrl("https://www.teamrankings.com/nba/trends/ats_trends/").Result;
            var trends = GetAtsCoverTrends(page);
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
