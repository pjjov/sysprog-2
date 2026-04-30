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
    private string buildUrl(string baseUrl, string url)
    {
        // return baseUrl + url + "&key=" + apiKey;
        // ako ima ? u sredinu dodaje &, ali ako nema ? onda dodaje ?, ako ima na kraju onda ne dodaje nista, dodaje se na kraju pre kljuca
        string separator = url.Contains("?") ? "&" : "?";
        url += $"{separator}key={apiKey}";
        return baseUrl + url;
    }

    public JObject Fetch(string url)
    {   
        HttpResponseMessage response = client.GetAsync(buildUrl(baseUrl, url)).Result;
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