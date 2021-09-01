using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HyMovieRental.Controllers
{
    public class RentalsController : Controller
    {
        public ActionResult New()
        {
            return View();
        }
    }
}