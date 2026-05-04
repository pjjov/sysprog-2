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
    public ApiService(string apiKey, int cacheSize)
    {
        this.apiKey = apiKey;
        this.client = new HttpClient();
        this.cache = new BookCache(cacheSize);
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

        result = Handle(req);

        cache.Insert(url, result);
        return result;
    }

    public void SaveCache()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        string folderPath = Path.Combine(projectRoot, "files");
        Directory.CreateDirectory(folderPath);

        List<JObject> data = cache.CachedData();
        List<string> queries = cache.CachedQueries();
        if(data != null)
        {
            FileUtil.WriteResults(folderPath, data, queries);
        }
    }
}