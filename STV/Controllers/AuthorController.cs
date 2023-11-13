using System;
using System.Collections.Generic;
using System.Linq;
using STV.Models;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.UI.WebControls;
using System.Runtime.Serialization.Formatters.Binary;
using PagedList;
using Microsoft.Ajax.Utilities;

namespace STV.Controllers
{
    public class AuthorController : Controller
    {
        dbSTVDataContext db = new dbSTVDataContext("Data Source=LAPTOP-4PHTMN7E;Initial Catalog=Nhom6;Integrated Security=True");
        // GET: Author
        public ActionResult list(int MemberID, int ? page)
        {
            
            
            ViewBag.AuthorID = MemberID;
            ViewBag.Page = page;
            int iSize = 6;
            int iPageNum = (page ?? 1);
            var sach = from s in db.Stories where s.Author.MemberID == MemberID select s;
            return View(sach.ToPagedList(MemberID,iSize));
        }
        public ActionResult listChap(int StoryID, int? page)
        {
            Session["StoryID"] = StoryID;
            var sach = db.Stories.SingleOrDefault(n => n.StoryID == StoryID);
            ViewBag.TenSach = sach.Title; 
            int iSize = 50;
            int iPageNum = (page ?? 1);
            var chuong = db.Chapters.Where(n => n.StoryID == StoryID).ToList();
            return View(chuong.ToPagedList(iPageNum,iSize));
        }
        [HttpGet]
        public ActionResult EditBook(int StoryID)
        {
            var sach = db.Stories.SingleOrDefault(n => n.StoryID == StoryID);
            if (sach == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ViewBag.CatID = new SelectList(db.categories.ToList().OrderBy(s => s.CatName), "CatID", "CatName", sach.CatID);

            return View(sach);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditBook(FormCollection collection, HttpPostedFileBase fFileUpload)
        {
            var sach = db.Stories.SingleOrDefault(n => n.StoryID == int.Parse(collection["StoryID"]));
            ViewBag.CatID = new SelectList(db.categories.ToList().OrderBy(s => s.CatName), "CatID", "CatName", int.Parse(collection["CatID"]));

            if (ModelState.IsValid)
            {
                var sFileName = Path.GetFileName(fFileUpload.FileName);
                var path = Path.Combine(Server.MapPath("~/Image"), sFileName);
                if (!System.IO.File.Exists(path))
                {
                    fFileUpload.SaveAs(path);
                }
                sach.Title = collection["ip-name"];
                var ttg = collection["ip-author"];
                var n = db.Authors.SingleOrDefault(a => a.Pen_Name == ttg);
                sach.AuthorID = n.AuthorID;
                sach.Description = collection["ip-description"];
                sach.image = path;
                sach.LastUpdate = DateTime.Now;     
                db.SubmitChanges();
            }
            return View(sach);
        }
        [HttpGet] 
        public ActionResult addBook() 
        {
            
            ViewBag.CatID = new SelectList(db.categories.ToList().OrderBy(n => n.CatName), "CatID", "CatName");
            return View();
        }
        public ActionResult BatVIP(int chapternumber)
        {
            var chap = db.Chapters.Where(n => n.StoryID == Convert.ToInt16( Session["StoryID"]) && n.Chapter_Number >= chapternumber ).ToList();
            foreach(var i in chap)
            {
                i.Vip = true;
            }
            db.SubmitChanges();
            return RedirectToAction("listChap");

        }
        public ActionResult TatVIP(int chapternumber)
        {
            var chap = db.Chapters.SingleOrDefault(n => n.StoryID == Convert.ToInt16(Session["StoryID"]) && n.Chapter_Number == chapternumber);
            chap.Vip = false;
            db.SubmitChanges();
            return RedirectToAction("listChap");

        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult addBook(FormCollection collection, Story kh, HttpPostedFileBase fFileUpload)
        {
            ViewBag.CatID = new SelectList(db.categories.ToList().OrderBy(s => s.CatName), "CatID", "CatName");
            if (fFileUpload == null)
            {
                ViewBag.thongbao = "Phải chọn Hình ảnh";
                var Name = collection["ip-name"];
                var ttg = collection["ip-author"];
                var n = db.Authors.SingleOrDefault(tg => tg.Pen_Name == ttg);
                var mtg = n.MemberID;
                var gioithieu = collection["ip-description"];
                ViewBag.CatID = new SelectList(db.categories.ToList().OrderBy(s => s.CatName), "CatID", "CatName", int.Parse(collection["CatID"]));
                if (String.IsNullOrEmpty(Name))
                {
                    ViewData["err1"] = "Tên truyện không được rỗng";
                }
                else if (String.IsNullOrEmpty(mtg.ToString()))
                {
                    ViewData["err2"] = "tác giả không được để rỗng";
                }
                else if (String.IsNullOrEmpty(ttg))
                {
                    ViewData["err2"] = "tác giả không được để rỗng";
                }
                else if (String.IsNullOrEmpty(gioithieu))
                {
                    ViewData["err4"] = "Phải nhập giới thiệu";
                }
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var sFileName = Path.GetFileName(fFileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Image"), sFileName);
                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);
                    }
                    kh.Title = collection["ip-name"];
                    var ttg = collection["ip-author"];
                    var n = db.Authors.SingleOrDefault(a => a.Pen_Name == ttg);
                    kh.AuthorID = n.AuthorID;
                    kh.Description = collection["ip-description"];
                    kh.image = sFileName;
                    kh.N_O_Chapter = 0;
                    kh.View = 0;
                    kh.Rating = 0;
                    kh.Status = 0;
                    kh.Vip = false;
                    kh.hot = false;
                    kh.Publishdate = DateTime.Now;
                    kh.LastUpdate   = DateTime.Now;
                    db.Stories.InsertOnSubmit(kh);
                    db.SubmitChanges();
                    return RedirectToAction("list", "Author", new { MemberID = Session["MemberID"] });
                }
                return View();
            }
        }

        public ActionResult addChapter(int id)
        {
            Session["StoryID"] = id;
            var sach = db.Stories.SingleOrDefault(n => n.StoryID == id);
            var chap = db.Chapters.Where(n => n.StoryID == id).ToList();
            if(chap.Max(c => c.Chapter_Number) == null)
            {
                ViewBag.Max = 1;
            }
            else
            {
                 ViewBag.Max = chap.Max(c => c.Chapter_Number) + 1 ;
            }
           
            ViewBag.TenSach = sach.Title;
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult addChapter(FormCollection collection, Chapter kh)
        {

            if (ModelState.IsValid)
            {

                kh.Title = collection["ip-name"];
                kh.StoryID = int.Parse(Session["StoryID"].ToString());
                kh.Chapter_Number = int.Parse(collection["ip-num"]);
                kh.Content = collection["ip-content"];
                kh.N_O_W = collection["ip-content"].Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length.ToString();
                kh.View = 0;
                kh.Rating = 0;
                kh.Pushlishdate = DateTime.Now;
                kh.status = 0;
                kh.LastUpdate = DateTime.Now;
                kh.Vip = bool.Parse("False");
                kh.Money = 1000;
                kh.sold = 0;
                db.Chapters.InsertOnSubmit(kh);
                db.SubmitChanges();
                return RedirectToAction("listChap", "Author", new { StoryID = Session["StoryID"] });
                }
                return View();
            
        }
    }
}