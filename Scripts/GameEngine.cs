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
            for (int i = 0; i < titleWords.Length; ++i)
            {
                censoredHint = censoredHint.Replace(titleWords[i], new String('#', titleWords[i].Length), StringComparison.InvariantCultureIgnoreCase);
            }
            return censoredHint;
        }

    }
}
