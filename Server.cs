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