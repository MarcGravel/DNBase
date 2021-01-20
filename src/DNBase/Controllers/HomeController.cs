using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DNBase.Models;
using DNBase.Data;

namespace DNBase.Controllers
{
    public class HomeController : Controller
    {
        private readonly DNBaseContext _context;

        public HomeController(DNBaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Button(string button)
        {
            var artists = _context.Artists;
            var artistIds = artists.Select(a => a.ArtistID).ToList();
            Random randomArtistNumber = new Random();
            int randomArtistID = randomArtistNumber.Next(1, artistIds.Count);

            if (button.Equals("btnArtist"))
            {
                return RedirectToAction("Details", "ArtistModels", new { id = randomArtistID.ToString() });
            }
            
            var tracks = _context.Tracks;
            var trackIds = tracks.Select(t => t.TrackID).ToList();
            Random randomTrackNumber = new Random();
            int randomTrackID = randomTrackNumber.Next(1, trackIds.Count);

            if (button.Equals("btnTrack"))
            {
                return RedirectToAction("Details", "TrackModels", new { id = randomTrackID.ToString() });
            }
            else
            {
                return View(nameof(Index));
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
