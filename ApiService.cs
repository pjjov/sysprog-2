using System.Net;
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

    public JObject Query(HttpListenerRequest req) {
        string url = UrlUtils.NormalizeUrl(req);
        JObject? result = cache.Find(url);
        if (result != null)
            return result;

        result = Fetch("/volumes?q=inauthor:tolkien");

        cache.Insert(url, result);
        return result;
    }
}