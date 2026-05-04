using System.Net;
using SysProg;
using DotNetEnv;

Env.Load();
string? apiKey = Environment.GetEnvironmentVariable("API_KEY");
if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Error while loading API key from .env file");
    return;
}

int cacheSize = int.Parse(Environment.GetEnvironmentVariable("CACHE_SIZE") ?? "10");
ApiService service = new ApiService(apiKey, cacheSize);

Server server = new Server("http://localhost:8080/");

Thread thread = new Thread(() => {
    try {
        while (server.listener.IsListening) {
            var context = server.Accept();
            server.Execute(context, service);
        }
    }
    catch (HttpListenerException) { /* Expected on Stop() */ }
    catch (ObjectDisposedException) { /* Expected on Close() */ }
    finally
    {
        Console.WriteLine("Server stopiran.");
    }
});

thread.Start();

Console.ReadLine();

server.Close();

service.SaveCache();