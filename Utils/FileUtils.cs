
using System.Net;
using Newtonsoft.Json.Linq;

namespace SysProg.Utils;

public class FileUtil
{

    public static void WriteResults(string basePath, List<JObject> data, List<string> queries)
    {
        foreach(var bookLists in data)
        {
            int i = 0;
            var books = (JArray)bookLists["books"];

            string filePath = Path.Combine(basePath, $"cachedData_{i+1}.txt");

            using(StreamWriter sw = File.CreateText(filePath))
            {
                string query = $"Query: \n{UrlUtils.GetUrlQuery(queries[i])}\n\n";
                sw.WriteLine(query);
                
                foreach(JObject book in books)
                {
                    string title = book["title"].ToString();
                    string authors = string.Join(", ", book["authors"]?.ToObject<List<string>>() ?? new List<string>());
                    string description = book["description"].ToString();

                    string content = $"Title: {title}\n" +
                              $"Authors: {authors}\n" +
                              $"Description: {description}\n";

                    sw.WriteLine(content);
                }
            }
            i = i + 1;
        }
    }
}