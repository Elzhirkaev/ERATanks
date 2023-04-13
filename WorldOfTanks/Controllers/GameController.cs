using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Numerics;
using System.Security.Claims;
using System.Text.Json;
using WorldOfTanks.Data;
using WorldOfTanks.Hubs;
using WorldOfTanks.Models;
using WorldOfTanks.Models.GameLobbyModels;
using WorldOfTanks.Models.GameObject;
using WorldOfTanks.Models.GamePlayModels;
using WorldOfTanks.Models.Register;
using WorldOfTanks.Models.ViewModels;
using WorldOfTanks.Utility;

namespace WorldOfTanks.Controllers
{
    public class GameController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<LobbyHub> _hubContext;
        private static List<Game> gameList= new List<Game>();
        private SessionPlayer? _sessionPlayer;
        private Game? _game;
        private Player? _player;
        private static readonly Timer timer = new(СlearingGameList, null, 300000, 300000);

        public GameController(ApplicationDbContext db, IHubContext<LobbyHub> hubContext, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _hubContext = hubContext;
            if (httpContextAccessor.HttpContext != null)
            {
                _sessionPlayer = httpContextAccessor.HttpContext.Session.Get<SessionPlayer>(WC.SessionPlayer);
                if (_sessionPlayer != null && _sessionPlayer.LobbyId != null)
                {
                    _game = gameList.FirstOrDefault(u => u.GameId == _sessionPlayer.LobbyId);
                    if (_game != null && _game.PlayerList != null)
                    {
                        _player = _game.PlayerList.FirstOrDefault(u => u.PlayerId == _sessionPlayer.PlayerId);
                    }
                }
            }
        }

        //СlearingGameLobyList
        private static void СlearingGameList(object? state)
        {
            TimeSpan timeSpan;
            List<Game> gameRemove = new List<Game>();
            foreach (var game in gameList)
            {
                timeSpan = DateTime.Now - game.LastTime;
                if (timeSpan.TotalMinutes > 5)
                {
                    gameRemove.Add(game);
                    GameLobby? gameLobby = LobbyController.gameLobbyList.FirstOrDefault(u => u.LobbyId == game.GameId);
                    if (gameLobby != null)
                    {
                        LobbyController.gameLobbyList.Remove(gameLobby);
                    }
                }
            }
            foreach (var game in gameRemove)
            {
                gameList.Remove(game);
            }
        }

        //GameStart GET
        public async Task<IActionResult> GameStart()
        {
            if (_sessionPlayer == null || _sessionPlayer.LobbyId == null) 
            {
                return RedirectToAction("Index", "Lobby");
            }
            GameLobby? gameLobby = LobbyController.gameLobbyList.FirstOrDefault(u => u.LobbyId == _sessionPlayer.LobbyId);
            if (gameLobby != null) 
            {
                if (_sessionPlayer.PlayerHost)
                {
                    if (gameLobby.PlayerList != null && gameLobby.LobbyId != null)
                    {
                        foreach (var player in gameLobby.PlayerList)
                        {
                            if (!player.Ready)
                            {
                                return RedirectToAction("Index", "Lobby", new { message = "Not all players are ready" });
                            }
                        }
                        Map? map;
                        List<Tank>? tankList;
                        try
                        {
                            map = await _db.Map!.FirstOrDefaultAsync(u => u.MapId == gameLobby.MapId);
                            tankList = await _db.Tank!.Include(u => u.Weapon).ToListAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            return NotFound();
                        }
                        if (map == null || map.MapElementBGIdList == null || map.MapElementCVIdList == null || tankList.Count == 0) 
                        {
                            return RedirectToAction("Index", "Lobby", new { message = "failed to load data for the game" });
                        }
                        List<PassiveMapElement>? mapElementList;
                        List<int>? meIdList = new();
                        List<int>? mapMEBGIdList;
                        List<int>? mapMECVIdList;
                        mapMEBGIdList = JsonSerializer.Deserialize<List<int>>(map.MapElementBGIdList);
                        mapMECVIdList = JsonSerializer.Deserialize<List<int>>(map.MapElementCVIdList);
                        if (mapMEBGIdList == null || mapMECVIdList == null) 
                        {
                            return RedirectToAction("Index", "Lobby", new { message = "failed to load data for the game" });
                        }
                        meIdList.AddRange(mapMEBGIdList);
                        meIdList.AddRange(mapMECVIdList);
                        try
                        {
                            mapElementList = await _db.PassiveMapElement!.Where(u => meIdList.Contains(u.PasMapElementId)).ToListAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            return NotFound();
                        }
                        gameLobby.InGame = true;
                        Game game = new Game()
                        {
                            GameId = gameLobby.LobbyId,
                            StartTime = DateTime.Now,
                            PlayerList = gameLobby.PlayerList,
                            Map = map,
                            MapElementList = mapElementList,
                            TankList = tankList,
                            NumOfTeams = gameLobby.NumOfTeams,
                            RebirthPoints = gameLobby.RebirthPoints,
                            FriendlyFire = gameLobby.FriendlyFire,
                        };
                        game.RenderMap();
                        gameList.Add(game);
                        foreach (var player in game.PlayerList)
                        {
                            if (player.PlayerId == _sessionPlayer.PlayerId)
                            {
                                continue;
                            }
                            await _hubContext.Clients.User(player.UserId!).SendAsync("gameStart");
                        }
                        return RedirectToAction("Index", "Game");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Lobby");
                }
            }
            return RedirectToAction("Index", "Lobby", new { message = "Lobby not found" });
        }

        //Index GET
        public IActionResult Index()
        {
            if (_player != null) 
            {
                if (_player.UserId != HttpContext.Session.Id) 
                {
                    _player.UserId = HttpContext.Session.Id;
                }
                return View(_game);
            }
            return RedirectToAction("Index", "Lobby", new { message = "There is no data about the game" });
        }

        //ExitGame GET
        public async Task<IActionResult> ExitGame()
        {
            int countLeave = 0;
            if (_player != null)
            {
                _game!.DeletePlayer(_player);
                GameLobby? gameLobby = LobbyController.gameLobbyList.FirstOrDefault(u => u.LobbyId == _sessionPlayer!.LobbyId);
                _sessionPlayer = null;
                HttpContext.Session.Set(WC.SessionPlayer, _sessionPlayer);
                ApplicationUser? userA = null;
                try
                {
                    ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity!;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    if (claim != null)
                    {
                        userA = await _db.ApplicationUser!.FirstOrDefaultAsync(u => u.Id == claim.Value);
                        if (userA != null)
                        {
                            userA.LobbyId = null;
                            userA.PlayerName = null;
                            userA.PlayerId = null;
                            userA.PlayerHost = false;
                            _db.Update(userA);
                            await _db.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                foreach (var pl in _game.PlayerList!)
                {
                    if (pl.RebirthPoints > -11111111 && pl.RebirthPoints < -999999)
                    {
                        countLeave++;
                    }
                }
                if (_game.PlayerList!.Count == countLeave)
                {
                    gameList.Remove(_game);
                    if (gameLobby != null)
                    {
                        LobbyController.gameLobbyList.Remove(gameLobby);
                    }
                    await _hubContext.Clients.All.SendAsync("refreshPageIndexLobby");
                }
            }
            return RedirectToAction("Index", "Lobby", new { message = "you are not participating in the game" });
        }

        //SelectTank GET
        public async Task SelectTankAjax(int x, int y, int direction, double directionDouble, int tankId) 
        {
            if (_player != null)
            {
                string message = _game!.SelectTank(x, y, direction, directionDouble, tankId, _player);
                await _hubContext.Clients.User(_player.UserId!).SendAsync("selectTankStatus", message);
            }
        }

        //SetGetDataAjax GET
        public async Task SetGetDataAjax(int w, int s, int a, int d, int mbtn, int directionTower)
        {
            if (_player != null)
            {
                TimeSpan timeSpan = DateTime.Now - _sessionPlayer!.DateTimeGetSetData;
                double timeSp = timeSpan.TotalMilliseconds;
                string gameDataJson;
                if (timeSpan > new TimeSpan(0, 0, 0, 0, 24))
                {
                    if (timeSp > 5000)
                    {
                        timeSp = 5000;
                        _player.OnMove = false;
                    }
                    _sessionPlayer.DateTimeGetSetData = DateTime.Now;
                    HttpContext.Session.Set(WC.SessionPlayer, _sessionPlayer);
                    if (!_player.OnMove)
                    {
                        _game!.TankMove(w, s, a, d, mbtn, directionTower, _player, timeSp);
                        _player.OnMove = false;
                    }
                    if (!_player.OnGet)
                    {
                        gameDataJson = _game!.GetGameData(_player);
                        await _hubContext.Clients.User(_player.UserId!).SendAsync("refreshGameData", gameDataJson);
                        _player.OnGet = false;
                    }
                }
            }
        }

        //SetMessageAjax GET
        public async Task SetMessageAjax(string? mes)
        {
            if (_player != null && mes != null && mes.Length < 101) 
            {
                TimeSpan timeSpan = DateTime.Now - _sessionPlayer!.DateTimeSetMessage;
                if (timeSpan > new TimeSpan(0, 0, 0, 2, 0)) 
                {
                    _sessionPlayer.DateTimeSetMessage = DateTime.Now;
                    HttpContext.Session.Set(WC.SessionPlayer, _sessionPlayer);
                    mes = _player.Name + " : " + mes;
                    _game!.AddMessage(mes);
                    foreach (var player in _game.PlayerList!)
                    {
                        await _hubContext.Clients.User(player.UserId!).SendAsync("addChatData", mes);
                    }
                }
                else
                {
                    await _hubContext.Clients.User(_player.UserId!).SendAsync("chatErrorData", mes);
                }
            }
        }

        //GiveRPAjax GET
        public void GiveRPAjax(int? rp, string? name)
        {
            if (_player != null && rp != null && name != null && rp != 0) 
            {
                _game!.AddRP(_player, (int)rp, name);
            }
        }
    }
}