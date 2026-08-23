using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using InteractiveGameApi.InteractiveGame.API.Services;

namespace InteractiveGameApi.InteractiveGame.API.Hubs
{
    public class GameHub : Hub
    {
        private readonly InteractiveGameService _service;
        public GameHub(InteractiveGameService service) => _service = service;

    }
}
