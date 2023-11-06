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
        public ActionResult TopBook()
        {

            return PartialView(db.Stories.OrderByDescending(a => a.Rating).Take(12).ToList());
        }
        public ActionResult NewUpdate()
        {

            return PartialView(db.Stories.OrderByDescending(a => a.LastUpdate).Take(20).ToList());
        }
        public ActionResult History()
        {
            var b = db.Readers.SingleOrDefault(n => n.MemberID == Convert.ToInt16(Session["MemberID"]));
            if(b == null)
            {
                return PartialView(null);
            }
            else
            {
                return PartialView(db.Histories.Where(a => a.ReaderID == b.ReaderID).OrderByDescending(a => a.E_Time).Take(20).ToList());
            }
           
           
        }


    }
}