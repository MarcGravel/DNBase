using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DNBase.Data;
using DNBase.Models;
using System.Globalization;

namespace DNBase.Controllers
{
    public class ArtistModelsController : Controller
    {
        private readonly DNBaseContext _context;

        public ArtistModelsController(DNBaseContext context)
        {
            _context = context;
        }

        // GET: ArtistModels
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["ArtistSort"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CountrySort"] = sortOrder == "Country" ? "country_desc" : "Country";
            ViewData["SubgenreSort"] = sortOrder == "Subgenre" ? "subgenre_desc" : "Subgenre";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var artists = from a in _context.Artists
                          .Include(s => s.Subgenre)
                          select a;

            if (!String.IsNullOrEmpty(searchString))
            {
                artists = artists.Where(a => a.ArtistName.Contains(searchString) || 
                                        a.CountryOfOrigin.Contains(searchString) || 
                                        a.Subgenre.SubgenreName.Contains(searchString));
                
                if (!artists.Any(a => a.ArtistName.Contains(searchString) ||
                                        a.CountryOfOrigin.Contains(searchString) ||
                                        a.Subgenre.SubgenreName.Contains(searchString)))
                {
                    TempData["messageNonFound"] = "Oops! There is nothing here. Try another search.";
                }
            }

            switch (sortOrder)
            {
                case "name_desc":
                    artists = artists.OrderByDescending(a => a.ArtistName);
                    break;
                case "Country":
                    artists = artists.OrderBy(a => a.CountryOfOrigin);
                    break;
                case "country_desc":
                    artists = artists.OrderByDescending(a => a.CountryOfOrigin);
                    break;
                case "Subgenre":
                    artists = artists.OrderBy(a => a.Subgenre.SubgenreName);
                    break;
                case "subgenre_desc":
                    artists = artists.OrderByDescending(a => a.Subgenre.SubgenreName);
                    break;
                default:
                    artists = artists.OrderBy(a => a.ArtistName);
                    break;
            }

            int pageSize = 8;

            return View(await PaginatedList<ArtistModel>.CreateAsync(artists.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: ArtistModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artistModel = await _context.Artists
                .Include(a => a.Subgenre)
                .Include(s => s.Albums)
                    .ThenInclude(e => e.Tracks)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ArtistID == id);

            if (artistModel == null)
            {
                return NotFound();
            }

            return View(artistModel);
        }

        // GET: ArtistModels/Create
        public IActionResult Create()
        {
            
            return View();
        }

        // POST: ArtistModels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ArtistName,CountryOfOrigin,Subgenre")] ArtistModel artistModel)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(artistModel.ArtistName))
                {
                    TempData["messageArtist"] = "You must enter an artist name. Try again.";
                    return View();
                }

                if (String.IsNullOrWhiteSpace(artistModel.CountryOfOrigin))
                {
                    artistModel.CountryOfOrigin = "Unknown";
                }

                if (String.IsNullOrWhiteSpace(artistModel.Subgenre.SubgenreName))
                {
                    artistModel.Subgenre.SubgenreName = "Unknown";
                }

                TextInfo caseSwitch = new CultureInfo("en-US", false).TextInfo;

                artistModel.ArtistName = artistModel.ArtistName.ToLower();
                artistModel.ArtistName = caseSwitch.ToTitleCase(artistModel.ArtistName);

                artistModel.Subgenre.SubgenreName = artistModel.Subgenre.SubgenreName.ToLower();
                artistModel.Subgenre.SubgenreName = caseSwitch.ToTitleCase(artistModel.Subgenre.SubgenreName);

                artistModel.CountryOfOrigin = artistModel.CountryOfOrigin.ToLower();
                artistModel.CountryOfOrigin = caseSwitch.ToTitleCase(artistModel.CountryOfOrigin);

                string[] ukArray = new string[] { "uk", "united kingdom", "britain", "great britain", "england", "scotland", "wales", "northern ireland" };
                string[] usaArray = new string[] { "usa", "us", "america", "united states", "united states of america", 
                                                   "u.s", "u.s.", "u.s.a", "u.s.a.", "us of a", "'murica", "murica", "merica", "'merica" };

                foreach (var u in ukArray)
                {
                    if (artistModel.CountryOfOrigin.ToLower() == u)
                    {
                        artistModel.CountryOfOrigin = "UK";
                    }
                }

                foreach (var u in usaArray)
                {
                    if (artistModel.CountryOfOrigin.ToLower() == u)
                    {
                        artistModel.CountryOfOrigin = "USA";
                    }
                }

                SubgenreModel subgenre = await _context.Subgenres.SingleOrDefaultAsync(s => s.SubgenreName == artistModel.Subgenre.SubgenreName);

                if (subgenre == null)
                {
                    subgenre = new SubgenreModel()
                    {
                        SubgenreName = artistModel.Subgenre.SubgenreName
                    };
                }

                ArtistModel newArtist = new ArtistModel()
                {
                    ArtistName = artistModel.ArtistName,
                    CountryOfOrigin = artistModel.CountryOfOrigin,
                    Subgenre = subgenre
                };

                var subgenreNames = _context.Subgenres;

                foreach (var s in subgenreNames)
                {
                    if (artistModel.Subgenre.SubgenreName.ToLower() == s.SubgenreName.ToLower())
                    {
                        artistModel.SubgenreID = s.SubgenreID;
                    }
                }

                var artistNames = _context.Artists;
                foreach (var a in artistNames)
                {
                    if (artistModel.ArtistName.ToLower() == a.ArtistName.ToLower())
                    {
                        TempData["message"] = "That artist already exists in the database.";
                        return View();
                    }
                }
                
                _context.Add(newArtist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }

            return View(artistModel);
        }

        // GET: ArtistModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View();
            }

            var artistModel = await _context.Artists
                .Include(a => a.Subgenre)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ArtistID == id);

            if (artistModel == null)
            {
                return View();
            }

            return View(artistModel);
        }

        // POST: ArtistModels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ArtistName,CountryOfOrigin,Subgenre")] ArtistModel artistModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var artistToUpdate = await _context.Artists
                        .Include(a => a.Albums)
                        .ThenInclude(t => t.Tracks)
                        .Include(s => s.Subgenre)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.ArtistID == id);

                    if (String.IsNullOrWhiteSpace(artistModel.ArtistName))
                    {
                        TempData["messageArtist"] = "You must enter an artist. Try again.";
                        return View();
                    }
                    if (String.IsNullOrWhiteSpace(artistModel.CountryOfOrigin))
                    {
                        artistModel.CountryOfOrigin = "Unknown";
                    }

                    if (String.IsNullOrWhiteSpace(artistModel.Subgenre.SubgenreName))
                    {
                        artistModel.Subgenre.SubgenreName = "Unknown";
                    }

                    TextInfo caseSwitch = new CultureInfo("en-US", false).TextInfo;

                    artistModel.ArtistName = artistModel.ArtistName.ToLower();
                    artistModel.ArtistName = caseSwitch.ToTitleCase(artistModel.ArtistName);

                    artistModel.Subgenre.SubgenreName = artistModel.Subgenre.SubgenreName.ToLower();
                    artistModel.Subgenre.SubgenreName = caseSwitch.ToTitleCase(artistModel.Subgenre.SubgenreName);

                    artistModel.CountryOfOrigin = artistModel.CountryOfOrigin.ToLower();
                    artistModel.CountryOfOrigin = caseSwitch.ToTitleCase(artistModel.CountryOfOrigin);

                    string[] ukArray = new string[] { "uk", "united kingdom", "britain", "great britain", "england", "scotland", "wales", "northern ireland" };
                    string[] usaArray = new string[] { "usa", "us", "america", "united states", "united states of america",
                                                   "u.s", "u.s.", "u.s.a", "u.s.a.", "us of a", "'murica", "murica", "merica", "'merica" };

                    foreach (var u in ukArray)
                    {
                        if (artistModel.CountryOfOrigin.ToLower() == u)
                        {
                            artistModel.CountryOfOrigin = "UK";
                        }
                    }

                    foreach (var u in usaArray)
                    {
                        if (artistModel.CountryOfOrigin.ToLower() == u)
                        {
                            artistModel.CountryOfOrigin = "USA";
                        }
                    }

                    if (artistModel.ArtistName.ToLower() != artistToUpdate.ArtistName.ToLower())
                    {
                        artistModel.ArtistID = id;
                    }

                    if (artistModel.CountryOfOrigin.ToLower() != artistToUpdate.CountryOfOrigin.ToLower())
                    {
                        artistToUpdate.CountryOfOrigin = artistModel.CountryOfOrigin;
                        artistModel.ArtistID = id;
                    }

                    if (artistModel.ArtistName.ToLower() == artistToUpdate.ArtistName.ToLower() &&
                        artistModel.CountryOfOrigin.ToLower() == artistToUpdate.CountryOfOrigin.ToLower() &&
                        artistModel.Subgenre.SubgenreName.ToLower() == artistToUpdate.Subgenre.SubgenreName.ToLower())
                    {
                        artistModel.ArtistID = id;
                    }

                    var duplicateCheck = _context.Artists
                        .Include(s => s.Subgenre)
                        .Include(a => a.Albums)
                        .ThenInclude(t => t.Tracks)
                        .AsNoTracking();

                    foreach (var d in duplicateCheck)
                    {
                        if (artistModel.ArtistName.ToLower() == d.ArtistName.ToLower())
                        {
                            if (artistModel.ArtistName.ToLower() != artistToUpdate.ArtistName.ToLower())
                            {
                                TempData["messageExists"] = "That artist already exists.";
                                return View();
                            }
                        }
                    }

                    var subgenreCheck = _context.Subgenres
                        .AsNoTracking();

                    foreach (var s in subgenreCheck)
                    {
                        if (artistModel.Subgenre.SubgenreName == s.SubgenreName)
                        {
                            artistModel.ArtistID = id;
                            artistModel.Subgenre.SubgenreID = s.SubgenreID;
                        }
                    }

                    if (!subgenreCheck.Any(s => s.SubgenreName == artistModel.Subgenre.SubgenreName))
                    {
                        artistModel.ArtistID = id;
                        SubgenreModel newSubgenre = new SubgenreModel();
                    }

                    _context.Update(artistModel);
                    await _context.SaveChangesAsync();
                }

                catch (DbUpdateException)
                {
                    if (!ArtistModelExists(artistModel.ArtistID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(artistModel);
        }

        // GET: ArtistModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artistModel = await _context.Artists
                .Include(a => a.Subgenre)
                .Include(s => s.Albums)
                    .ThenInclude(e => e.Tracks)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ArtistID == id);

            if (artistModel == null)
            {
                return NotFound();
            }

            return View(artistModel);
        }

        // POST: ArtistModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artistModel = await _context.Artists.FindAsync(id);
            var subgenreIdOfDeleted = artistModel.SubgenreID;
            var subgenreModel = await _context.Subgenres.FindAsync(subgenreIdOfDeleted);
            int count = 0;

            var checkArtistsWithSubgenre = _context.Artists
                .Include(s => s.Subgenre)
                .AsNoTracking();

            foreach (var artist in checkArtistsWithSubgenre)
            {
                if (artist.SubgenreID == subgenreIdOfDeleted)
                {
                    count++;
                }
            }

            var checkSubgenres = _context.Subgenres
                .AsNoTracking();

            foreach (var c in checkSubgenres)
            {
                if (c.SubgenreID == subgenreIdOfDeleted && count == 1)
                {
                    _context.Subgenres.Remove(subgenreModel);
                }
            }

            _context.Artists.Remove(artistModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArtistModelExists(int id)
        {
            return _context.Artists.Any(e => e.ArtistID == id);
        }

    }
}
