using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WorldOfTanks.Data;
using WorldOfTanks.Models;
using WorldOfTanks.Models.GameLobbyModels;
using WorldOfTanks.Models.GameObject;
using WorldOfTanks.Models.ViewModels;
using System.Text.Json;
using WorldOfTanks.Utility;
using Microsoft.AspNetCore.SignalR;
using WorldOfTanks.Hubs;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using NuGet.Packaging;
using WorldOfTanks.Models.Register;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
//using WorldOfTanks.Migrations;

namespace WorldOfTanks.Controllers
{
    public class LobbyController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHubContext<LobbyHub> _hubContext;
        public static List<GameLobby> gameLobbyList = new List<GameLobby>();
        private static Timer timer = new Timer(СlearingGameLobyList, null, 300000, 300000);

        public LobbyController(ApplicationDbContext db, IHubContext<LobbyHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        //СlearingGameLobyList
        private static void СlearingGameLobyList(object? state)
        {
            TimeSpan timeSpan;
            List<GameLobby> gameLobbyRemove=new List<GameLobby>();
            foreach (var gameLobby in gameLobbyList)
            {
                timeSpan = DateTime.Now - gameLobby.CreationTime;
                if (!gameLobby.InGame && timeSpan.TotalMinutes > 5) 
                {
                    gameLobbyRemove.Add(gameLobby);
                }
            }
            foreach (var gameLobby in gameLobbyRemove)
            {
                gameLobbyList.Remove(gameLobby);
            }
        }

        //INDEX GET
        public async Task<IActionResult> Index(string? message)
        {
            GameLobbyListVM gameLobbyListVM;
            ApplicationUser? userA = null;
            string? nickName = null;
            SessionPlayer? sessionPlayer = HttpContext.Session.Get<SessionPlayer>(WC.SessionPlayer);
            if (sessionPlayer != null && sessionPlayer.LobbyId != null && !gameLobbyList.Exists(u => u.LobbyId == sessionPlayer.LobbyId)) 
            {
                sessionPlayer = null;
                HttpContext.Session.Set<SessionPlayer>(WC.SessionPlayer, sessionPlayer);
            }
            try
            {
                ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity!;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null)
                {
                    try
                    {
                        userA = await _db.ApplicationUser!.FirstOrDefaultAsync(u => u.Id == claim.Value);
                        if (userA != null && userA.EmailConfirmed)
                        {
                            nickName = userA.NickName;
                            if (gameLobbyList.Exists(u => u.LobbyId == userA.LobbyId))
                            {
                                sessionPlayer = new SessionPlayer()
                                {
                                    PlayerName = userA.PlayerName,
                                    PlayerId = userA.PlayerId,
                                    PlayerHost = userA.PlayerHost,
                                    LobbyId = userA.LobbyId,
                                };
                                HttpContext.Session.Set<SessionPlayer>(WC.SessionPlayer, sessionPlayer);
                            }
                            else
                            {
                                if (sessionPlayer != null)
                                {
                                    userA.LobbyId = sessionPlayer.LobbyId;
                                    userA.PlayerId = sessionPlayer.PlayerId;
                                    userA.PlayerName = sessionPlayer.PlayerName;
                                    userA.PlayerHost = sessionPlayer.PlayerHost;
                                    _db.Update(userA);
                                    await _db.SaveChangesAsync();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return NotFound();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound();
            }
            gameLobbyListVM = new GameLobbyListVM()
            {
                PlayerName = nickName,
                Message = message,
                GameLobbyList = gameLobbyList,
            };
            return View(gameLobbyListVM);
        }

        //JoinLobby GET
        public IActionResult JoinLobby(string? lobbyName, string? playerName)
        {
            SessionPlayer? sessionPlayer = HttpContext.Session.Get<SessionPlayer>(WC.SessionPlayer);
            if (sessionPlayer != null)
            {
                if (sessionPlayer.PlayerHost)
                {
                    return RedirectToAction("Index", new {message= "You have already created your own lobby" });
                }
                if (sessionPlayer.LobbyId != null) 
                {
                    return RedirectToAction("Index", new { message = "You are already in the lobby" });
                }
            }
            if (lobbyName == null || playerName == null) 
            {
                return RedirectToAction("Index", new { message = "Incorrect request parameters" });
            }
            string playerID = Guid.NewGuid().ToString();
            
            sessionPlayer = new SessionPlayer()
            {
                PlayerId = playerID,
                PlayerName = playerName,
                PlayerHost = false,
            };
            HttpContext.Session.Set(WC.SessionPlayer, sessionPlayer);
            return RedirectToAction("Lobby", new { lobbyName });
        }

        //ExitLobby GET
        public async Task<IActionResult> ExitLobby()
        {
            string lobbyId;
            SessionPlayer? sessionPlayer = HttpContext.Session.Get<SessionPlayer>(WC.SessionPlayer);

            if (sessionPlayer == null || sessionPlayer.LobbyId == null)
            {
                return RedirectToAction("Index", new { message = "No information about the lobby" });
            }
            else
            {
                lobbyId = sessionPlayer.LobbyId;
                if (sessionPlayer.PlayerHost)
                {
                    return RedirectToAction("Index", new { message = "You can't leave your lobby" });
                }
                if (gameLobbyList.Exists(u => u.LobbyId == lobbyId)) 
                {
                    GameLobby gameLobby = gameLobbyList.FirstOrDefault(u => u.LobbyId == lobbyId)!;
                    if (gameLobby.PlayerList!.Exists(u => u.Name == sessionPlayer.PlayerName)) 
                    {
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
                        gameLobby.PlayerList.RemoveAll(u => u.Name == sessionPlayer.PlayerName);
                        HttpContext.Session.Set<SessionPlayer>(WC.SessionPlayer, null);
                        IEnumerable<PlayerVM> playerVM = from p in gameLobby.PlayerList select new PlayerVM { Host = p.Host, Name = p.Name, Team = p.Team, Ready = p.Ready };
                        string jsonPlayerList = JsonSerializer.Serialize(playerVM);
                        RefrehsIndexLobbyPage();
                        foreach (var player in gameLobby.PlayerList)
                        {
                            if (player.Name == sessionPlayer.PlayerName)
                            {
                                continue;
                            }
                            await _hubContext.Clients.User(player.UserId!).SendAsync("refreshPlayerList", jsonPlayerList, player.Name);
                        }
                    }
                }
            }
            return RedirectToAction("Index", new { message = "You left the lobby" });
        }

        //CREATELOBBY GET
        public async Task<IActionResult> CreateLobby(string? playerName)
        {
            if (playerName == null || playerName == "" || playerName.Length > 11)  
            {
                return RedirectToAction("Index", new { message = "Incorrect request parameters" });
            }
            SessionPlayer? sessionPlayer = HttpContext.Session.Get<SessionPlayer>(WC.SessionPlayer);
            if (sessionPlayer != null && sessionPlayer.LobbyId != null) 
            {
                return RedirectToAction("Index", new { message = "You are already in the lobby" });
            }
            List<PassiveMapElement> passiveMapElementList;
            List<Map> mapList;
            try
            {
                passiveMapElementList = await _db.PassiveMapElement!.ToListAsync();
                mapList = await _db.Map!.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound();
            }
            LobbyVM LobbyVM = new LobbyVM()
            {
                GameLobby = new GameLobby()
                {
                    PlayerList = new List<Player>()
                    {
                        new Player()
                        {
                            Name = playerName,
                        },
                    }
                },
                MapElementList = passiveMapElementList,
                MapList = mapList,
            };
            return View(LobbyVM);
        }

        //CREATELOBBY POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLobby(LobbyVM? lobbyVM)
        {
            SessionPlayer? sessionPlayer = HttpContext.Session.Get<SessionPlayer>(WC.SessionPlayer);
            if (sessionPlayer != null && sessionPlayer.LobbyId != null)
            {
                return RedirectToAction("Index", new { message = "You are already in the lobby" });
            }
            if (lobbyVM == null || lobbyVM.GameLobby == null || lobbyVM.GameLobby.PlayerList == null)
            {
                return RedirectToAction("Index", new { message = "Incorrect request parameters" });
            }
            List<PassiveMapElement> passiveMapElementList;
            List<Map> mapList;
            try
            {
                passiveMapElementList = await _db.PassiveMapElement!.ToListAsync();
                mapList = await _db.Map!.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound();
            }
            if (gameLobbyList.Exists(u => u.Name == lobbyVM.GameLobby.Name)) 
            {
                lobbyVM.MapList = mapList;
                lobbyVM.MapElementList = passiveMapElementList;
                lobbyVM.Error = "ERROR : Lobby with this name already exists";
                return View(lobbyVM);
            }
            if (ModelState.IsValid)
            {
                lobbyVM.GameLobby.LobbyId = Guid.NewGuid().ToString();
                lobbyVM.GameLobby.PlayerList[0].Host = true;
                lobbyVM.GameLobby.PlayerList[0].PlayerId = Guid.NewGuid().ToString();
                ApplicationUser? userA = null;
                try
                {
                    ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity!;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    if (claim != null)
                    {
                        userA = await _db.ApplicationUser!.FirstOrDefaultAsync(u => u.Id == claim.Value);
                    }
                    if (userA != null && userA.EmailConfirmed)
                    {
                        lobbyVM.GameLobby.PlayerList[0].Name = userA.NickName;
                        lobbyVM.GameLobby.PlayerList[0].PlayerId = userA.Id;
                        userA.PlayerName = userA.NickName;
                        userA.PlayerId = userA.Id;
                        userA.PlayerHost = lobbyVM.GameLobby.PlayerList[0].Host;
                        userA.LobbyId = lobbyVM.GameLobby.LobbyId;
                        _db.Update(userA);
                        await _db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                SessionPlayer sessionPlayerId = new SessionPlayer()
                {
                    PlayerId = lobbyVM.GameLobby.PlayerList[0].PlayerId,
                    PlayerName = lobbyVM.GameLobby.PlayerList[0].Name,
                    PlayerHost = lobbyVM.GameLobby.PlayerList[0].Host,
                    LobbyId = lobbyVM.GameLobby.LobbyId,
                };
                HttpContext.Session.Set(WC.SessionPlayer, sessionPlayerId);
                lobbyVM.GameLobby.chatMessageList = new();
                lobbyVM.GameLobby.CreationTime = DateTime.Now;
                lobbyVM.GameLobby.PlayerList[0].UserId = HttpContext.Session.Id;
                gameLobbyList.Add(lobbyVM.GameLobby);
                RefrehsIndexLobbyPage();
                return RedirectToAction("Lobby", new { lobbyName = lobbyVM.GameLobby.Name });
            }
            lobbyVM.MapList = mapList;
            lobbyVM.MapElementList = passiveMapElementList;
            if (lobbyVM.GameLobby.MapId == 0) 
            {
                lobbyVM.Error = "ERROR : Select a map";
            }
            return View(lobbyVM);
        }

        //LOBBY GET
        public async Task<IActionResult> Lobby(string? lobbyName)
        {
            SessionPlayer? sessionPlayer = HttpContext.Session.Get<SessionPlayer>(WC.SessionPlayer);
            if (sessionPlayer == null)
            {
                return RedirectToAction("Index", new { message = "No information about the player" });
            }
            GameLobby? gameLobby = gameLobbyList.FirstOrDefault(u => u.Name == lobbyName);
            if (gameLobby == null)
            {
                return RedirectToAction("Index", new { message = "Lobby not found" });
            }
            if (gameLobby.InGame)
            {
                return RedirectToAction("Index", new { message = "The game is already running" });
            }
            ApplicationUser? userA = null;
            
            if (sessionPlayer.PlayerHost)
            {
                if (sessionPlayer.LobbyId != gameLobby.LobbyId) 
                {
                    return RedirectToAction("Index", new {message = "You are already in the lobby" });
                }
                return View(gameLobby);
            }
            else
            {
                try
                {
                    ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity!;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                    if (claim != null)
                    {
                        userA = await _db.ApplicationUser!.FirstOrDefaultAsync(u => u.Id == claim.Value);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                if (sessionPlayer.LobbyId == null) 
                {
                    if (gameLobby.PlayerList!.Count >= gameLobby.MaxPlayer)
                    {
                        return RedirectToAction("Index", new { message = "Exceeded the limit of players" });
                    }
                    sessionPlayer.LobbyId = gameLobby.LobbyId;
                    if (userA != null && userA.EmailConfirmed)
                    {
                        sessionPlayer.PlayerName = userA.NickName;
                    }
                    while (gameLobby.PlayerList.Exists(u => u.Name == sessionPlayer.PlayerName))
                    {
                        sessionPlayer.PlayerName += "1";
                    }
                    HttpContext.Session.Set(WC.SessionPlayer, sessionPlayer);
                    if (userA != null && userA.EmailConfirmed)
                    {
                        userA.LobbyId = gameLobby.LobbyId;
                        userA.PlayerName = sessionPlayer.PlayerName;
                        userA.PlayerId = userA.Id;
                        userA.PlayerHost = sessionPlayer.PlayerHost;
                        try
                        {
                            _db.Update(userA);
                            await _db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            return NotFound();
                        }
                    }
                    Player player = new Player()
                    {
                        UserId = HttpContext.Session.Id,
                        PlayerId = sessionPlayer.PlayerId,
                        Host = sessionPlayer.PlayerHost,
                        Name = sessionPlayer.PlayerName,
                    };
                    gameLobby.PlayerList.Add(player);
                    RefrehsIndexLobbyPage();
                    return View(gameLobby);
                }
                else
                {
                    Player? player = gameLobby.PlayerList!.FirstOrDefault(u => u.PlayerId == sessionPlayer.PlayerId);
                    if (sessionPlayer.LobbyId == gameLobby.LobbyId && player != null)
                    {
                        if (player.UserId != HttpContext.Session.Id)
                        {
                            player.UserId = HttpContext.Session.Id;
                        }
                        return View(gameLobby);
                    }
                    return RedirectToAction("Index", new { message = "You are already in the lobby" });
                }
            }
        }

        //LobbyAjax GET
        public async Task LobbyAjax()
        {
            SessionPlayer? sessionPlayer = HttpContext.Session.Get<SessionPlayer>(WC.SessionPlayer);
            if (sessionPlayer == null)
            {
                return;
            }
            else
            {
                GameLobby? gameLobby = gameLobbyList.FirstOrDefault(u => u.LobbyId == sessionPlayer.LobbyId);
                if (gameLobby != null) 
                {
                    IEnumerable<PlayerVM> playerVM = from p in gameLobby.PlayerList select new PlayerVM { Host = p.Host, Name = p.Name, Team = p.Team, Ready = p.Ready };
                    string jsonPlayerList = JsonSerializer.Serialize(playerVM);
                    foreach (var player in gameLobby.PlayerList!)
                    {
                        await _hubContext.Clients.User(player.UserId!).SendAsync("refreshPlayerList", jsonPlayerList, player.Name);
                    }
                }
                return;
            }
        }

        //LobbyAjax POST
        [HttpPost]
        public async Task LobbyAjax(int? team, string? isReady)
        {
            if (isReady == null || team == null)
            {
                return;
            }
            bool ready = false;
            int teamm = (int)team;
            if (isReady == "true")
            {
                ready = true;
            }
            SessionPlayer? sessionPlayer = HttpContext.Session.Get<SessionPlayer>(WC.SessionPlayer);
            if (sessionPlayer == null)
            {
                return;
            }
            else
            {
                GameLobby? gameLobby = gameLobbyList.FirstOrDefault(u => u.LobbyId == sessionPlayer.LobbyId);
                if (gameLobby != null)
                {
                    Player? player = gameLobby.PlayerList!.FirstOrDefault(u => u.PlayerId == sessionPlayer.PlayerId);
                    if (player != null)
                    {
                        player.Team = teamm;
                        player.Ready = ready;
                        IEnumerable<PlayerVM> playerVM = from p in gameLobby.PlayerList select new PlayerVM { Host = p.Host, Name = p.Name, Team = p.Team, Ready = p.Ready };
                        string jsonPlayerList = JsonSerializer.Serialize(playerVM);
                        foreach (var pl in gameLobby.PlayerList!)
                        {
                            await _hubContext.Clients.User(pl.UserId!).SendAsync("refreshPlayerList", jsonPlayerList, pl.Name);
                        }
                    }
                }
                return;
            }
        }

        //DELETELOBBY GET
        public async Task<IActionResult> DeleteLobby()
        {
            SessionPlayer? sessionPlayer = HttpContext.Session.Get<SessionPlayer>(WC.SessionPlayer);
            if (sessionPlayer == null)
            {
                return RedirectToAction("Index", new { message = "No information about the lobby" });
            }
            else
            {
                if (sessionPlayer.LobbyId == null) 
                {
                    return RedirectToAction("Index", new { message = "No information about the lobby" });
                }
                else
                {
                    GameLobby? gameLobby = gameLobbyList.FirstOrDefault(u => u.LobbyId == sessionPlayer.LobbyId);
                    if (gameLobby != null && sessionPlayer.PlayerHost) 
                    {
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
                        string message = "Lobby has been deleted";
                        foreach (var player in gameLobby.PlayerList!)
                        {
                            if (player.PlayerId == sessionPlayer.PlayerId) 
                            {
                                continue;
                            }
                            await _hubContext.Clients.User(player.UserId!).SendAsync("deleteLobby", message);
                        }
                        HttpContext.Session.Set<SessionPlayer>(WC.SessionPlayer, null!);
                        gameLobbyList.Remove(gameLobby);
                        RefrehsIndexLobbyPage();
                        return RedirectToAction("Index", new { message = "Lobby has been deleted" });
                    }
                    else
                    {
                        return RedirectToAction("Index", new { message = "No information about the lobby" });
                    }
                }
            }
        }

        //SetMessageAjax GET
        public async Task SetMessageAjax(string? mes)
        {
            SessionPlayer? sessionPlayer = HttpContext.Session.Get<SessionPlayer>(WC.SessionPlayer);
            if (mes == null || mes.Length > 101 || sessionPlayer == null || sessionPlayer.LobbyId == null) 
            {
                return;
            }
            GameLobby? gameLobby = gameLobbyList.FirstOrDefault(u => u.LobbyId == sessionPlayer.LobbyId);
            if (gameLobby == null)
            {
                return;
            }
            Player? player = gameLobby.PlayerList!.ToList().FirstOrDefault(u => u.PlayerId == sessionPlayer.PlayerId);
            if (player == null)
            {
                return;
            }
            TimeSpan timeSpan = DateTime.Now - sessionPlayer!.DateTimeSetMessage;
            if (timeSpan > new TimeSpan(0, 0, 0, 2, 0))
            {
                sessionPlayer.DateTimeSetMessage = DateTime.Now;
                HttpContext.Session.Set(WC.SessionPlayer, sessionPlayer);
                mes = player.Name + " : " + mes;
                gameLobby!.AddMessage(mes);
                foreach (var p in gameLobby.PlayerList!)
                {
                    await _hubContext.Clients.User(p.UserId!).SendAsync("addChatData", mes);
                }
            }
            else
            {
                await _hubContext.Clients.User(player.UserId!).SendAsync("chatErrorData", mes);
            }
        }

        //RefrehsIndexLobbyPage GET
        public void RefrehsIndexLobbyPage()
        {
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                await _hubContext.Clients.All.SendAsync("refreshPageIndexLobby");
            });
        }
    }
}