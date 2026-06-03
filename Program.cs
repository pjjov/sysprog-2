using System.Net;
using SysProg;
using DotNetEnv;

Env.Load();
string? apiKey = Environment.GetEnvironmentVariable("API_KEY");
int cacheSize = int.Parse(Environment.GetEnvironmentVariable("CACHE_SIZE") ?? "10");
int flushPeriod = int.Parse(Environment.GetEnvironmentVariable("FLUSH_PERIOD") ?? "60");

if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Error while loading API key from .env file");
    return;
}

ApiService service = new ApiService(apiKey, cacheSize, flushPeriod);
Server server = new Server("http://localhost:8080/");

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    server.Close();
};

try 
{
    while (server.listener.IsListening) {
        var context = server.Accept();
        server.Execute(context, service);
    }
}
catch (HttpListenerException) { /* Expected on Stop() */ }
catch (ObjectDisposedException) { /* Expected on Close() */ }

service.Close();