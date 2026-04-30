

using Newtonsoft.Json.Linq;
using Superpower;
using Superpower.Model;

namespace SysProg.Utils;

public class VolumeUtils
{
    public static JArray ParseVolume(JObject data)
    {
        var res = new JArray();
        var items = data["items"] as JArray;
        if(items == null)
        {
            return res;
        }

        foreach(var item in items)
        {
            var volumeInfo = item["volumeInfo"];
            if(volumeInfo == null)
                continue;

            var filteredBook = new JObject
            {
                ["title"] = volumeInfo["title"]?.ToString() ?? "",
                ["authors"] = volumeInfo["authors"] ?? new JArray(),
                ["description"] = volumeInfo["description"]?.ToString() ?? "",   
            };

            res.Add(filteredBook);
        }
        
        return res;
    }
}