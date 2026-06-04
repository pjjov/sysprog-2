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

    private async Task<JObject> Fetch(string url)
    {   
        HttpResponseMessage response = await client.GetAsync(UrlUtils.BuildUrl(baseUrl, url, apiKey));
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        return VolumeUtils.ParseVolume(responseBody);
    }

    public async Task<JObject?> Query(string url) {
        JObject? result = cache.Find(url);
        if (result != null)
            return result;

        try
        {
            result = await Fetch(url);
            cache.Insert(url, result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            cache.Abort(url);
        }

        return result;
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

    public void PrintStatistics()
    {
        var stats = cache.Statistics();
        var percentage = (float)stats.hits / (float)(stats.hits + stats.misses) * 100.0;
        Console.WriteLine($"Cache hits/misses (%missed): {stats.hits}/{stats.misses} ({percentage}%)");
    }

    private void SaveCache()
    {
        FileUtil.WriteResults(cache.Snapshot());
    }

    public void Close()
    {
        PrintStatistics();
        SaveCache();
        timer.Cancel();
    }
}