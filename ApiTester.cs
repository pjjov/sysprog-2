using System.Diagnostics;
using System.Net;

namespace SysProg;

class ApiTester
{
    private const string baseUrl = "http://localhost:8080";
    private readonly HttpClient client;
    private Stream stream;
    private StreamWriter writer;
    private Stopwatch stopwatch;

    public ApiTester(string path)
    {
        client = new HttpClient();
        stream = new FileStream(path, FileMode.Create);
        writer = new StreamWriter(stream);
        stopwatch = new Stopwatch();
    }

    public void TestSingle(string url)
    {
        var started = Stopwatch.GetTimestamp();

        stopwatch.Restart();
        var response = client.GetAsync(baseUrl + url).Result;
        stopwatch.Stop();

        writer.WriteLine($"{response.StatusCode},{started},{stopwatch.Elapsed}");
    }

    public void TestMany(int iterations = 100)
    {
        Random r = new Random();

        string[] urls = [
            "/?author=tolkien",
            "/?author=kafka",
            "/?author=dostoevsky",
            "/?subject=war",
            "/?subject=fantasy",
        ];

        Console.WriteLine($"Testiranje {iterations} iteracija...");

        for (int i = 0; i < iterations; i++)
        {
            TestSingle(urls[r.Next(urls.Length)]);
        }

        Console.WriteLine("Gotovo testiranje!");
    }

    public void Close()
    {
        writer.Close();
        stream.Close();
    }
}