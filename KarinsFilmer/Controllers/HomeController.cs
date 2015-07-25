using KarinsFilmer.CouchDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace KarinsFilmer.Controllers
{
    [Authorize]
    [RequireHttps]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var rep = new CouchRepository();
            rep.AddMovieRating(new CouchDb.Entities.MovieRating
            {
                AccountId = "staffan.ekvall@gmail.com",
                MovieId = "ttte122",
                Rating = 5
            });
            return View();
        }
    }
}
