using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace KdramaHoodJsonizer
{
    public class clsSerieInfoJsonizer
    {
        static object ProcessValue(string value)
        {
            // Split the value by comma and trim each part
            var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(part => part.Trim())
                             .ToList();
            // Return list if there are multiple parts, otherwise return single string
            return parts.Count > 1 ? (object)parts : parts.FirstOrDefault();
        }

        public static clsResult<Dictionary<string, object>> GetSerieInfoAsJsonDictionary(string serieUrl)
        {
            if (!serieUrl.StartsWith("https://kdramahood.com/dh", StringComparison.OrdinalIgnoreCase))
                return clsResult<Dictionary<string, object>>.Failure("Invalid URL");

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc;
            try
            {
                doc = web.Load(serieUrl);
            }
            catch (Exception ex)
            {
                return clsResult<Dictionary<string, object>>.Failure(ex.Message);
            }

            if (web.StatusCode != HttpStatusCode.OK)
            {
                string errorMessage = $"Failed to fetch serie information. HTTP Status Code: {web.StatusCode}";
                return clsResult<Dictionary<string, object>>.Failure(errorMessage);
            }

            Dictionary<string, object> data = new Dictionary<string, object>();

            // Extract the English title
            var titleNode = doc.DocumentNode.SelectSingleNode(@"//*[@id=""seasons""]/div/div[1]/span[2]");
            if (titleNode != null)
            {
                data["English Name"] = titleNode.InnerText.Trim();
            }

            // Find the info div
            HtmlNode infoDiv = doc.GetElementbyId("info");

            if (infoDiv != null)
            {
                foreach (HtmlNode node in infoDiv.ChildNodes)
                {
                    if (node.GetAttributeValue("class", string.Empty) == "metadatac")
                    {
                        // Extract key (label) from the <b> tag
                        HtmlNode keyNode = node.SelectSingleNode("./b");
                        if (keyNode != null)
                        {
                            string key = keyNode.InnerText.Trim();
                            HtmlNodeCollection spanNodes = node.SelectNodes("./span");
                            HtmlNodeCollection anchorNodes = node.SelectNodes("./a");

                            if (spanNodes != null && spanNodes.Count > 1)
                            {
                                data[key] = spanNodes.Select(x => x.InnerText.Trim()).ToList();
                            }
                            else if (anchorNodes != null && anchorNodes.Count > 1)
                            {
                                data[key] = anchorNodes.Select(x => x.InnerText.Trim()).ToList();
                            }
                            else if (spanNodes != null && spanNodes.Count == 1)
                            {
                                data[key] = ProcessValue(spanNodes.First().InnerText.Trim());
                            }
                            else if (anchorNodes != null && anchorNodes.Count == 1)
                            {
                                data[key] = ProcessValue(anchorNodes.First().InnerText.Trim());
                            }
                        }
                    }
                    else if (node.GetAttributeValue("class", string.Empty) == "contenidotv")
                    {
                        var descNode = node.SelectSingleNode("./div/p");
                        if (descNode != null && !descNode.HasAttributes)
                        {
                            data["description"] = descNode.InnerText.Trim();
                        }
                    }
                }
            }

            // Extract additional information using helper methods
            ExtractAdditionalInfo(doc, data);

            return clsResult<Dictionary<string, object>>.Success(data);
        }

        static void ExtractAdditionalInfo(HtmlDocument doc, Dictionary<string, object> data)
        {
            void TryAddData(string key, string xPath, Func<HtmlNode, string> extractor)
            {
                var node = doc.DocumentNode.SelectSingleNode(xPath);
                if (node != null)
                {
                    string value = extractor(node);
                    if (!string.IsNullOrEmpty(value))
                    {
                        data[key] = value;
                    }
                }
            }

            TryAddData("Total Episodes", @"//*[@id=""fixar""]/div[3]/span[2]/i", node => node.InnerText.Trim());
            TryAddData("Last Uploaded Episode", @"//*[@id=""seasons""]/div/div[2]/ul/li[1]/div[1]", node => node.InnerText.Trim());

            TryAddData("Trailer", @"//*[@id=""trailer""]/div[1]/iframe", node =>
            {
                string trailerUrl = node.GetAttributeValue("src", string.Empty).Trim();
                return !string.IsNullOrEmpty(trailerUrl) ? "https:" + trailerUrl : null;
            });

            TryAddData("Image", @"//*[@id=""fixar""]/div[1]/img", node => node.GetAttributeValue("src", string.Empty).Trim());
        }

        public static clsResult<string> GetSerieInfoAsJsonString(string serieUrl)
        {
            var serieInfo = clsSerieInfoJsonizer.GetSerieInfoAsJsonDictionary(serieUrl);

            if (serieInfo.IsSuccess)
            {
                string jsonString= JsonConvert.SerializeObject(serieInfo.Value, Formatting.Indented);

                return clsResult<string>.Success(jsonString);
            }

            return clsResult<string>.Failure(serieInfo.Error);
        }

        public static clsResult<int> GetLastUploadedEpisode(string serieUrl)
        {
            int lastUploadedEp = 0;
            if (!serieUrl.StartsWith("https://kdramahood.com/dh", StringComparison.OrdinalIgnoreCase))
                return clsResult<int>.Failure("Invalid URL");

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc;
            try
            {
                doc = web.Load(serieUrl);
            }
            catch (Exception ex)
            {
                return clsResult<int>.Failure(ex.Message);
            }

            if (web.StatusCode != HttpStatusCode.OK)
            {
                string errorMessage = $"Failed to fetch serie information. HTTP Status Code: {web.StatusCode}";
                return clsResult<int>.Failure(errorMessage);
            }

            var lastUploadedEpNode = doc.DocumentNode.SelectSingleNode(@"//*[@id=""seasons""]/div/div[2]/ul/li[1]/div[1]");
            if (lastUploadedEpNode != null)
            {
                if(!int.TryParse(lastUploadedEpNode.InnerText.ToString().Trim(), out lastUploadedEp)) {
                    return clsResult<int>.Failure("Failed to parse episode number");
                }
            }

            return clsResult<int>.Success(lastUploadedEp);
        }
    }
}