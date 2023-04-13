using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WorldOfTanks.Data;
using WorldOfTanks.Models.GameObject;
using WorldOfTanks.Models.ViewModels;

namespace WorldOfTanks.Controllers
{
    public class MapElementController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public MapElementController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        //Index GET
        public async Task<IActionResult> Index()
        {
            List<PassiveMapElement> objList;
            try
            {
                objList = await _db.PassiveMapElement!.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound();
            }
            return View(objList);
        }

        //CreateEditMapElement GET
        [Authorize(Roles = WC.AdminRole)]
        [Authorize(Policy = "EmailConfirmed")]
        public async Task<IActionResult> CreateEditMapElement(int? ind, int? id)
        {
            PassMapElementVM? mapElementVM = new()
            {
                Ind = ind ?? 0,
                MapElement = new PassiveMapElement(),
            };
            if (id == null) 
            {
                return View(mapElementVM);
            }
            else
            {
                try
                {
                    mapElementVM.MapElement = await _db.PassiveMapElement!.FindAsync(id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                if (mapElementVM.MapElement == null) 
                {
                    return NotFound();
                }
                if (!mapElementVM.MapElement.Background)
                {
                    mapElementVM.Ind = 1;
                }
                return View(mapElementVM);
            }
        }

        //CreateEditMapElement POST
        [Authorize(Roles = WC.AdminRole)]
        [Authorize(Policy = "EmailConfirmed")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEditMapElement(PassMapElementVM? obj)
        {
            if (obj == null || obj.MapElement == null)
            {
                return RedirectToAction("Index");
            }
            if(ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                if (obj.MapElement.Image == null && files.Count == 0) 
                {
                    return View(obj);
                }
                if (obj.Ind == 0)
                {
                    obj.MapElement.Background = true;
                    obj.MapElement.Invulnerability = true;
                    obj.MapElement.Resp = false;
                    obj.MapElement.HQ = false;
                }
                else
                {
                    if (obj.MapElement.Resp)
                    {
                        obj.MapElement.MachinePermeability = true;
                        obj.MapElement.BulletPermeability = true;
                        obj.MapElement.Invulnerability = true;
                    }
                    else
                    {
                        obj.MapElement.MachinePermeability = false;
                        obj.MapElement.BulletPermeability = false;
                    }
                    obj.MapElement.Background = false;
                }
                if (obj.MapElement.PasMapElementId == 0) 
                {
                    string upload = webRootPath + WC.ImageMapElementPath;
                    string fileName = Guid.NewGuid().ToString();
                    string extention = Path.GetExtension(files[0].FileName).ToLower();
                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extention), FileMode.Create)) 
                    {
                        await files[0].CopyToAsync(fileStream);
                    }
                    obj.MapElement.Image = fileName + extention;
                    try
                    {
                        await _db.AddAsync(obj.MapElement);
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return NotFound();
                    }
                }
                else
                {
                    PassiveMapElement? objFromDb;
                    try
                    {
                        objFromDb = await _db.PassiveMapElement!.AsNoTracking().FirstOrDefaultAsync(u => u.PasMapElementId == obj.MapElement.PasMapElementId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return NotFound();
                    }
                    if (objFromDb == null || objFromDb.Image == null)
                    {
                        return NotFound();
                    }
                    if (files.Count > 0) 
                    {
                        string upload = webRootPath + WC.ImageMapElementPath;
                        string fileName = Guid.NewGuid().ToString();
                        string extention = Path.GetExtension(files[0].FileName).ToLower();
                        await Task.Run(() =>
                        {
                            var oldFile = Path.Combine(upload, objFromDb.Image!);
                            if (System.IO.File.Exists(oldFile))
                            {
                                System.IO.File.Delete(oldFile);
                            }
                        });
                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extention), FileMode.Create))
                        {
                            await files[0].CopyToAsync(fileStream);
                        }
                        obj.MapElement.Image = fileName + extention;
                    }
                    else
                    {
                        obj.MapElement.Image = objFromDb.Image;
                    }
                    try
                    {
                        _db.Update(obj.MapElement);
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return NotFound();
                    }
                }
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //DeleteMapElement GET
        [Authorize(Roles = WC.AdminRole)]
        [Authorize(Policy = "EmailConfirmed")]
        public async Task<IActionResult> DeleteMapElement(int? id)
        {
            if (id == null || id == 0) 
            {
                return NotFound();
            }
            else
            {
                PassiveMapElement? mapElement;
                try
                {
                    mapElement = await _db.PassiveMapElement!.FindAsync(id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                if (mapElement == null)
                {
                    return NotFound();
                }
                return View(mapElement);
            }
        }

        //DeletePostMapElement POST
        [Authorize(Roles = WC.AdminRole)]
        [Authorize(Policy = "EmailConfirmed")]
        [HttpPost, ActionName("DeleteMapElement")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePostMapElement(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                PassiveMapElement? mapElement;
                try
                {
                    mapElement = await _db.PassiveMapElement!.FindAsync(id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                if (mapElement == null)
                {
                    return NotFound();
                }
                if (mapElement.Image != null)
                {
                    string upload = _webHostEnvironment.WebRootPath + WC.ImageMapElementPath;
                    await Task.Run(() =>
                    {
                        var oldFile = Path.Combine(upload, mapElement.Image);
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                    });
                }
                try
                {
                    _db.Remove(mapElement);
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
