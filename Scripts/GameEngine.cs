namespace DailyWiki.Scripts
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

    }
}
