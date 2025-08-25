/*
Se cere o aplicatie de tip console care primeste 2 parametrii ca input de la user:
- site valid (e.g.: "https://www.dailymail.co.uk/")
- tag type ("video" / "audio" / "source")
Aplicatia trebuie sa faca site crawling in asa fel incat sa returneze numarul de aparatii al html tag-uri de tipul 
indicat prin user input 'tag type'. Procesul de crawling se va realiza pe un thread separat fata de cel main. 
Se urmareste utilizarea unui parameterized thread.  
*/

using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class CrawlParameters
{
    public string Url { get; }
    public string Tag { get; }

    public CrawlParameters(string url, string tag)
    {
        Url = url;
        Tag = tag;
    }
}

public class WebCrawler
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Please enter a valid website URL (e.g., https://www.dailymail.co.uk/):");
        string siteUrl = Console.ReadLine() ?? string.Empty;
        if (!Uri.TryCreate(siteUrl, UriKind.Absolute, out _))
        {
            Console.WriteLine("Invalid URL provided. Exiting.");
            return;
        }

        Console.WriteLine("Enter the HTML tag to count (video/audio/source):");
        string tag = (Console.ReadLine() ?? string.Empty).ToLower();

        if (tag != "video" && tag != "audio" && tag != "source")
        {
            Console.WriteLine("Invalid tag type. Exiting.");
            return;
        }
        
        CrawlParameters parameters = new CrawlParameters(siteUrl, tag);
        //Thread workerThread = new Thread(new ParameterizedThreadStart(CrawlWebsite));
        //workerThread.Start(parameters);
        //workerThread.Join();
        await Task.Run(() => CrawlWebsiteAsync(parameters));
        Console.WriteLine("Main thread has finished.");
    }
    
    public static async Task CrawlWebsiteAsync(object? data)
    {
        if (data is not CrawlParameters parameters)
        {
            Console.WriteLine("Worker thread received invalid parameters. Exiting.");
            return;
        }

        Console.WriteLine($"\nWorker thread started for URL: {parameters.Url} and tag: <{parameters.Tag}>");
        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                string htmlContent = await client.GetStringAsync(parameters.Url);
                string pattern = $"<{parameters.Tag}[^>]*>";
                
                MatchCollection matches = Regex.Matches(htmlContent, pattern, RegexOptions.IgnoreCase);
                Console.WriteLine($"Found {matches.Count} occurrences of the <{parameters.Tag}> tag.");
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"\nAn error occurred while fetching the website: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"\nAn unexpected error occurred in the worker thread: {e.Message}");
        }
    }
}