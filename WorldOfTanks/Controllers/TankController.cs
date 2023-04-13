using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WorldOfTanks.Data;
using WorldOfTanks.Models.GameObject;
using WorldOfTanks.Models.ViewModels;

namespace WorldOfTanks.Controllers
{
    public class TankController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public TankController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        //Index GET
        public async Task<IActionResult> Index()
        {
            List<Tank> objList;
            try
            {
                objList = await _db.Tank!.Include(u => u.Weapon).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound();
            }
            return View(objList);
        }

        //CreateEditTank GET
        [Authorize(Roles = WC.AdminRole)]
        [Authorize(Policy = "EmailConfirmed")]
        public async Task<IActionResult> CreateEditTank(int? id)
        {
            IEnumerable<SelectListItem> weaponSL;
            try
            {
                weaponSL = await _db.Weapon!.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.WeaponId.ToString(),
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound();
            }
            TankVM tankVM = new TankVM()
            {
                Tank = new Tank(),
                WeaponSelectList = weaponSL,
            };
            if (id == null) 
            {
                return View(tankVM);
            }
            else
            {
                try
                {
                    tankVM.Tank = await _db.Tank!.FindAsync(id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                if (tankVM.Tank == null) 
                {
                    return NotFound();
                }
                return View(tankVM);
            }
            
        }

        //CreateEditTank POST
        [Authorize(Roles = WC.AdminRole)]
        [Authorize(Policy = "EmailConfirmed")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEditTank(TankVM? obj)
        {
            if (obj == null || obj.Tank == null)
            {
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                if (obj.Tank.Image == null && files.Count == 0) 
                {
                    try
                    {
                        obj.WeaponSelectList = await _db.Weapon!.Select(i => new SelectListItem
                        {
                            Text = i.Name,
                            Value = i.WeaponId.ToString(),
                        }).ToListAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return NotFound();
                    }
                    return View(obj);
                }
                if (obj.Tank.TankId == 0) 
                {
                    string upload = webRootPath + WC.ImageTankPath;
                    string fileName = Guid.NewGuid().ToString();
                    string extention = Path.GetExtension(files[0].FileName).ToLower();
                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extention), FileMode.Create)) 
                    {
                        await files[0].CopyToAsync(fileStream);
                    }
                    obj.Tank.Image = fileName + extention;
                    try
                    {
                        await _db.AddAsync(obj.Tank);
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
                    Tank? objFromDb;
                    try
                    {
                        objFromDb = await _db.Tank!.AsNoTracking().FirstOrDefaultAsync(u => u.TankId == obj.Tank.TankId);
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
                        string upload = webRootPath + WC.ImageTankPath;
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
                        obj.Tank.Image = fileName + extention;
                    }
                    else
                    {
                        obj.Tank.Image = objFromDb.Image;
                    }
                    try
                    {
                        _db.Update(obj.Tank);
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
            try
            {
                obj.WeaponSelectList = await _db.Weapon!.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.WeaponId.ToString(),
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return NotFound();
            }
            return View(obj);
        }

        //DeleteTank GET
        [Authorize(Roles = WC.AdminRole)]
        [Authorize(Policy = "EmailConfirmed")]
        public async Task<IActionResult> DeleteTank(int? id)
        {
            if (id == null || id == 0) 
            {
                return NotFound();
            }
            else
            {
                Tank? tank;
                try
                {
                    tank = await _db.Tank!.Include(u => u.Weapon).FirstOrDefaultAsync(u => u.TankId == id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                if (tank == null)
                {
                    return NotFound();
                }
                return View(tank);
            }
        }

        //DeletePostTank POST
        [Authorize(Roles = WC.AdminRole)]
        [Authorize(Policy = "EmailConfirmed")]
        [HttpPost, ActionName("DeleteTank")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePostTank(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            else
            {
                Tank? tank;
                try
                {
                    tank = await _db.Tank!.FindAsync(id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return NotFound();
                }
                if (tank == null)
                {
                    return NotFound();
                }
                if (tank.Image != null)
                {
                    string upload = _webHostEnvironment.WebRootPath + WC.ImageTankPath;
                    await Task.Run(() =>
                    {
                        var oldFile = Path.Combine(upload, tank.Image);
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                    });
                }
                try
                {
                    _db.Remove(tank);
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
