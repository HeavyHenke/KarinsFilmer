﻿using KarinsFilmer.CouchDb;
using KarinsFilmer.CouchDb.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace KarinsFilmer.Controllers
{
    //[Authorize]
    [RequireHttps]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
