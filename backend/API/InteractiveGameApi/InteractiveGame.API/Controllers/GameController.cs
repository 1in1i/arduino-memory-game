using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using InteractiveGameApi.InteractiveGame.BLL;
using InteractiveGameApi.InteractiveGame.API.Services;

namespace InteractiveGameApi.InteractiveGame.API.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameController : ControllerBase
    {
        private readonly InteractiveGameService _gameService;

        public GameController(InteractiveGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("submit-sequence/{gameId}")]
        public IActionResult SubmitSequence(int gameId, [FromBody] List<string> sequence)
        {
            _gameService.SubmitSequence(gameId, sequence);
            Console.WriteLine($"Sequence: {string.Join(" ", sequence)}");
            return Ok(); 
        }

        [HttpPost("game/start/{gameId}")]
        public async Task<IActionResult> StartGame(int gameId)
        {
            bool started = await _gameService.StartGame(gameId);

            if (started)
                return Ok();
            else
                return StatusCode(500, "Failed to start game");
        }

        [HttpPost("{gameId}/quit")]
        public IActionResult QuitGame(int gameId)
        {
            _gameService.HandleQuit(gameId);
            Console.WriteLine($"Quit command received for game {gameId}");
            return Ok();
        }
    }

}
