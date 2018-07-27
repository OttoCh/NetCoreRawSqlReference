using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RawSql.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RawSql.Controllers
{
    public class AlbumController : Controller
    {
        public async Task<IActionResult> Index()
        {
            Context context = HttpContext.RequestServices.GetService(typeof(Models.Context)) as Context;
            return View(await context.GetAllAlbums());
        }

        public async Task<IActionResult> Details(int id)
        {
            Context context = HttpContext.RequestServices.GetService(typeof(Models.Context)) as Context;

            return View(await context.GetAlbum(id));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Album album)
        {
            Context context = HttpContext.RequestServices.GetService(typeof(Models.Context)) as Context;
            await context.CreateAlbum(album);
            return RedirectToAction(nameof(Index));
        }

    }
}
