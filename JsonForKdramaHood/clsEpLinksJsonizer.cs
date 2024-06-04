using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using JsonForKdramaHood;
using Newtonsoft.Json;

namespace KdramaHoodJsonizer
{
    public static class clsEpLinksJsonizer
    {
        public static clsResult<Dictionary<string, object>> ExtractEpisodeInfoAsDictionary(string epUrl)
        {
            if (!epUrl.StartsWith("https://kdramahood.com/nt/", StringComparison.OrdinalIgnoreCase))
                return clsResult<Dictionary<string, object>>.Failure("Invalid URL");

            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc;
            try
            {
                htmlDoc = web.Load(epUrl);
            }
            catch (Exception ex)
            {
                return clsResult<Dictionary<string, object>>.Failure(ex.Message);
            }

            if (web.StatusCode != HttpStatusCode.OK)
            {
                string errorMessage = $"Failed to fetch episode information. HTTP Status Code: {web.StatusCode}";
                return clsResult<Dictionary<string, object>>.Failure(errorMessage);
            }

            Dictionary<string, object> episodeInfo = new Dictionary<string, object>
            {
                ["Ep Number"] = ExtractEpisodeNumber(epUrl),
                ["Links"] = ExtractEpStreamingUrls(htmlDoc.DocumentNode)
            };

            HtmlNode subsAnchorNode = htmlDoc.DocumentNode.SelectSingleNode($@"//*[@id=""links""]/div/div/li[{htmlDoc.DocumentNode.SelectNodes(@"//*[@id=""links""]/div/div/li").Count}]/a");

            if (subsAnchorNode != null)
            {
                episodeInfo["SubtitlesLink"] = subsAnchorNode.GetAttributeValue("href", "N\\A");
            }

            if (((List<string>)episodeInfo["Links"]).Count > 0)
                return clsResult<Dictionary<string, object>>.Success(episodeInfo);

            return clsResult<Dictionary<string, object>>.Failure("No episode links found");
        }

        private static List<string> ExtractEpStreamingUrls(HtmlNode docNode)
        {
            HashSet<string> uniqueUrls = new HashSet<string>();

            var scriptNode = docNode.SelectSingleNode($@"//*[@id=""streaming""]/div/div[{docNode.SelectNodes(@"//*[@id=""streaming""]/div/div").Count}]/script");

            if (scriptNode != null)
            {
                string scriptContent = scriptNode.InnerHtml;
                string pattern = @"ifr_target\.src\s*=\s*'([^']+)'";
                MatchCollection matches = Regex.Matches(scriptContent, pattern);

                uniqueUrls.UnionWith(matches.Cast<Match>().Select(m =>
                {
                    string url = m.Groups[1].Value;
                    if (!url.StartsWith("https:"))
                    {
                        url = "https:" + url;
                    }

                    url = RemoveUnnecessaryPartsFromUrl(url);
                    return url;
                }));
            }

            HtmlNode fbcdnAnchorNode = docNode.SelectSingleNode($@"//*[@id=""links""]/div/div/li[{1}]/a");

            if (fbcdnAnchorNode != null && fbcdnAnchorNode.InnerText == "Link1")
            {
                uniqueUrls.Add(fbcdnAnchorNode.GetAttributeValue("href", "N\\A"));
            }

            return uniqueUrls.ToList();
        }

        public static clsResult<Dictionary<string, object>> ExtractSerieEpsInfoAsDictionary(string serieUrl, int from = 1, int to = 0)
        {
            if (!serieUrl.StartsWith("https://kdramahood.com/dh/", StringComparison.OrdinalIgnoreCase))
                return clsResult<Dictionary<string, object>>.Failure("Invalid URL");

            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc;
            try
            {
                htmlDoc = web.Load(serieUrl);
                serieUrl = web.ResponseUri.OriginalString;
            }
            catch (Exception ex)
            {
                return clsResult<Dictionary<string, object>>.Failure(ex.Message);
            }

            int lastUploadedEp = 0;

            clsResult<int> lastUploadedEpResult = clsSerieInfoJsonizer.GetLastUploadedEpisode(serieUrl);
            if (lastUploadedEpResult.IsSuccess)
            {
                lastUploadedEp = lastUploadedEpResult.Value;
            }
            else
            {
                return clsResult<Dictionary<string, object>>.Failure("Failed to retrieve last uploaded episode number");
            }

            if (to == 0)
            {
                to = lastUploadedEp;
            }

            from = Math.Max(1, from);
            to = Math.Min(lastUploadedEp, to);

            if (from > to)
            {
                return clsResult<Dictionary<string, object>>.Failure($"Episodes range is out of bound, consider passing just the serie URL or this range ({1}, {lastUploadedEp})");
            }

            Dictionary<string, object> epsInfo = new Dictionary<string, object>();
            string dramaName = GetDramaNameFromSerieUrl(serieUrl);
            List<Dictionary<string, object>> episodes = new List<Dictionary<string, object>>();

            for (int i = from; i <= to; i++)
            {
                clsResult<Dictionary<string, object>> result = ExtractEpisodeInfoAsDictionary(GetEpisodeUrl(dramaName, i));
                if (result.IsSuccess)
                {
                    episodes.Add(result.Value);
                }
                else
                {
                    return clsResult<Dictionary<string, object>>.Failure($"Failed to retrieve information for episode {i}: {result.Error}");
                }
            }

            epsInfo["Episodes"] = episodes;
            return clsResult<Dictionary<string, object>>.Success(epsInfo);
        }


        // Helper functions
        private static string GetDramaNameFromSerieUrl(string url)
        {
            Uri uri = new Uri(url);
            string[] pathSegments = uri.AbsolutePath.Split('/');
            return pathSegments[pathSegments.Length - 2];
        }

        private static string GetEpisodeUrl(string dramaName, int episodeNumber)
        {
            return $"https://kdramahood.com/nt/{dramaName}-ep-{episodeNumber}/";
        }

        private static int ExtractEpisodeNumber(string url)
        {
            string pattern = @"-ep-(\d+)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(url);
            if (match.Success)
                return int.Parse(match.Groups[1].Value);
            return -1;
        }

        private static string RemoveUnnecessaryPartsFromUrl(string url)
        {
            if (url.Contains("&title"))
            {
                string[] ampParts = url.Split('&');
                url = ampParts[0];
            }
            else if (url.Contains("?caption"))
            {
                string[] parts = url.Split('?');
                url = parts[0];
            }

            return url;
        }
    }
}
