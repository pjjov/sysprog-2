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

    private void Handle(HttpListenerContext context, ApiService service)
    {
        if (context.Request.HttpMethod == "GET")
        {
            Send(context, service.Query(context.Request));
        }
        else
        {
            context.Response.StatusCode = 402;
            Send(context, "Samo GET metoda je podrzana!");
        }
    }

    public void Execute(HttpListenerContext context, ApiService service) {

        ThreadPool.QueueUserWorkItem(state => {
            try {
                Handle(context, service);
            }
            catch (Exception e) {
                Console.WriteLine($"Greska prilikom rada sa ThreadPool-om: {e.Message}");
            }
            finally {
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
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

    public void Send(HttpListenerContext context, JObject result)
    {
        Send(context, result.ToString());
    }

    public void Close() {
        listener.Stop();
    }
}