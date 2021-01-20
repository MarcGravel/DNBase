using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DNBase.Data;
using DNBase.Models;
using System.Data;
using System.Globalization;

namespace DNBase.Controllers
{
    public class SubgenreModelsController : Controller
    {
        private readonly DNBaseContext _context;

        public SubgenreModelsController(DNBaseContext context)
        {
            _context = context;
        }

        // GET: SubgenreModels
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSort"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var subgenres = from s in _context.Subgenres
                            select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                subgenres = subgenres.Where(s => s.SubgenreName.Contains(searchString));

                if (!subgenres.Any(s => s.SubgenreName.Contains(searchString)))
                {
                    TempData["messageNonFound"] = "Oops! There is nothing here. Try another search.";
                }
            }

            switch (sortOrder)
            {
                case "name_desc":
                    subgenres = subgenres.OrderByDescending(s => s.SubgenreName);
                    break;
                default:
                    subgenres = subgenres.OrderBy(s => s.SubgenreName);
                    break;
            }

            int pageSize = 8;

            return View(await PaginatedList<SubgenreModel>.CreateAsync(subgenres.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: SubgenreModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subgenreModel = await _context.Subgenres
                .Include(s => s.Artists)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.SubgenreID == id);

            if (subgenreModel == null)
            {
                return NotFound();
            }

            return View(subgenreModel);
        }

        // GET: SubgenreModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SubgenreModels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubgenreName")] SubgenreModel subgenreModel)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(subgenreModel.SubgenreName))
                {
                    TempData["message"] = "You must enter a subgenre. Try again.";
                    return View();
                }

                TextInfo toTitleCase = new CultureInfo("en-US", false).TextInfo;

                subgenreModel.SubgenreName = subgenreModel.SubgenreName.ToLower();
                subgenreModel.SubgenreName = toTitleCase.ToTitleCase(subgenreModel.SubgenreName);

                var subgenreNames = _context.Subgenres;

                foreach (var s in subgenreNames)
                {
                    if(subgenreModel.SubgenreName.ToLower() == s.SubgenreName.ToLower())
                    {
                        TempData["message"] = "That subgenre already exists.";
                        return View();
                    }
                }

                _context.Add(subgenreModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subgenreModel);
        }

        // GET: SubgenreModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View();
            }

            var subgenreModel = await _context.Subgenres.FindAsync(id);

            if (subgenreModel == null)
            {
                return NotFound();
            }
            return View(subgenreModel);
        }

        // POST: SubgenreModels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SubgenreName")] SubgenreModel subgenreModel)
        {
            if (String.IsNullOrWhiteSpace(subgenreModel.SubgenreName))
            {
                TempData["messageSubName"] = "You must enter a subgenre. Try again.";
                return View();
            }

            var subgenreToUpdate = await _context.Subgenres
                .FirstOrDefaultAsync(c => c.SubgenreID == id);

            TextInfo toTitleCase = new CultureInfo("en-US", false).TextInfo;

            subgenreModel.SubgenreName = toTitleCase.ToTitleCase(subgenreModel.SubgenreName);

            var subgenreNames = _context.Subgenres;
            foreach (var s in subgenreNames)
            {
                if (subgenreModel.SubgenreName.ToLower() == s.SubgenreName.ToLower())
                {
                    TempData["messageExists"] = "That subgenre already exists.";
                    return View();
                }
            }

            if (await TryUpdateModelAsync<SubgenreModel>(subgenreToUpdate, "", c => c.SubgenreID, c => c.SubgenreName))
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable To Save Changes");
                }
                return RedirectToAction(nameof(Index));
            }
            return View(subgenreModel);
        }

        // GET: SubgenreModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subgenreModel = await _context.Subgenres
                .Include(a => a.Artists)
                .FirstOrDefaultAsync(m => m.SubgenreID == id);

            if (subgenreModel == null)
            {
                return NotFound();
            }

            return View(subgenreModel);
        }

        // POST: SubgenreModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subgenreModel = await _context.Subgenres.FindAsync(id);

            if (subgenreModel.SubgenreName == "Unknown")
            {
                TempData["messageCannotDelete"] = "Sorry, the 'Unknown' group cannot be deleted.";
                return View();
            }

            var subgenreUnknownCheck = _context.Subgenres
                .AsNoTracking();

            var artists = _context.Artists.Where(a => a.SubgenreID == id);

            if (!subgenreUnknownCheck.Any(s => s.SubgenreName == "Unknown"))
            {
                var unknown = CreateUnknownSubgenre();
            }


            if (artists != null)
            {
                foreach (var artist in artists)
                {
                    foreach (var s in subgenreUnknownCheck)
                    {
                        if (s.SubgenreName == "Unknown")
                        {
                            artist.SubgenreID = s.SubgenreID;
                        }

                    }
                }
            }

            _context.Subgenres.Remove(subgenreModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public SubgenreModel CreateUnknownSubgenre()
        {
            SubgenreModel unknown = new SubgenreModel()
            {
                SubgenreName = "Unknown"
            };

            _context.Add(unknown);
           _context.SaveChanges();

            return unknown;
        }

        private bool SubgenreModelExists(int id)
        {
            return _context.Subgenres.Any(e => e.SubgenreID == id);
        }
    }
}
