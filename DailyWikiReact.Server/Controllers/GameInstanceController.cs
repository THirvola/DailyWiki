using DailyWikiReact.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DailyWikiReact.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameInstanceController : ControllerBase
    {
        private readonly ILogger<GameInstanceController> _logger;

        public GameInstanceController(ILogger<GameInstanceController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetGameInstance")]
        public IEnumerable<GameInstance> Get(string category)
        {
            //string category = Categories[Random.Shared.Next(Categories.Length)];
            List<string> options = GameEngine.GetCategoryTitles(category).Result;
            string title = options[new Random(GameEngine.GetRandomSeed()).Next(0, options.Count)];
            List<string> hints = GameEngine.GetHints(title).Result;
            string hint1 = hints[0];
            hints.RemoveAt(0);
            return new GameInstance[1]
            { new GameInstance{
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Category = category,
                Title = title,
                Hint1 = hint1,
                Hint2 = hints,
                Options = options,
                Tries = 0,
                gameEnd = false

            }};
        }

        [HttpPost(Name = "PostGameResult")]
        [Route("postgameresult")]
        public object PostGameResult([FromHeader] string token, [FromForm] GameInstance data)
        {
            if (token.Equals("Game result"))
            {
                //todo: add data to database
                Console.WriteLine(data.Tries);
                return new string[1] { "Success" };
            }

            return new string[1] { "Failed" };
        }
         
    }
}
