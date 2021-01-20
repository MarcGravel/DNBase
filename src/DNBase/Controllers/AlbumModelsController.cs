using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DNBase.Data;
using DNBase.Models;
using System.Globalization;

namespace DNBase.Controllers
{
    public class AlbumModelsController : Controller
    {
        private readonly DNBaseContext _context;

        public AlbumModelsController(DNBaseContext context)
        {
            _context = context;
        }

        // GET: AlbumModels
        public async Task<IActionResult> Index(string sortOrder, string currentFiler, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSort"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["ArtistSort"] = sortOrder == "Artist" ? "artist_desc" : "Artist";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFiler;
            }

            ViewData["CurrentFilter"] = searchString;

            var albums = from a in _context.Albums
                         .Include(r => r.Artist)
                         select a;

            if (!String.IsNullOrEmpty(searchString))
            {
                albums = albums.Where(a => a.AlbumName.Contains(searchString) ||
                                      a.Artist.ArtistName.Contains(searchString));


                if (!albums.Any(a => a.AlbumName.Contains(searchString) ||
                                      a.Artist.ArtistName.Contains(searchString)))
                {
                    TempData["messageNonFound"] = "Oops! There is nothing here. Try another search.";
                }

            }

            switch (sortOrder)
            {
                case "name_desc":
                    albums = albums.OrderByDescending(a => a.AlbumName);
                    break;
                case "Artist":
                    albums = albums.OrderBy(a => a.Artist.ArtistName);
                    break;
                case "artist_desc":
                    albums = albums.OrderByDescending(a => a.Artist.ArtistName);
                    break;
                default:
                    albums = albums.OrderBy(a => a.AlbumName);
                    break;
            }

            int pageSize = 8;
            return View(await PaginatedList<AlbumModel>.CreateAsync(albums.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: AlbumModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var albumModel = await _context.Albums
                .Include(s => s.Tracks)
                .Include(a => a.Artist)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.AlbumID == id);

            if (albumModel == null)
            {
                return NotFound();
            }

            return View(albumModel);
        }

        // GET: AlbumModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AlbumModels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlbumName,Artist")] AlbumModel albumModel)
        {   
            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(albumModel.AlbumName))
                {
                    TempData["messageAlbumName"] = "You must enter an album. Try again.";
                    return View();
                }

                if (albumModel.AlbumName.ToLower() == "single" || albumModel.AlbumName.ToLower() == "singles" ||
                        albumModel.AlbumName.ToLower() == "no album" || albumModel.AlbumName.ToLower() == "n/a")
                {
                    TempData["messageAlbumName"] = "If you are trying to add an individual track, please go to 'Tracks' and add it there.";
                    return View();
                }

                if (String.IsNullOrWhiteSpace(albumModel.Artist.ArtistName))
                {
                    TempData["messageArtistName"] = "You must enter an artist. Try again.";
                    return View();
                }

                TextInfo caseSwitch = new CultureInfo("en-US", false).TextInfo;

                albumModel.AlbumName = albumModel.AlbumName.ToLower();
                albumModel.AlbumName = caseSwitch.ToTitleCase(albumModel.AlbumName);

                albumModel.Artist.ArtistName = albumModel.Artist.ArtistName.ToLower();
                albumModel.Artist.ArtistName = caseSwitch.ToTitleCase(albumModel.Artist.ArtistName);

                var albumDuplicate = _context.Albums
                    .Include(a => a.Artist)
                    .AsNoTracking();

                foreach (var a in albumDuplicate)
                {
                    if (albumModel.AlbumName.ToLower() == a.AlbumName.ToLower())
                    {
                        if (albumModel.Artist.ArtistName.ToLower() == a.Artist.ArtistName.ToLower())
                        {
                            TempData["message"] = "That album already exists.";
                            return View();
                        }
                    }
                }

                var artistNames = _context.Artists
                    .AsNoTracking();

                foreach (var a in artistNames)
                {
                    if (albumModel.Artist.ArtistName.ToLower() == a.ArtistName.ToLower())
                    {
                        albumModel.ArtistID = a.ArtistID;
                    }
                }

                ArtistModel artist = await _context.Artists.SingleOrDefaultAsync(a => a.ArtistName == albumModel.Artist.ArtistName);

                if (artist == null)
                {
                    TempData["message"] = "Oops! That artist has not been entered in DNBase yet. " +
                        "Please add them below.";
                    return RedirectToAction("Create", "ArtistModels");
                }
                else
                {
                    AlbumModel newAlbum = new AlbumModel()
                    {
                        AlbumName = albumModel.AlbumName,
                        Artist = artist
                    };

                    _context.Add(newAlbum);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(albumModel);
        }

        // GET: AlbumModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View();
            }

            var albumModel = await _context.Albums
                .Include(a => a.Artist)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.AlbumID == id);

            if (albumModel == null)
            {
                return View();
            }
            return View(albumModel);
        }

        // POST: AlbumModels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlbumName,Artist")] AlbumModel albumModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(albumModel.AlbumName))
                    {
                        TempData["messageAlbum"] = "You must enter an album. Try again.";
                        return View();
                    }

                    if (albumModel.AlbumName.ToLower() == "single" || albumModel.AlbumName.ToLower() == "singles" ||
                        albumModel.AlbumName.ToLower() == "no album" || albumModel.AlbumName.ToLower() == "n/a")
                    {
                        TempData["message"] = "If you are trying to add an individual track, please go to 'Tracks' and add it there.";
                        return View();
                    }

                    if (String.IsNullOrWhiteSpace(albumModel.Artist.ArtistName))
                    {
                        TempData["messageArtist"] = "You must enter an artist. Try again.";
                        return View();
                    }



                    TextInfo caseSwitch = new CultureInfo("en-US", false).TextInfo;

                    albumModel.AlbumName = albumModel.AlbumName.ToLower();
                    albumModel.AlbumName = caseSwitch.ToTitleCase(albumModel.AlbumName);

                    albumModel.Artist.ArtistName = albumModel.Artist.ArtistName.ToLower();
                    albumModel.Artist.ArtistName = caseSwitch.ToTitleCase(albumModel.Artist.ArtistName);

                    var albumToUpdate = await _context.Albums
                        .Include(a => a.Artist)
                        .ThenInclude(s => s.Subgenre)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.AlbumID == id);

                    if (albumModel.AlbumName.ToLower() != albumToUpdate.AlbumName.ToLower())
                    {
                        albumModel.AlbumID = id;
                    }

                    var duplicateCheck = _context.Albums
                        .Include(a => a.Artist)
                        .AsNoTracking();

                    foreach (var d in duplicateCheck)
                    {
                        if (albumModel.AlbumName.ToLower() == d.AlbumName.ToLower())
                        {
                            if (albumModel.Artist.ArtistName.ToLower() == d.Artist.ArtistName.ToLower())
                            {
                                TempData["message"] = "That album already exists.";
                                return View();
                            }
                        }
                    }

                    var artistCheck = _context.Artists
                        .Include(s => s.Subgenre)
                        .AsNoTracking();

                    foreach (var a in artistCheck)
                    {
                        if (albumModel.Artist.ArtistName.ToLower() == a.ArtistName.ToLower())
                        {
                            albumModel.Artist.ArtistID = a.ArtistID;
                            albumModel.Artist.CountryOfOrigin = a.CountryOfOrigin;
                            albumModel.Artist.SubgenreID = a.SubgenreID;
                            albumModel.AlbumID = id;
                        }
                    }

                    if (!artistCheck.Any(a => a.ArtistName == albumModel.Artist.ArtistName))
                    {
                        TempData["message"] = "Oops! That artist has not been entered in DNBase yet. " +
                                              "Please add them below.";

                        return RedirectToAction("Create", "ArtistModels");
                    }

                    _context.Update(albumModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlbumModelExists(albumModel.AlbumID))
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

            return View(albumModel);
        }

        // GET: AlbumModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var albumModel = await _context.Albums
                .FirstOrDefaultAsync(m => m.AlbumID == id);

            if (albumModel.AlbumName.ToLower() == "unknown")
            {
                TempData["messageWillDeleteUnknownTracks"] = "CAUTION! All tracks within the Unknown section of this artist will also be deleted. Are you sure?";
            }
            else
            {
                TempData["messageWillDeleteTracks"] = "You will also be deleting all associated tracks.";
            }

            if (albumModel == null)
            {
                return NotFound();
            }

            return View(albumModel);
        }

        // POST: AlbumModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var albumModel = await _context.Albums.FindAsync(id);
            _context.Albums.Remove(albumModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlbumModelExists(int id)
        {
            return _context.Albums.Any(e => e.AlbumID == id);
        }
    }
}
