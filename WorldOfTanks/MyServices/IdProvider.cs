using Microsoft.AspNetCore.SignalR;
using WorldOfTanks.Models.GameLobbyModels;
using WorldOfTanks.Utility;

namespace WorldOfTanks.MyServices
{
    public class IdProvider : IUserIdProvider
    {
        private readonly IHttpContextAccessor? _contextAccessor;
        public IdProvider(IHttpContextAccessor contextAccessor) 
        {
            _contextAccessor = contextAccessor;
        }
        public string GetUserId(HubConnectionContext connection)
        {
            string userId;
            if (_contextAccessor != null && _contextAccessor.HttpContext != null)
            {
                userId = _contextAccessor.HttpContext.Session.Id;
            }
            else
            {
                userId = "Error";
            }
            return userId;
        }
    }
}
