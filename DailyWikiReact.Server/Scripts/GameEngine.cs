using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Cryptography.Xml;
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
                //Short "words" such as "a" or "an" don't need to be censored
                if (titleWords[i].Length < 3)
                    continue;

                //For replacing every letter with a # character 1:1
                //censoredHint = censoredHint.Replace(titleWords[i], new String('#', titleWords[i].Length), StringComparison.InvariantCultureIgnoreCase);

                //Replacing any instance of title's words with five # characters
                censoredHint = censoredHint.Replace(titleWords[i], "#####", StringComparison.InvariantCultureIgnoreCase);
            }

            //Replacing any 3+ letter word that is also included in the title with five # characters
            //To counter obvious hints when the title contains a compound word
            string[] splitHint = censoredHint.Split(new string[3]{ ".", ",", " "}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splitHint.Length; ++i)
            {
                if (splitHint[i].Length > 2 && title.Contains(splitHint[i]) && censoredHint.Contains(splitHint[i]))
                {
                    //Replacing every occurence of this part of the title
                    censoredHint = censoredHint.Replace(splitHint[i], "#####", StringComparison.InvariantCultureIgnoreCase);
                    //For replacing only full words, use this:
                    /*
                    censoredHint = censoredHint.Replace(" " + splitHint[i] + " ", " ##### ", StringComparison.InvariantCultureIgnoreCase);
                    censoredHint = censoredHint.Replace(" " + splitHint[i] + ".", " #####.", StringComparison.InvariantCultureIgnoreCase);
                    censoredHint = censoredHint.Replace(" " + splitHint[i] + ",", " #####,", StringComparison.InvariantCultureIgnoreCase);

                    //special case of the word being at the start or the end of the hint
                    if (censoredHint.StartsWith(splitHint[i]))
                        censoredHint = "#####" + censoredHint.Substring(splitHint[i].Length);

                    if (censoredHint.EndsWith(splitHint[i]))
                        censoredHint = censoredHint.Substring(0, censoredHint.Length - splitHint[i].Length) + "#####"; */

                }

            }

            return censoredHint;
        }

        /// <summary>
        /// Get all the relevant titles in the given category.
        /// </summary>
        /// <returns>List of all the titles</returns>
        public static async Task<List<string>> GetCategoryTitles(string category)
        {
                List<string> options = await WikipediaAPI.FetchPagesInCategory(category);
                return options;
        }

        /// <summary>
        /// Gets the hints related to the given title
        /// </summary>
        /// <returns>List of hint strings. First index is the short description and the rest are paragraphs in the article.</returns>
        public static async Task<List<string>> GetHints(string title)
        {
            List<string> hints = new List<string>();

            JsonElement parseResult = await WikipediaAPI.GetWikipediaPage(title);

            JsonElement properties = parseResult.GetProperty("properties");
            JsonElement shortDesc;
            bool shortDescExists = properties.TryGetProperty("wikibase-shortdesc", out shortDesc);
            if (shortDescExists)
            {
                string hint1 = CensorHint(title, shortDesc.GetString()!);
                hints.Add(hint1);
            }
            
            string description = WikipediaAPI.ParseWikipediaMarkup(parseResult.GetProperty("text").GetString()!, true, false);
            string[] splitDesc = description.Split("</p>", StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < splitDesc.Length; ++i)
            {
                hints.Add(CensorHint(title, splitDesc[i] + "</p>"));
            }

            return hints;
        }

    }
}
