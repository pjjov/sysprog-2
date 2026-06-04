

using Newtonsoft.Json.Linq;
using Superpower;
using Superpower.Model;

namespace SysProg.Utils;

public class VolumeUtils
{
    private static JObject? ParseVolumeInfo(JToken volume)
    {
        var volumeInfo = volume["volumeInfo"];
        if(volumeInfo == null)
            return null;

        return new JObject
        {
            ["title"] = volumeInfo["title"]?.ToString() ?? "",
            ["authors"] = volumeInfo["authors"] ?? new JArray(),
            ["description"] = volumeInfo["description"]?.ToString() ?? "",
        };
    }

    public static JObject ParseVolume(string response)
    {
        var res = new JArray();
        var items = JObject.Parse(response)["items"] as JArray;

        if(items == null)
            throw new Exception("Greska pri obradi rezultata!");

        foreach(var item in items)
        {
            var book = ParseVolumeInfo(item);
            if (book != null)
                res.Add(book);
        }

        if (res.Count == 0)
            throw new Exception("Nije pronadjena nijedna knjiga sa datim filterima!");
        
        return new JObject { ["books"] = res };
    }
}