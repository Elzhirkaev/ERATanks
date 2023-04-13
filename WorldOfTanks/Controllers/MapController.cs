using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using WorldOfTanks.Data;
using WorldOfTanks.Models.GameLobbyModels;
using WorldOfTanks.Models.GameObject;
using WorldOfTanks.Models.ViewModels;

namespace WorldOfTanks.Controllers
{
    public class MapController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly int userMapCount = 1;
        public MapController(ApplicationDbContext db)
        {
            _db = db;
        }

        //Index GET
        public async Task<IActionResult> Index()
        {
            List<Map>? objList = new();
            try
            {
                if (User.IsInRole(WC.AdminRole)) 
                {
                    objList = await _db.Map!.ToListAsync();
                }
                else
                {
                    ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity!;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.Email);
                    if (claim != null)
                    {
                        objList = await _db.Map!.Where(x => x.Author == claim.Value).ToListAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound();
            }
            return View(objList);
        }

        //CreateEditMap GET
        [Authorize(Policy = "EmailConfirmed")]
        public async Task<IActionResult> CreateEditMap(int? id)
        {
            List<PassiveMapElement>? mapElementList;
            List<Map>? userMapList = new();
            try
            {
                mapElementList = await _db.PassiveMapElement!.ToListAsync();
                if (!User.IsInRole(WC.AdminRole))
                {
                    ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity!;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.Email);
                    if (claim != null)
                    {
                        userMapList = await _db.Map!.Where(x => x.Author == claim.Value).ToListAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound();
            }
            MapVM mapVM = new MapVM()
            {
                Map = new Map(),
                MapElementList = mapElementList,
            };
            if (id == null) 
            {
                if (User.IsInRole(WC.AdminRole)) 
                {
                    return View(mapVM);
                }
                else
                {
                    if (userMapList.Count < userMapCount)
                    {
                        return View(mapVM);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
            else
            {
                try
                {
                    mapVM.Map = await _db.Map!.FindAsync(id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                if (mapVM.Map == null)
                {
                    return NotFound();
                }
                if (!User.IsInRole(WC.AdminRole))
                {
                    ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity!;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.Email);
                    if (claim == null || mapVM.Map.Author != claim.Value) 
                    {
                        return NotFound();
                    }
                }
                return View(mapVM);
            }
        }

        //CreateEditMap POST
        [Authorize(Policy = "EmailConfirmed")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEditMap(MapVM? mapVM)
        {
            if (mapVM == null || mapVM.Map == null)
            {
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (await _db.Map!.AnyAsync(u => u.Name == mapVM.Map.Name && u.MapId != mapVM.Map.MapId)) 
                    {
                        mapVM.Info = "A map with this name already exists. Please change the name of the map";
                        if (mapVM.Map.MapId == 0)
                        {
                            mapVM.Map.MapId = -1;
                        }
                        mapVM.MapElementList = await _db.PassiveMapElement!.ToListAsync();
                        return View(mapVM);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                string val;
                int meId;
                Dictionary<string, string>? meBGDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(mapVM.Map.MapPointListBG!);
                Dictionary<string, string>? meCVDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(mapVM.Map.MapPointListCV!);
                if (meBGDict == null || meCVDict == null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    List<int> mapElementBGIdList = new();
                    List<int> mapElementCVIdList = new();
                    foreach (var item in meBGDict)
                    {
                        if (item.Value == "")
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            val = item.Value;
                        }
                        meId = Convert.ToInt32(val);
                        if (meId == 0)
                        {
                            return RedirectToAction("Index");
                        }
                        if (!mapElementBGIdList.Exists(u => u == meId)) 
                        {
                            mapElementBGIdList.Add(meId);
                        }
                    }
                    foreach (var item in meCVDict)
                    {
                        if (item.Value == "")
                        {
                            continue;
                        }
                        else
                        {
                            val = item.Value;
                        }
                        meId = Convert.ToInt32(val);
                        if (meId == 0)
                        {
                            return RedirectToAction("Index");
                        }
                        if (!mapElementCVIdList.Exists(u => u == meId))
                        {
                            mapElementCVIdList.Add(meId);
                        }
                    }
                    mapVM.Map.MapElementBGIdList = System.Text.Json.JsonSerializer.Serialize(mapElementBGIdList);
                    mapVM.Map.MapElementCVIdList = System.Text.Json.JsonSerializer.Serialize(mapElementCVIdList);
                }
                try
                {
                    ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity!;
                    var claim = claimsIdentity.FindFirst(ClaimTypes.Email);
                    if (claim == null)
                    {
                        return NotFound();
                    }
                    
                    if (mapVM.Map.MapId <= 0)
                    {
                        mapVM.Map.MapId = 0;
                        List<Map>? userMapList = new();
                        if (!User.IsInRole(WC.AdminRole))
                        {
                            if (claim != null)
                            {
                                userMapList = await _db.Map!.Where(x => x.Author == claim.Value).ToListAsync();
                            }
                        }
                        mapVM.Map.Author = claim!.Value;
                        if (User.IsInRole(WC.AdminRole)) 
                        {
                            await _db.AddAsync(mapVM.Map);
                        }
                        if (!User.IsInRole(WC.AdminRole) && userMapList.Count < userMapCount) 
                        {
                            await _db.AddAsync(mapVM.Map);
                        }
                    }
                    else
                    {
                        Map? userMap = new();
                        userMap = await _db.Map!.AsNoTracking().FirstOrDefaultAsync(u => u.MapId == mapVM.Map.MapId);
                        if (userMap != null)
                        {
                            mapVM.Map.Author = userMap.Author;
                            if (User.IsInRole(WC.AdminRole))
                            {
                                _db.Update(mapVM.Map);
                            }
                            else
                            {
                                if (claim != null && userMap.Author == claim.Value) 
                                {
                                    _db.Update(mapVM.Map);
                                }
                                else
                                {
                                    return NotFound();
                                }
                            }
                        }
                    }
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                return RedirectToAction("Index");
            }
            else
            {
                try
                {
                    mapVM.MapElementList = await _db.PassiveMapElement!.ToListAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                if (mapVM.MapElementList == null)
                {
                    return NotFound();
                }
                return View(mapVM);
            }
        }

        //DeleteMap GET
        [Authorize(Policy = "EmailConfirmed")]
        public async Task<IActionResult> DeleteMap(int? id)
        {
            List<PassiveMapElement> mapElementList;
            try
            {
                mapElementList = await _db.PassiveMapElement!.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound();
            }
            MapVM mapVM = new MapVM()
            {
                Map = new Map(),
                MapElementList = mapElementList,
            };
            if (id == null)
            {
                return NotFound();
            }
            else
            {
                ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity!;
                var claim = claimsIdentity.FindFirst(ClaimTypes.Email);
                if (claim == null)
                {
                    return NotFound();
                }
                try
                {
                    mapVM.Map = await _db.Map!.FindAsync(id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                if (mapVM.Map == null)
                {
                    return NotFound();
                }
                if (!User.IsInRole(WC.AdminRole))
                {
                    if (mapVM.Map.Author != claim.Value)
                    {
                        return NotFound();
                    }
                }
                return View(mapVM);
            }
        }

        //DeletePostMap POST
        [Authorize(Policy = "EmailConfirmed")]
        [HttpPost, ActionName("DeleteMap")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePostMap(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity!;
                var claim = claimsIdentity.FindFirst(ClaimTypes.Email);
                if (claim == null)
                {
                    return NotFound();
                }
                Map? map;
                try
                {
                    map = await _db.Map!.FindAsync(id);
                    if (map == null)
                    {
                        return NotFound();
                    }
                    if (!User.IsInRole(WC.AdminRole))
                    {
                        if (map.Author != claim.Value)
                        {
                            return NotFound();
                        }
                    }
                    _db.Remove(map);
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                return RedirectToAction("Index");
            }
        }
    }
}
