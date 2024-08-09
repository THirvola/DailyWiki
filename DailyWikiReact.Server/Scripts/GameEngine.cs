using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DailyWikiReact.Server
{
    public class GameEngine
    {
        public static int GetRandomSeed()
        {
            return DateTime.UtcNow.Year * 366 + DateTime.UtcNow.DayOfYear;
        }

        public static string CensorHint(string title, string hint)
        {
            string[] titleWords = title.Split(' ');

            string censoredHint = hint + "";

            //Replacing any instance of the full title with five # characters
            censoredHint = censoredHint.Replace(title, "#####", StringComparison.InvariantCultureIgnoreCase);

            for (int i = 0; i < titleWords.Length; ++i)
            {
                //For replacing every letter with a # character 1:1
                //censoredHint = censoredHint.Replace(titleWords[i], new String('#', titleWords[i].Length), StringComparison.InvariantCultureIgnoreCase);

                //Replacing any instance of title's words with five # characters
                censoredHint = censoredHint.Replace(titleWords[i], "#####", StringComparison.InvariantCultureIgnoreCase);
            }
            return censoredHint;
        }

        public static async Task<string> GetDailyTitle(string category)
        {
                List<string> options = await WikipediaAPI.FetchPagesInCategory(category);
                string title = options[new System.Random(GetRandomSeed()).Next(0, options.Count)];
                return title;
        }

        /// <summary>
        /// Gets the hints related to the given title
        /// </summary>
        /// <returns>List of hint strings. First index is the short description and the rest are paragraphs in the article.</returns>
        public static async Task<List<string>> GetHints(string title)
        {
            JsonElement parseResult = await WikipediaAPI.GetWikipediaPage(title);
            
            string hint1 = CensorHint(title, parseResult.GetProperty("properties").GetProperty("wikibase-shortdesc").GetString()!);
            string description = WikipediaAPI.ParseWikipediaMarkup(parseResult.GetProperty("text").GetString()!, true, false);
            string[] splitDesc = description.Split("</p>", StringSplitOptions.RemoveEmptyEntries);
            List<string> hints = new List<string>() { hint1 };
            
            for (int i = 0; i < splitDesc.Length; ++i)
            {
                hints.Add(CensorHint(title, splitDesc[i] + "</p>"));
            }

            return hints;
        }

    }
}
