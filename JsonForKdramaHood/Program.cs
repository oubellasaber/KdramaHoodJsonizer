using System;
using KdramaHoodJsonizer;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        string serieUrl = "https://kdramahood.com/dh/itaewon";
        var result = clsSerieInfoJsonizer.GetSerieInfoAsJsonString(serieUrl);

        if (result.IsSuccess)
        {
            Console.WriteLine("Extracted Series Info:");
            Console.WriteLine(result.Value);
        }
        else
        {
            Console.WriteLine($"Failed to extract series info: {result.Error}");
        }

        var eps = clsEpLinksJsonizer.ExtractSerieEpsInfoAsDictionary(serieUrl);

        if(eps.IsSuccess)
        {
            string jsonContent = JsonConvert.SerializeObject(eps.Value, Formatting.Indented);

            Console.WriteLine(jsonContent);
        }
        else
        {
            Console.WriteLine($"Failed to extract ep info: {eps.Error}");
        }

    }
}
