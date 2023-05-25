using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using task2.Areas.Admin.Viewmodel;
using task2.DAL;
using task2.Models;
using task2.Utilities.Constrants;

namespace task2.Areas.Admin.Controllers
{
    [Area("Admin")] 
    public class TeamsController : Controller
    {
        private readonly AppDbcontext _appdbcontext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TeamsController(AppDbcontext appdbcontext, IWebHostEnvironment webHostEnvironment)
        {
            _appdbcontext = appdbcontext;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task <IActionResult> Index()
        {
            return View(await _appdbcontext.Teams.Where(t=>!t.IsDelected).ToListAsync());

        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public  async Task<IActionResult> Create(CreatedTeamVM teamVM)
        {
            if (!ModelState.IsValid) { return View(teamVM); }
           
            if (!teamVM.Photo.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Photo", ErrorMessage.FileMustBeTypeImage);
                return View(teamVM);
            }
            if (teamVM.Photo.Length / 1024 > 200)
            {
                ModelState.AddModelError("Photo", ErrorMessage.FileSizeMustlessThan200Kb);
                return View(teamVM);
            }
            string Rootpath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images");
            string filename = Guid.NewGuid().ToString() + teamVM.Photo.FileName;
            using (FileStream fileStream = new FileStream(Path.Combine(Rootpath, filename), FileMode.Create))
            {
                await teamVM.Photo.CopyToAsync(fileStream);

            }
            Team team = new Team()
            {
                Name = teamVM.Name,
                Description = teamVM.Description,
                ImagePath = filename
            };
            await _appdbcontext.Teams.AddAsync(team);
            await _appdbcontext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
    

        }
        public async Task<IActionResult> Delete(int id)
        {
            Team team= await _appdbcontext.Teams.FindAsync(id);
            if (team == null) { return NotFound(); }
                string filepath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images", team.ImagePath);
            if(System.IO.File.Exists(filepath))
            {
                System.IO.File.Delete(filepath);
            }
            
            

            _appdbcontext.Teams.Remove(team);
          await  _appdbcontext.SaveChangesAsync(true);
            return RedirectToAction(nameof(Index));

        }
    }
       

    
}
