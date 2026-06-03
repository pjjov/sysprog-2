using System.Net;
using Newtonsoft.Json.Linq;

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

    private string Handle(HttpListenerContext context, ApiService service)
    {
        if (context.Request.HttpMethod == "GET")
        {
            context.Response.StatusCode = 402;
            return "Samo GET metoda je podrzana!";
        }

        return service.Query(context.Request).ToString();
    }

    public void Execute(HttpListenerContext context, ApiService service) {
        Task.Run(() => {
            string? result = null;

            try {
                result = Handle(context, service);
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