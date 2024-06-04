# KdramaHoodJsonizer

`KdramaHoodJsonizer` is a set of classes designed to extract and process series and episode information from the website [kdramahood](https://kdramahood.com/). This project utilizes the `HtmlAgilityPack` library for HTML parsing and `Newtonsoft.Json` for JSON serialization.

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
  - [Extracting Episode Information](#extracting-episode-information)
  - [Extracting Series Information](#extracting-series-information)
- [Classes and Methods](#classes-and-methods)
  - [clsEpLinksJsonizer](#clseplinksjsonizer)
  - [clsSerieInfoJsonizer](#clsserieinfojsonizer)
- [Helper Functions](#helper-functions)

## Installation

1. Clone the repository:

    ```sh
    git clone https://github.com/oubellasaber/KdramaHoodJsonizer
    ```

2. Install the required NuGet packages:

    ```sh
    dotnet add package HtmlAgilityPack
    dotnet add package Newtonsoft.Json
    ```

## Usage

### Extracting Episode Information

To extract episode information from a specific episode URL, use the `ExtractEpisodeInfoAsDictionary` method of `clsEpLinksJsonizer`.

```csharp
using KdramaHoodJsonizer;

string episodeUrl = "https://kdramahood.com/nt/itaewon-class-ep-12/";
var result = clsEpLinksJsonizer.ExtractEpisodeInfoAsDictionary(episodeUrl);

if (result.IsSuccess)
{
    var episodeInfo = result.Value;
    string jsonContent = JsonConvert.SerializeObject(result.Value, Formatting.Indented);
    Console.WriteLine(jsonContent);
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}
```

### Extracting Series Information

To extract serie information from a specific serie URL, use the `GetSerieInfoAsJsonDictionary` or `GetSerieInfoAsJsonString` methods of `clsSerieInfoJsonizer`.

```csharp
using KdramaHoodJsonizer;

string seriesUrl = "https://kdramahood.com/dh/some-drama/";
var result = clsSerieInfoJsonizer.GetSerieInfoAsJsonDictionary(seriesUrl);

if (result.IsSuccess)
{
    var seriesInfo = result.Value;
    // Process seriesInfo
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}
```

## Classes and Methods

### clsEpLinksJsonizer

This class is responsible for extracting episode information.

#### Methods

- **ExtractEpisodeInfoAsDictionary**: Extracts episode information as a dictionary.
- **ExtractSerieEpsInfoAsDictionary**: Extracts information for a range of episodes from a series URL.

### clsSerieInfoJsonizer

This class is responsible for extracting series information.

#### Methods

- **GetSerieInfoAsJsonDictionary**: Extracts series information as a dictionary.
- **GetSerieInfoAsJsonString**: Extracts series information as a JSON string.
- **GetLastUploadedEpisode**: Retrieves the last uploaded episode number for a series.

## Helper Functions

The following helper functions are used internally within the classes:

- **ExtractEpStreamingUrls**: Extracts streaming URLs for an episode.
- **GetDramaNameFromSerieUrl**: Extracts the drama name from the series URL.
- **GetEpisodeUrl**: Constructs the episode URL from the drama name and episode number.
- **ExtractEpisodeNumber**: Extracts the episode number from the URL.
- **RemoveUnnecessaryPartsFromUrl**: Cleans up URLs by removing unnecessary parts.
- **ProcessValue**: Processes values by splitting and trimming them.
- **ExtractAdditionalInfo**: Extracts additional information such as total episodes, last uploaded episode, trailer, and image URLs.

## Example

Here is a full example of extracting series information and printing it as a JSON string:

```csharp
using System;
using KdramaHoodJsonizer;

class Program
{
    static void Main()
    {
        string seriesUrl = "https://kdramahood.com/dh/itaewon-class/";
        var result = clsSerieInfoJsonizer.GetSerieInfoAsJsonString(seriesUrl);

        if (result.IsSuccess)
        {
            Console.WriteLine(result.Value);
        }
        else
        {
            Console.WriteLine($"Error: {result.Error}");
        }
    }
}
```
