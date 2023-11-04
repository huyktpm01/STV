using Microsoft.Ajax.Utilities;
using STV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace STV.Controllers
{
    public class StoryController : Controller
    {
        // GET: Story
        dbSTVDataContext db = new dbSTVDataContext("Data Source=LAPTOP-4PHTMN7E;Initial Catalog=Nhom6;Integrated Security=True");
        [Route("Story/Truyen/{StoryID}")]
        public ActionResult Truyen(int StoryID)
        {
                Session["StoryID"] = StoryID;
                ViewBag.AuthorID = StoryID;
                var sach = db.Stories.SingleOrDefault(n => n.StoryID == StoryID);
                return View(sach);
        }
        [Route("Story/Truyen/{StoryID}/{ChapterID}")]
        public ActionResult Chapter(int ChapterID)
        {
            ViewBag.ChapterID = ChapterID;
            var chap = db.Chapters.SingleOrDefault(n => n.ChapterID == ChapterID);
            return View(chap);
        }
        public ActionResult DSChap(int StoryID)
        {
            var ds = from s in db.Chapters where s.StoryID == StoryID  select s;
            Session["StoryID"] = StoryID;
            ViewBag.SC = ds.Count();
            return PartialView(ds);
        }
    }
}