using Microsoft.AspNetCore.SignalR;
using WorldOfTanks.Controllers;
using WorldOfTanks.Models.GameLobbyModels;

namespace WorldOfTanks.Hubs
{
    public class LobbyHub:Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (Context.UserIdentifier == "Error")
            {
                await Clients.User("Error").SendAsync("redirectToHome", "Error");
            } 
            await base.OnConnectedAsync();
        }
    }
}
