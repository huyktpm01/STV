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
using System.Web.UI;

namespace STV.Areas.Admin.Controllers
{
    public class QuanLyController : Controller
    {
        dbSTVDataContext db = new dbSTVDataContext("Data Source=LAPTOP-4PHTMN7E;Initial Catalog=Nhom6;Integrated Security=True");
        // GET: Admin/QuanLy
        public ActionResult Index(int? page)
        {
            ViewBag.Page = page;
            int iSize = 10;
            int iPageNum = (page ?? 1);
            var sach = from s in db.Stories where s.Status == 0 select s;
            return View(sach.ToPagedList(iPageNum, iSize));
            
        }
        public ActionResult BaoCao(int StoryID)
        {
           
            var ds = from s in db.Chapters where s.status == 3 select s;
           
            return View(ds);
        }
        public ActionResult DSBaoCao(int? page)
        {
            ViewBag.Page = page;
            int iSize = 10;
            int iPageNum = (page ?? 1);
            var ds = from s in db.Chapters where s.status == 3 select s;
  
            return View(ds.ToPagedList(iPageNum, iSize));
        }
        public ActionResult DuyetBC()
        {
            var ds = from s in db.Chapters where s.status == 3 select s;
            foreach (var s in ds)
            {
                s.status = 1;
            }
            db.SubmitChanges();
            return RedirectToAction("DSBaoCao");
        }
        public  ActionResult Duyet(int StoryID)
        {
            var sach = db.Stories.SingleOrDefault(n => n.StoryID == StoryID);
            sach.Status = 1;
            db.SubmitChanges();
            return RedirectToAction("Index");
        }
        public ActionResult KhongDuyet(int StoryID)
        {
            var sach = db.Stories.SingleOrDefault(n => n.StoryID == StoryID);
            sach.Status = 4;
            db.SubmitChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DSChap(int StoryID)
        {
            var dsc = db.Chapters.Where(n => n.StoryID == StoryID).Take(3).ToList();
            ViewBag.st = StoryID;
            return View(dsc);
        }
        public ActionResult RutTien(int MemberID)
        {
            var dsc = db.RutTiens.SingleOrDefault(n => n.MemberID == MemberID);
            dsc.status = 1;
            db.SubmitChanges();
            return RedirectToAction("RT");
        }
        public ActionResult RT(int? page)
        {
            ViewBag.Page = page;
            int iSize = 10;
            int iPageNum = (page ?? 1);
            var sach = from s in db.RutTiens where s.status == 0 select s;
            return View(sach.ToPagedList(iPageNum, iSize));

        }
        [HttpGet]
        public ActionResult NapTien()
        {
            return View();
        }
        [HttpPost]
        public ActionResult NapTien(FormCollection collection)
        {
            var UserName = collection["UserName"];
            var Num = collection["Num"];
            var mem = db.Members.SingleOrDefault(n => n.UserName == UserName);
            Recharge a = new Recharge();
            a.Deposit_date = DateTime.Now;
            a.contend = UserName;
            a.Money = Convert.ToInt32(Num);
            mem.Money += Convert.ToInt32(Num);
            db.SubmitChanges();
            return View();
        }
        public ActionResult DoanhThu()
        {
            return View();
        }
        public ActionResult Rut(int? page)
        {
                    ViewBag.Page = page;
                    int iSize = 10;
                    int iPageNum = (page ?? 1);
                    var sach = from s in db.RutTiens where s.status == 1 select s;
                    return PartialView(sach.ToPagedList(iPageNum, iSize));

        }
        public ActionResult Nap(int? page)
        {
            ViewBag.Page = page;
            int iSize = 10;
            int iPageNum = (page ?? 1);
            var sach = from s in db.Recharges select s;
            return PartialView(sach.ToPagedList(iPageNum, iSize));

        }

    }
}