using System.Net;
using Newtonsoft.Json.Linq;
using SysProg.Utils;

namespace SysProg;

class Server {
    public HttpListener listener;

    public Server(string prefix) {
        if (!HttpListener.IsSupported)
            throw new Exception("HttpListener nije podrzan!");

        listener = new HttpListener();
        listener.Prefixes.Add(prefix);
        listener.Start();
    }

    public HttpListenerContext Accept() {
        return listener.GetContext();
    }

    public void Execute(HttpListenerContext context, ApiService service) {
        Task.Run(async () => {
            string? result = null;

            try {
                if (context.Request.HttpMethod != "GET")
                {
                    context.Response.StatusCode = 402;
                    result = "Samo GET metoda je podrzana!";
                }
                else
                {
                    var url = UrlUtils.TransformUrl(context.Request);
                    var body = await service.Query(url);
                    result = body?.ToString();
                }
            }
            catch (Exception e) {
                Console.WriteLine($"Greska prilikom obrade zahteva: {e.Message}");
            }

            return result;
        }).ContinueWith(task => {
            Send(context, task.Result ?? "Greska!");
            context.Response.Close();
        });
    }

    public void Send(HttpListenerContext context, string body) {
        HttpListenerResponse response = context.Response;

        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(body);
        response.ContentLength64 = buffer.Length;

        System.IO.Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
    }

    public void Close() {
        listener.Stop();
    }
}