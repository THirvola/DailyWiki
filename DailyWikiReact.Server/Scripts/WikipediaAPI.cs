using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DailyWikiReact.Server
{
    public class WikipediaAPI
    {
        private static readonly List<string> ignoredTags = new List<string>() { "span", "div" , "figure", "style", "!--" };

        public static string ParseWikipediaMarkup(string source, bool stopAtHeader, bool includeRawText)
        {
            string parsed = "";
            //Extracting the paragraphs
            int index = source.IndexOf("<");
            if (index > 0 && includeRawText)
                parsed = source.Substring(0, index);
            else if (index < 0 && includeRawText)
                parsed = source;
            while (index >= 0 && index < source.Length && source.Substring(index).Contains("<"))
            {

                string tag = source.Substring(index + 1, source.IndexOf(">", index) - index - 1);
                string tagName = tag.Split(' ')[0];
                if (stopAtHeader && tagName.Length == 2 && tagName.StartsWith("h"))
                    break;
                if (tag.EndsWith("/") || tag.StartsWith("/") || ignoredTags.Contains(tagName) || !source.Substring(index).Contains("</" + tagName + ">"))
                {
                    //special case of empty line tag <br/>
                    if (tagName.Equals("br"))
                        parsed += "<br />";

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
                    int tagEnd = ClosingTagIndex(source, index, tagName) + tagName.Length + 3;
                    int nextTagStart = source.IndexOf("<", tagEnd);

                    //special case: ignoring the infobox table and superscripts (references)
                    if ((tagName.Equals("table") && tag.Contains("infobox")) || (tagName.Equals("sup")))
                    {
                        index = nextTagStart;
                        continue;
                    }

                    string paragraphContent = source.Substring(index + 2 + tag.Length, tagEnd - index - 5 - tag.Length - tagName.Length);


                    //paragraphs may contain tags such as <sup> references whose contents should be removed
                    string trimmedContents = ParseWikipediaMarkup(paragraphContent, false, true);


                    if (trimmedContents.Trim().Length > 0)
                    {
                        if (!tagName.Equals("a"))
                        {
                            parsed += "<" + tagName + ">";
                        }

                        parsed += trimmedContents;

                        if (!tagName.Equals("a"))
                        {
                            parsed += "</" + tagName + ">";
                        }
                    }

                    if (nextTagStart > tagEnd && includeRawText)
                        parsed += source.Substring(tagEnd, nextTagStart - tagEnd);
                    else if (nextTagStart < 0 && source.Length > tagEnd && includeRawText)
                        parsed += source.Substring(tagEnd);
                    index = nextTagStart;
                }
            }

            return parsed;
        }

        private static int ClosingTagIndex(string source, int tagStart, string tag)
        {
            int tagEnd = tagStart+1;

            int additionalTagStack = 0;

            do
            {
                int nextTagEnd = source.IndexOf("</" + tag + ">", tagEnd);
                int nextTagStart = source.IndexOf("<" + tag + ">", tagEnd);
                int nextTagStart2 = source.IndexOf("<" + tag + " ", tagEnd);
                if ((nextTagStart < 0 || nextTagStart > nextTagEnd) && (nextTagStart2 < 0 || nextTagStart2 > nextTagEnd))
                {
                    if (additionalTagStack == 0)
                        return nextTagEnd;
                    additionalTagStack--;
                    tagEnd = nextTagEnd + 1;
                }
                else
                {

                    if (nextTagStart < 0)
                        tagEnd = nextTagStart2+1;
                    else if (nextTagStart2 < 0)
                        tagEnd = nextTagStart+1;
                    else
                        tagEnd = Math.Min(nextTagStart, nextTagStart2)+1;
                    additionalTagStack++;
                }

            } while (tagEnd < source.Length - 4);

            return source.Length-1;
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
                    bool hasContinue = jsonRoot.TryGetProperty("continue", out continueProperty);
                    if (hasContinue)
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

        public static async Task<JsonElement> GetWikipediaPage(string title)
        {
            try
            {
                var client = new HttpClient();
                //GET
                HttpResponseMessage resp = await client.GetAsync("https://en.wikipedia.org/w/api.php?action=parse&format=json&page=" + title + "&formatversion=2");
                string respString = await resp.Content.ReadAsStringAsync();

                //Parsing response
                Stream responseStream = resp.Content.ReadAsStream();
                JsonElement jsonRoot = JsonDocument.Parse(responseStream).RootElement;
                JsonElement parseResult = jsonRoot.GetProperty("parse");
                return parseResult;
            }
            catch
            {
                //todo: catch errors
            }
            return new JsonElement();
        }
    }
}
