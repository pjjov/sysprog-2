using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SysProg.Utils;

namespace SysProg;

class ApiService {
    private const string baseUrl = "https://www.googleapis.com/books/v1";
    private BookCache cache;
    private readonly string apiKey;
    private readonly HttpClient client;
    private readonly CancellationTokenSource timer;
    public ApiService(string apiKey, int cacheSize = 10, int flushPeriod = 60)
    {
        this.apiKey = apiKey;
        client = new HttpClient();
        cache = new BookCache(cacheSize);
        timer = new CancellationTokenSource();
        Task.Run(async () => await this.FlushCacheLoop(timer.Token, flushPeriod));
    }

    public JObject Fetch(string url)
    {   
        HttpResponseMessage response = client.GetAsync(UrlUtils.BuildUrl(baseUrl, url, apiKey)).Result;
        response.EnsureSuccessStatusCode();
        string responseBody = response.Content.ReadAsStringAsync().Result;
        return JObject.Parse(responseBody);
    }

    private JObject Handle(HttpListenerRequest req) {
        var query = req.QueryString;

        if (query.Count == 0)
            return Fetch("/volumes");

        var sb = new StringBuilder("/volumes?q=");

        UrlUtils.AppendFilter(ref sb, null, query.Get("search"));
        UrlUtils.AppendFilter(ref sb, "inauthor", query.Get("author"));
        UrlUtils.AppendFilter(ref sb, "inpublisher", query.Get("publisher"));
        UrlUtils.AppendFilter(ref sb, "subject", query.Get("subject"));

        var result = Fetch(sb.ToString());
        JArray books = VolumeUtils.ParseVolume(result);
        
        if (books.Count == 0)
            throw new Exception("Nije pronadjena nijedna knjiga sa datim filterima!");

        return new JObject {
            ["books"] = books,
        };
    }

    public JObject Query(HttpListenerRequest req) {
        string url = UrlUtils.NormalizeUrl(req);
        JObject? result = cache.Find(url);
        if (result != null)
            return result;

        try
        {
            result = Handle(req);
            cache.Insert(url, result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            cache.Abort(url);
        }

        return result ?? new JObject{{ "status", "Server nije trenutno dostupan" }};
    }

    private async Task FlushCacheLoop(CancellationToken token, int flushPeriod)
    {
        PeriodicTimer timer = new (TimeSpan.FromSeconds(flushPeriod));

        try
        {
            while (await timer.WaitForNextTickAsync(token))
            {
                cache.Flush();
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Periodicno brisanje kesa!.");
        }
        finally
        {
            timer.Dispose();
        }
    }

    private void SaveCache()
    {
        var stats = cache.Statistics();
        var percentage = (float)stats.hits / (float)(stats.hits + stats.misses) * 100.0;
        Console.WriteLine($"Cache hits/misses (%missed): {stats.hits}/{stats.misses} ({percentage}%)");

        string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        string folderPath = Path.Combine(projectRoot, "files");
        Directory.CreateDirectory(folderPath);
        FileUtil.WriteResults(folderPath, cache.Snapshot());
    }

    public void Close()
    {
        SaveCache();
        timer.Cancel();
    }
}