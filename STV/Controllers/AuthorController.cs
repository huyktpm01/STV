using System;
using System.Collections.Generic;
using System.Linq;
using STV.Models;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.UI.WebControls;
using System.Runtime.Serialization.Formatters.Binary;

namespace STV.Controllers
{
    public class AuthorController : Controller
    {
        dbSTVDataContext db = new dbSTVDataContext("Data Source=LAPTOP-4PHTMN7E;Initial Catalog=Nhom6;Integrated Security=True");
        // GET: Author
        public ActionResult list(int AuthorID, int ? page)
        {
            ViewBag.AuthorID = AuthorID;
            int iSize = 6;
            int iPageNum = (page ?? 1);
            var sach = from s in db.Stories where s.AuthorID == AuthorID select s;
            return View();
        }
        [HttpGet] 
        public ActionResult addBook() 
        {
            ViewBag.CatID = new SelectList(db.categories.ToList().OrderBy(n => n.CatName), "CatID", "CatName");
            return View();
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
                    kh.image = path;
                    db.Stories.InsertOnSubmit(kh);
                    db.SubmitChanges();
                    return RedirectToAction("list","Author");
                }
                return View();
            }
        }
    }
}