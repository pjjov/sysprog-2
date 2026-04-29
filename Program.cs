using System.Net;
using SysProg;

ApiService service = new ApiService();
Server server = new Server("http://localhost:8080/");

Thread thread = new Thread(() => {
    try {
        while (server.listener.IsListening) {
            var context = server.Accept();
            server.Send(context, service.Handle(context.Request));
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