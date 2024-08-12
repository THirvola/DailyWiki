using DailyWikiReact.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DailyWikiReact.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameInstanceController : ControllerBase
    {
        private static readonly string[] Categories = new[]
        {
            "Physics", "Countries in Europe", "States of the United States", "Chemical elements", "Fish common names", "Chinese inventions"
        };

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
                Options = options

            }};
        }

        /*
         * todo: use fetch in REACT to POST the result of the game (along with a fingerprint and all data that will be saved in the database)
         *Receive the data in a method akin to the following
         [HttpPost]
[Route("test")]
public object Test([FromHeader] string token, [FromForm] MyDataClass data)
{
    // check token
    // do something with data
    return CustomResult.Ok;
}
         */
    }
}
