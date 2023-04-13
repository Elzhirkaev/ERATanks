using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WorldOfTanks.Data;
using WorldOfTanks.Models.GameObject;

namespace WorldOfTanks.Controllers
{
    public class WeaponController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public WeaponController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        //Index GET
        public async Task<IActionResult> Index()
        {
            List<Weapon>? objList;
            try
            {
                objList = await _db.Weapon!.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound();
            }
            return View(objList);
        }

        //CreateEditWeapon GET
        [Authorize(Roles = WC.AdminRole)]
        [Authorize(Policy = "EmailConfirmed")]
        public async Task<IActionResult> CreateEditWeapon(int? id)
        {
            Weapon? weapon = new Weapon();
            if (id == null) 
            {
                return View(weapon);
            }
            else
            {
                try
                {
                    weapon = await _db.Weapon!.FindAsync(id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                if (weapon == null) 
                {
                    return NotFound();
                }
                return View(weapon);
            }
        }

        //CreateEditWeapon POST
        [Authorize(Roles = WC.AdminRole)]
        [Authorize(Policy = "EmailConfirmed")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEditWeapon(Weapon? obj)
        {
            if (obj == null)
            {
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                if (obj.Image == null && files.Count == 0) 
                {
                    return View(obj);
                }
                if (obj.WeaponId == 0) 
                {
                    string upload = webRootPath + WC.ImageWeaponPath;
                    string fileName = Guid.NewGuid().ToString();
                    string extention = Path.GetExtension(files[0].FileName).ToLower();
                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extention), FileMode.Create)) 
                    {
                        await files[0].CopyToAsync(fileStream);
                    }
                    obj.Image = fileName + extention;
                    try
                    {
                        await _db.AddAsync(obj);
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
                    Weapon? objFromDb;
                    try
                    {
                        objFromDb = await _db.Weapon!.AsNoTracking().FirstOrDefaultAsync(u => u.WeaponId == obj.WeaponId);
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
                        string upload = webRootPath + WC.ImageWeaponPath;
                        string fileName = Guid.NewGuid().ToString();
                        string extention = Path.GetExtension(files[0].FileName).ToLower();
                        await Task.Run(() =>
                        {
                            var oldFile = Path.Combine(upload, objFromDb.Image);
                            if (System.IO.File.Exists(oldFile))
                            {
                                System.IO.File.Delete(oldFile);
                            }
                        });
                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extention), FileMode.Create))
                        {
                            await files[0].CopyToAsync(fileStream);
                        }
                        obj.Image = fileName + extention;
                    }
                    else
                    {
                        obj.Image = objFromDb.Image;
                    }
                    try
                    {
                        _db.Update(obj);
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

        //DeleteWeapon GET
        [Authorize(Roles = WC.AdminRole)]
        [Authorize(Policy = "EmailConfirmed")]
        public async Task<IActionResult> DeleteWeapon(int? id)
        {
            if (id == null || id == 0) 
            {
                return NotFound();
            }
            else
            {
                Weapon? weapon;
                try
                {
                    weapon = await _db.Weapon!.FindAsync(id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                if (weapon == null)
                {
                    return NotFound();
                }
                return View(weapon);
            }
        }

        //DeletePostWeapon POST
        [Authorize(Roles = WC.AdminRole)]
        [Authorize(Policy = "EmailConfirmed")]
        [HttpPost, ActionName("DeleteWeapon")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePostWeapon(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                Weapon? weapon;
                try
                {
                    weapon = await _db.Weapon!.FindAsync(id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                if (weapon == null)
                {
                    return NotFound();
                }
                if (weapon.Image != null)
                {
                    string upload = _webHostEnvironment.WebRootPath + WC.ImageWeaponPath;
                    await Task.Run(() =>
                    {
                        var oldFile = Path.Combine(upload, weapon.Image);
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                    });
                }
                try
                {
                    _db.Remove(weapon);
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
