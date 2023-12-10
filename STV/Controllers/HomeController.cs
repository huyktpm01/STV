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
            if (Session["TaiKhoan"] != null)
            {
                var kh = (STV.Models.Member)Session["TaiKhoan"];
                int sl = 0;
                var dsfl = db.Follows.Where(n => n.Reader.MemberID == kh.MemberID).ToList();
                foreach (var i in dsfl)
                {
                    Story x = db.Stories.SingleOrDefault(n => n.StoryID == i.StoryID);
                    if (i.FollowDate < x.LastUpdate)
                    {
                        sl++;

                    }
                    Session["SLF"] = sl;
                }
            }    
            return View();
        }
        public ActionResult User() 
        {

            return PartialView();
        }
        public ActionResult NewBook()
        {

            return PartialView(db.Stories.Where(n => n.Status != 4 && n.Status != 0).OrderByDescending(a => a.Publishdate).Take(12).ToList());
        }
        public ActionResult TopBook()
        {

            return PartialView(db.Stories.Where(n => n.Status != 4 && n.Status != 0).OrderByDescending(a => a.Rating).Take(12).ToList());
        }
        public ActionResult NewUpdate()
        {

            return PartialView(db.Stories.Where(n => n.Status != 4 && n.Status != 0).OrderByDescending(a => a.LastUpdate).Take(20).ToList());
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
        public ActionResult FollowTr(int id)
        {
            var ds = db.Follows.Where(n => n.Reader.MemberID == id).ToList();
            FollowDate(id);
            return PartialView(ds);
        }
        public ActionResult FollowDate(int id)
        {
            var kh = (STV.Models.Member)Session["TaiKhoan"];
            var ds = db.Follows.Where(n => n.Reader.MemberID == id).ToList();
            var dsfl = db.Follows.Where(n => n.Reader.MemberID == kh.MemberID).ToList();
            foreach (var i in dsfl)
            {
                Story x = db.Stories.SingleOrDefault(n => n.StoryID == i.StoryID);
                i.FollowDate = x.LastUpdate;
                Session["SLF"] = 0;
            }
            db.SubmitChanges();
            return RedirectToAction("Index");
        }


    }
}