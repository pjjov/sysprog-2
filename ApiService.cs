using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        // Doraditi generisanje krajnjeg urla.
        HttpResponseMessage response = client.GetAsync(baseUrl + url + "&key=" + apiKey).Result;
        response.EnsureSuccessStatusCode();
        string responseBody = response.Content.ReadAsStringAsync().Result;
        return JObject.Parse(responseBody);
    }
    
    private string NormalizeUrl(string url)
    {
        return url;
    }

    public JObject Handle(HttpListenerRequest req) {
        string url = NormalizeUrl(req.Url!.ToString());
        JObject? result = cache.Find(url);
        if (result != null)
            return result;

        result = Fetch("/volumes?q=inauthor:tolkien");

        cache.Insert(url, result);
        return result;
    }
}