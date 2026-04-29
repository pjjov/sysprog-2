using System.Net;

namespace SysProg;

class ApiService {
    public ApiService() {}
    
    public string Handle(HttpListenerRequest req) {
        return "<HTML><BODY> Hello world!</BODY></HTML>";
    }
}