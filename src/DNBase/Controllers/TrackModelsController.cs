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
using static System.Net.Mime.MediaTypeNames;

namespace DNBase.Controllers
{
    public class TrackModelsController : Controller
    {
        private readonly DNBaseContext _context;

        public TrackModelsController(DNBaseContext context)
        {
            _context = context;
        }

        // GET: TrackModels
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSort"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["AlbumSort"] = sortOrder == "Album" ? "album_desc" : "Album";
            ViewData["ArtistSort"] = sortOrder == "Artist" ? "artist_desc" : "Artist";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var tracks = from t in _context.Tracks
                         .Include(a => a.Album)
                         .ThenInclude(r => r.Artist)
                         select t;

            if (!String.IsNullOrEmpty(searchString))
            {
                tracks = tracks.Where(t => t.TrackName.Contains(searchString) ||
                                      t.Album.AlbumName.Contains(searchString) ||
                                      t.Album.Artist.ArtistName.Contains(searchString));

                if (!tracks.Any(t => t.TrackName.Contains(searchString) ||
                                      t.Album.AlbumName.Contains(searchString) ||
                                      t.Album.Artist.ArtistName.Contains(searchString)))
                {
                    TempData["messageNonFound"] = "Oops! There is nothing here. Try another search.";
                }
            }

            switch (sortOrder)
            {
                case "name_desc":
                    tracks = tracks.OrderByDescending(t => t.TrackName);
                    break;
                case "Album":
                    tracks = tracks.OrderBy(t => t.Album.AlbumName);
                    break;
                case "album_desc":
                    tracks = tracks.OrderByDescending(t => t.Album.AlbumName);
                    break;
                case "Artist":
                    tracks = tracks.OrderBy(t => t.Album.Artist.ArtistName);
                    break;
                case "artist_desc":
                    tracks = tracks.OrderByDescending(t => t.Album.Artist.ArtistName);
                    break;
                default:
                    tracks = tracks.OrderBy(t => t.TrackName);
                    break;
            }

            int pageSize = 8;

            return View(await PaginatedList<TrackModel>.CreateAsync(tracks.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: TrackModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trackModel = await _context.Tracks
                .Include(a => a.Album)
                .ThenInclude(t => t.Artist)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.TrackID == id);

            if (trackModel == null)
            {
                return NotFound();
            }

            return View(trackModel);
        }

        // GET: TrackModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TrackModels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TrackName,Album,Artist")] TrackModel trackModel)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(trackModel.TrackName))
                {
                    TempData["messageTrackName"] = "You must enter a track name. Try again.";
                    return View();
                }

                if (String.IsNullOrWhiteSpace(trackModel.Album.AlbumName))
                {
                    TempData["messageAlbumName"] = "You must enter an album name. If its a single, please write 'Single'. " +
                        "If it is unknown, please write 'Unknown'";
                    return View();
                }

                if (String.IsNullOrWhiteSpace(trackModel.Album.Artist.ArtistName))
                {
                    TempData["messageArtistName"] = "You must enter the artists name. " +
                        "Let's give them some credit here.";
                    return View();
                }

                TextInfo caseSwitch = new CultureInfo("en-us", false).TextInfo;

                trackModel.TrackName = trackModel.TrackName.ToLower();
                trackModel.TrackName = caseSwitch.ToTitleCase(trackModel.TrackName);

                trackModel.Album.AlbumName = trackModel.Album.AlbumName.ToLower();
                trackModel.Album.AlbumName = caseSwitch.ToTitleCase(trackModel.Album.AlbumName);

                trackModel.Album.Artist.ArtistName = trackModel.Album.Artist.ArtistName.ToLower();
                trackModel.Album.Artist.ArtistName = caseSwitch.ToTitleCase(trackModel.Album.Artist.ArtistName);

                if (trackModel.Album.AlbumName.ToLower() == "single" || trackModel.Album.AlbumName.ToLower() == "singles")
                {
                    trackModel.Album.AlbumName = "Singles";
                }

                var trackDuplicate = _context.Tracks
                    .Include(a => a.Album)
                    .ThenInclude(t => t.Artist)
                    .AsNoTracking();
                
                foreach (var t in trackDuplicate)
                {
                    if (trackModel.TrackName.ToLower() == t.TrackName.ToLower())
                    {
                        if (trackModel.Album.AlbumName.ToLower() == t.Album.AlbumName.ToLower())
                        {
                            if (trackModel.Album.Artist.ArtistName.ToLower() == t.Album.Artist.ArtistName.ToLower())
                            {
                                TempData["messageExists"] = "That track already exists";
                                return View();
                            }
                        }
                    }
                }

                var checkArtists = _context.Artists
                    .Include(a => a.Albums)
                    .AsNoTracking();

                if (!checkArtists.Any(c => c.ArtistName == trackModel.Album.Artist.ArtistName))
                {
                    TempData["message"] = "Oops! That artist has not been entered in DNBase yet. " +
                        "Please add them below.";
                    return RedirectToAction("Create", "ArtistModels");
                }

                ArtistModel existingArtist = _context.Artists.SingleOrDefault(a => a.ArtistName == trackModel.Album.Artist.ArtistName);

                trackModel.Album.Artist = existingArtist;

                var checkAlbums = _context.Albums
                    .Include(a => a.Artist)
                    .AsNoTracking();

                foreach (var check in checkAlbums)
                {
                    if (trackModel.Album.AlbumName.ToLower() == check.AlbumName.ToLower())
                    {
                        AlbumModel existingAlbum = _context.Albums.SingleOrDefault(a => a.AlbumID == check.AlbumID);

                        if (existingAlbum.ArtistID == existingArtist.ArtistID)
                        {
                            trackModel.Album = existingAlbum;
                        }
                    }
                }

                if (!checkAlbums.Any(c => c.AlbumName == trackModel.Album.AlbumName))
                {
                    
                    AlbumModel newAlbum = new AlbumModel()
                    {
                        AlbumName = trackModel.Album.AlbumName
                    };
                }

                _context.Add(trackModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(trackModel);
        }

        // GET: TrackModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return View();
            }

            var trackModel = await _context.Tracks
                .Include(a => a.Album)
                .ThenInclude(b => b.Artist)
                .ThenInclude(s => s.Subgenre)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.TrackID == id);
            
            if (trackModel == null)
            {
                return View();
            }
            return View(trackModel);
        }

        // POST: TrackModels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TrackName,Album")] TrackModel trackModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(trackModel.TrackName))
                    {
                        TempData["messageTrackName"] = "You must enter a track name. Try again.";
                        return View();
                    }

                    if (String.IsNullOrWhiteSpace(trackModel.Album.AlbumName))
                    {
                        TempData["messageAlbumName"] = "You must enter an album name. If its a single, please write 'Single'. " +
                            "If it is unknown, please write 'Unknown'";
                        return View();
                    }

                    if (String.IsNullOrWhiteSpace(trackModel.Album.Artist.ArtistName))
                    {
                        TempData["messageArtistName"] = "You must enter the artists name. " +
                            "Let's give them some credit here.";
                        return View();
                    }

                    TextInfo caseSwitch = new CultureInfo("en-US", false).TextInfo;

                    trackModel.TrackName = trackModel.TrackName.ToLower();
                    trackModel.TrackName = caseSwitch.ToTitleCase(trackModel.TrackName);

                    trackModel.Album.AlbumName = trackModel.Album.AlbumName.ToLower();
                    trackModel.Album.AlbumName = caseSwitch.ToTitleCase(trackModel.Album.AlbumName);

                    trackModel.Album.Artist.ArtistName = trackModel.Album.Artist.ArtistName.ToLower();
                    trackModel.Album.Artist.ArtistName = caseSwitch.ToTitleCase(trackModel.Album.Artist.ArtistName);

                    if (trackModel.Album.AlbumName.ToLower() == "single" || trackModel.Album.AlbumName.ToLower() == "singles")
                    {
                        trackModel.Album.AlbumName = "Singles";
                    }

                    var trackToUpdate = await _context.Tracks
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.TrackID == id);

                    if (trackModel.TrackName.ToLower() != trackToUpdate.TrackName.ToLower())
                    {
                        trackModel.TrackID = id;
                    }

                    var duplicateCheck = _context.Tracks
                        .Include(a => a.Album)
                        .ThenInclude(b => b.Artist)
                        .AsNoTracking();

                    foreach (var d in duplicateCheck)
                    {
                        if (trackModel.TrackName.ToLower() == d.TrackName.ToLower())
                        {
                            if (trackModel.Album.AlbumName.ToLower() == d.Album.AlbumName.ToLower())
                            {
                                if (trackModel.Album.Artist.ArtistName.ToLower() == d.Album.Artist.ArtistName.ToLower())
                                {
                                    TempData["message"] = "That track already exists.";
                                    return View();
                                }
                            }
                        }
                    }

                    var albumOrArtistExistsCheck = _context.Artists
                        .Include(a => a.Albums)
                        .Include(s => s.Subgenre)
                        .AsNoTracking();

                    foreach (var a in albumOrArtistExistsCheck)
                    {
                        if (trackModel.Album.Artist.ArtistName.ToLower() == a.ArtistName.ToLower())
                        {
                            trackModel.Album.Artist.ArtistID = a.ArtistID;
                            trackModel.Album.Artist.CountryOfOrigin = a.CountryOfOrigin;
                            trackModel.Album.Artist.SubgenreID = a.SubgenreID;
                            trackModel.TrackID = id;
                        }
                    }

                    if (!albumOrArtistExistsCheck.Any(a => a.ArtistName == trackModel.Album.Artist.ArtistName))
                    {
                        TempData["message"] = "Oops! That artist has not been entered in DNBase yet. " +
                                                  "Please add them below.";

                        return RedirectToAction("Create", "ArtistModels");
                    }

                    ArtistModel existingArtist = _context.Artists.SingleOrDefault(a => a.ArtistName == trackModel.Album.Artist.ArtistName);

                    trackModel.Album.Artist = existingArtist;

                    var checkAlbums = _context.Albums
                        .Include(a => a.Artist)
                        .AsNoTracking();

                    foreach (var check in checkAlbums)
                    {
                        if (trackModel.Album.AlbumName.ToLower() == check.AlbumName.ToLower())
                        {
                            AlbumModel existingAlbum = _context.Albums.SingleOrDefault(a => a.AlbumID == check.AlbumID);

                            if (existingAlbum.ArtistID == existingArtist.ArtistID)
                            {
                                trackModel.Album = existingAlbum;
                            }
                        }
                    }

                    if (!checkAlbums.Any(c => c.AlbumName == trackModel.Album.AlbumName))
                    {

                        AlbumModel newAlbum = new AlbumModel()
                        {
                            AlbumName = trackModel.Album.AlbumName
                        };
                    }

                    _context.Update(trackModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrackModelExists(trackModel.TrackID))
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
            return View(trackModel);
        }

        // GET: TrackModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trackModel = await _context.Tracks
                .FirstOrDefaultAsync(m => m.TrackID == id);
            if (trackModel == null)
            {
                return NotFound();
            }

            return View(trackModel);
        }

        // POST: TrackModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trackModel = await _context.Tracks.FindAsync(id);
            _context.Tracks.Remove(trackModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrackModelExists(int id)
        {
            return _context.Tracks.Any(e => e.TrackID == id);
        }
    }
}
