using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DailyWiki.Scripts
{
    public class WikipediaAPI
    {
        public static string ParseWikipediaMarkup(string source, bool stopAtHeader, bool includeRawText)
        {
            string parsed = "";
            //Extracting the paragraphs
            int index = source.IndexOf("<");
            if (index > 0)
                parsed = source.Substring(0, index);
            else if (index < 0)
                parsed = source;
            while (index >= 0 && index < source.Length && source.Substring(index).Contains("<"))
            {
                string tag = source.Substring(index + 1, source.IndexOf(">", index) - index - 1);
                string tagName = tag.Split(' ')[0];
                if (stopAtHeader && tagName.Length == 2 && tagName.StartsWith("h"))
                    break;
                if (tag.Equals("p") || tag.Equals("i") || tag.Equals("b") || tagName.Equals("a"))
                {
                    int tagEnd = source.IndexOf("</" + tagName + ">", index) + 4;
                    int nextTagStart = source.IndexOf("<", tagEnd);
                    string paragraphContent = source.Substring(index + 2 + tag.Length, tagEnd - index - 6 - tag.Length);
                    if (tag.Equals("b") || tag.Equals("i"))
                    {
                        parsed += "<" + tag + ">";
                    }

                    //paragraphs may contain tags such as <sup> that should be removed
                    parsed += ParseWikipediaMarkup(paragraphContent, false, true);
                    if (tag.Equals("b") || tag.Equals("i") || tag.Equals("p"))
                    {
                        parsed += "</" + tag + ">";
                    }

                    if (nextTagStart > tagEnd && includeRawText)
                        parsed += source.Substring(tagEnd, nextTagStart - tagEnd);
                    else if (nextTagStart < 0 && source.Length > tagEnd && includeRawText)
                        parsed += source.Substring(tagEnd);
                    index = nextTagStart;
                }
                else if (tag.EndsWith("/") || tag.StartsWith("/") || tagName.Equals("div") || !source.Contains("</" + tagName + ">"))
                {
                    int tagEnd = source.IndexOf(">", index) + 1;
                    int nextTagStart = source.IndexOf("<", tagEnd);
                    if (nextTagStart > tagEnd && includeRawText)
                        parsed += source.Substring(tagEnd, nextTagStart - tagEnd);
                    else if (nextTagStart < 0 && source.Length > tagEnd && includeRawText)
                        parsed += source.Substring(tagEnd);
                    index = nextTagStart;
                }
                else
                {
                    int tagEnd = source.IndexOf("</" + tagName + ">", index) + 3 + tagName.Length;
                    if (tagEnd < tagName.Length + 3)
                        break;
                    int nextTagStart = source.IndexOf("<", tagEnd);
                    if (nextTagStart > tagEnd && includeRawText)
                        parsed += source.Substring(tagEnd, nextTagStart - tagEnd);
                    else if (nextTagStart < 0 && source.Length > tagEnd && includeRawText)
                        parsed += source.Substring(tagEnd);
                    index = nextTagStart;
                }
            }

            return parsed;
        }


        public static async Task<List<string>> FetchPagesInCategory(string category)
        {
            //https://en.wikipedia.org/w/api.php?action=query&format=json&list=categorymembers&cmtitle=Category:Games

            List<string> Articles = new List<string>();
            try
            {
                string continueString = "";
                do
                {
                    var client = new HttpClient();
                    //GET
                    HttpResponseMessage resp = await client.GetAsync("https://en.wikipedia.org/w/api.php?action=query&format=json&cmlimit=100&list=categorymembers&cmtitle=Category:" + category + continueString);
                    string respString = await resp.Content.ReadAsStringAsync();

                    //Parsing response
                    Stream responseStream = resp.Content.ReadAsStream();
                    JsonElement jsonRoot = JsonDocument.Parse(responseStream).RootElement;
                    JsonElement parseResult = jsonRoot.GetProperty("query").GetProperty("categorymembers");

                    //Listing all category members
                    for (int i = 0; i < parseResult.GetArrayLength(); ++i)
                    {
                        if (parseResult[i].GetProperty("ns").GetInt32() == 0)
                            Articles.Add(parseResult[i].GetProperty("title").GetString()!);
                    }

                    JsonElement continueProperty = new JsonElement();
                    jsonRoot.TryGetProperty("continue", out continueProperty);
                    if (continueProperty.ValueKind != JsonValueKind.Undefined)
                    {
                        continueString = "&cmcontinue=" + continueProperty.GetProperty("cmcontinue").GetString()!;
                    }
                    else
                        continueString = "";

                } while (continueString.Length > 0);

            }
            catch
            {
                //todo: catch errors

            }

            return Articles;
        }
    }
}
