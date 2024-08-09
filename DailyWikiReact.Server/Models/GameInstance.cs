namespace DailyWikiReact.Server.Models
{
    public class GameInstance
    {
        public DateOnly Date { get; set; }

        public string? Category { get; set; }

        public string? Title { get; set; }

        public string? Hint1 { get; set; }

        public List<string>? Hint2 { get; set; }

        public List<string>? Options { get; set; }
    }
}
