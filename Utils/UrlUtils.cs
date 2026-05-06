using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SysProg.Utils;

public class UrlUtils {
    public static void AppendFilter(ref StringBuilder sb, string? key, string? value)
    {
        if (value != null)
        {
            if (key != null)
            {
                sb.Append('+');
                sb.Append(key);
                sb.Append(':');
            }
            sb.Append(value);
        }
    }

    public static string BuildUrl(string baseUrl, string path, string key)
    {
        // return base + path + "&key=" + key;
        // ako ima ? u sredinu dodaje &, ali ako nema ? onda dodaje ?
        string separator = path.Contains("?") ? "&" : "?";
        path += $"{separator}key={key}";
        return baseUrl + path;
    }

    public static string QuerySummary(string url)
    {
        var uri = new Uri(url);
        string result = uri.Query.TrimStart('?').Replace("&", "\n\t").Replace("=", ": ");
        return result;
    }
    public static string NormalizeUrl(HttpListenerRequest request)
    {
        var query = request.QueryString;

        var sortedKeys = query.AllKeys
            .Where(k => k != null)
            .OrderBy(k => k, StringComparer.Ordinal);

        var sb = new StringBuilder();

        foreach (var key in sortedKeys)
        {
            var values = query.GetValues(key);

            foreach (var value in values!)
            {
                if (sb.Length > 0)
                    sb.Append("&");

                sb.Append(Uri.EscapeDataString(key!));
                sb.Append("=");
                sb.Append(Uri.EscapeDataString(value));
            }
        }

        var baseUrl = request.Url!.GetLeftPart(UriPartial.Path);

        return sb.Length > 0
            ? $"{baseUrl}?{sb}"
            : baseUrl;
    }
}