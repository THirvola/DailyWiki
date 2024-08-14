namespace DailyWikiReact.Server.Models
{
    public class GameInstance
    {
        public DateOnly Date { get; set; }

        public string? Category { get; set; }

        public string? Title { get; set; }

        public List<string>? Hints { get; set; }

        public List<string>? Options { get; set; }

        public int Tries { get; set; }

        public bool? gameEnd { get; set; }
    }
}
