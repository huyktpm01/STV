using STV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace STV.Controllers
{
    public class HomeController : Controller
    {
        dbSTVDataContext db = new dbSTVDataContext("Data Source=LAPTOP-4PHTMN7E;Initial Catalog=Nhom6;Integrated Security=True");
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult User() 
        {
           
            return PartialView();
        }
        public ActionResult NewBook()
        {

            return PartialView(db.Stories.OrderByDescending(a => a.Publishdate).Take(12).ToList());
        }



    }
}