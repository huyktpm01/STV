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
        public ActionResult list()
        {
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
                var Name = collection["ip-name"];
                var ttg = collection["ip-author"];
                var n = db.Authors.SingleOrDefault(a => a.Pen_Name == ttg);
                var mtg = n.MemberID;
                var giothieu = collection["ip-description"];
                ViewBag.MaCD = new SelectList(db.categories.ToList().OrderBy(s => s.CatName), "MaCD", "CatName", int.Parse(collection["CatID"]));
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var sFileName = Path.GetFileName(fFileUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images"), sFileName);
                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);
                    }
                    kh.Title = collection["ip-name"];
                    var ttg = collection["ip-author"];
                    var n = db.Authors.SingleOrDefault(a => a.Pen_Name == ttg);
                    kh.AuthorID = n.MemberID;
                    kh.Description = collection["ip-description"];
                    kh.i
                    db.Stories.InsertOnSubmit(kh);
                    db.SubmitChanges();
                    return RedirectToAction("Index");
                }
                return View();
            }
            if (String.IsNullOrEmpty(Name))
            {
                ViewData["err1"] = "UserName không được rỗng";
            }
            else if (String.IsNullOrEmpty(ttg))
            {
                ViewData["err2"] = "Pass không được rỗng";
            }
            else if (String.IsNullOrEmpty(PassNL))
            {
                ViewData["err4"] = "Phải nhập lại mật khẩu";
            }
            return View();
        }
    }
}