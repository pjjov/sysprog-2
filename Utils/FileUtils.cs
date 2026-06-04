
using System.Net;
using Newtonsoft.Json.Linq;

namespace SysProg.Utils;

public class FileUtil
{
    public static string GetOutputDirectory()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
        string folderPath = Path.Combine(projectRoot, "files");
        Directory.CreateDirectory(folderPath);
        return folderPath;
    }

    public static void WriteResults(List<KeyValuePair<string, JObject>> snapshot)
    {
        string basePath = GetOutputDirectory();
        int i = 0;

        foreach(var pair in snapshot)
        {
            var books = (JArray?)(pair.Value["books"]);
            if (books == null)
                continue;

            string filePath = Path.Combine(basePath, $"cachedData_{i+1}.txt");

            using(StreamWriter sw = File.CreateText(filePath))
            {
                string query = $"Query: \n{UrlUtils.QuerySummary(pair.Key)}\n\n";
                sw.WriteLine(query);
                
                foreach(JObject book in books)
                {
                    string title = (book["title"] ?? "Nema naslova").ToString();
                    string authors = string.Join(", ", book["authors"]?.ToObject<List<string>>() ?? new List<string>());
                    string description = (book["description"] ?? "Nema opisa").ToString();

                    string content = $"Title: {title}\n" +
                              $"Authors: {authors}\n" +
                              $"Description: {description}\n";

                    sw.WriteLine(content);
                }
            }

            i++;
        }
    }
}