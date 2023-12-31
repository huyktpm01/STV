﻿using System;
using System.Collections.Generic;
using System.Linq;
using STV.Models;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.UI.WebControls;
using System.Runtime.Serialization.Formatters.Binary;
using Antlr.Runtime.Tree;
using System.Drawing;
using System.Xml;
using System.Diagnostics.Eventing.Reader;

namespace STV.Controllers
{
    public class MemberController : Controller
    {
        // GET: Member
        //dbSTVDataContext db = new dbSTVDataContext("Data Source=LAPTOP010502\\SQLEXPRESS;Initial Catalog=Nhom6;Integrated Security=True");
        dbSTVDataContext db = new dbSTVDataContext("Data Source=LAPTOP-4PHTMN7E;Initial Catalog=Nhom6;Integrated Security=True");
        // GET: User
        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }
        public ActionResult Follow(int id)
        {
            var tr = db.Readers.SingleOrDefault(n => n.MemberID == id);
            var ds = from s in db.Follows where s.ReaderID == tr.ReaderID select s;
            return View(ds);
        }
        public ActionResult truyenfollow(int id)
        {
            var tr = db.Stories.SingleOrDefault(n => n.StoryID == id);
            return PartialView(tr);
        }
        [HttpPost]
        public ActionResult DangKy(FormCollection collection, Member kh, HttpPostedFileBase fFileUpload)
        {
            var sFileName = Path.GetFileName(fFileUpload.FileName);
            var path = Path.Combine(Server.MapPath("~/Image"), sFileName);
            if (!System.IO.File.Exists(path))
            {
                fFileUpload.SaveAs(path);
            }
            var UserName = collection["UserName"];
            var Pass = collection["Pass"];
            var PassNL = collection["PassNL"];
            var Gender = collection["Gender"];
            var A_Level = collection["A_Level"];
            var Avatar = sFileName;
            var Email = collection["Email"];
 
            var Money = 0;
            var Status = String.Format("{0:dd/MM/yyyy}", DateTime.Now);
            var Joindate = String.Format("{0:dd/MM/yyyy}", DateTime.Now);
            var Birth = String.Format("{0:dd/MM/yyyy}", collection["Birth"]);
            if (String.IsNullOrEmpty(UserName))
            {
                ViewBag.ThongBao = "UserName không được rỗng";
            }
            else if (String.IsNullOrEmpty(Pass))
            {
                ViewBag.ThongBao = "Pass không được rỗng";
            }
            else if (String.IsNullOrEmpty(PassNL))
            {
                ViewBag.ThongBao = "Phải nhập lại mật khẩu";
            }
            else if (Pass != PassNL)
            {
                ViewBag.ThongBao = "Mật khẩu không khớp";
            }
            else if (String.IsNullOrEmpty(Email))
            {
                ViewBag.ThongBao = "Email không được rỗng";
            }
            else if (String.IsNullOrEmpty(Gender))
            {
                ViewBag.ThongBao = "Giới Tính không được rỗng";
            }
            else if (db.Members.Any(n => n.UserName == UserName))
            {
                ViewBag.ThongBao = "Tên đăng nhập đã tồn tại";
            }
            else if (db.Members.Any(n => n.Email == Email))
            {
                ViewBag.ThongBao = "Email đã được sử dụng";
            }
            else
            {
                kh.UserName = UserName;
                kh.Password = Pass;
                kh.Gender = Gender;
                kh.Email = Email;
                kh.Birth = DateTime.Parse(Birth);
                kh.Joindate = DateTime.Parse(Joindate);
                kh.Status = 1;
                kh.Money = 0;
                kh.Avatar = Avatar;
                kh.A_Level = 1;
                db.Members.InsertOnSubmit(kh);
               
                db.SubmitChanges();
               
                    Reader reader = new Reader();
                    var n = db.Members.SingleOrDefault(a => a.UserName == UserName);
                    reader.MemberID = n.MemberID;
                    reader.Name = n.UserName;
                    db.Readers.InsertOnSubmit(reader);
                    db.SubmitChanges();
                
                
                    Author Author = new Author();
                    var m = db.Members.SingleOrDefault(a => a.UserName == UserName);
                    Author.MemberID = m.MemberID;
                    Author.Pen_Name = m.UserName;
                    db.Authors.InsertOnSubmit(Author);
                    db.SubmitChanges();
                return RedirectToAction("Login");
            }
            return this.DangKy();
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(String url, FormCollection collection)
        {

            var UserName = collection["UserName"];
            var Pass = collection["Pass"];
            if (String.IsNullOrEmpty(UserName))
            {
                ViewData["Err1"] = "Bạn chưa nhập tên đăng nhập";
            }
            else if (String.IsNullOrEmpty(Pass))
            {
                ViewData["Err2"] = "Phải nhập mật khẩu";
            }
            else
            {
                Member kh = db.Members.SingleOrDefault(n => n.UserName == UserName && n.Password == Pass);
               
                if (kh != null)
                {
                    
                    ViewBag.ThongBao = "Chúc mừng đăng nhập thành công";
                    Session["TaiKhoan"] = kh;
                    Session["MemberID"] = kh.MemberID;
             
                    Reader a = db.Readers.SingleOrDefault(n => n.MemberID == kh.MemberID);
                    ReaderConfig config = db.ReaderConfigs.SingleOrDefault(n => n.ReaderID == a.ReaderID);
                    if(config != null)
                    {
                        Session["Nen"] = config.Color_Theme;
                        Session["Mau"] = config.Color_Word;
                        Session["Font"] = config.Style_Word;
                        Session["Line"] = config.Line;
                        Session["Size"] = config.Size_Word;
                        Session["Avata"] = kh.Avatar;
                        Session["LV"] = Convert.ToInt32(kh.A_Level);
                    }
                   
                    return Redirect(@Url.Action("Index", "Home"));
                }
                else
                {
                    ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }

            }
            return View();

        }
        [HttpGet]
        public ActionResult RutTien()
        {
    
            return View();
        }
        [HttpPost]
        public ActionResult RutTien(FormCollection collection)
        {

            RutTien a = new RutTien();
            Member us = db.Members.SingleOrDefault(n => n.MemberID == Convert.ToInt32(Session["MemberID"]));
            if(us.Money < Convert.ToInt32(collection["tien"]))
            {
                ViewBag.TB = "Số Tài Khoảng không đủ";
                return View();
            }
            else
            {
                a.MemberID = Convert.ToInt32(Session["MemberID"]);
                a.SoTien = Convert.ToInt32(collection["tien"]);
                a.Contend = collection["stk"] + collection["ttk"] + us.UserName;
                a.status = 0;
                us.Money -= a.SoTien;
                db.RutTiens.InsertOnSubmit(a);
                db.SubmitChanges();
                return Redirect(@Url.Action("Index", "Home"));
            }
           



        }
        public ActionResult Information(int MemberID)
        {
            var memberID = db.Members.SingleOrDefault(n => n.MemberID == MemberID);
            return View(memberID);
        }
        public ActionResult HisBuy(int MemberID)
        {
            var member = db.Members.SingleOrDefault(n => n.MemberID == MemberID);
            return View(member);
        }
        public ActionResult DaB(int MemberID)
        {
            var ds = db.Comments.Count(n => n.Reader.MemberID == MemberID);
            ViewBag.Count1 = ds;
            return PartialView();
        }
        public ActionResult DaD(int MemberID)
        {
            var ds = db.Histories.Count(n => n.Reader.MemberID == MemberID);
            ViewBag.Count = ds;
            return PartialView();
        }
        public ActionResult BanS(int MemberID)
        {
            var member = db.Authors.SingleOrDefault(n => n.MemberID == MemberID);
            var ds = db.Withdraws.Where(n => n.AuthorID == member.AuthorID).OrderBy(n => n.Withdrawaldate).ToList();
            return PartialView(ds);
        }
        public ActionResult LSNap(int MemberID)
        {
            var member = db.Readers.SingleOrDefault(n => n.MemberID == MemberID);
            var ds = db.Recharges.Where(n => n.ReaderID == member.ReaderID).OrderBy(n => n.Deposit_date).ToList();
            return PartialView(ds);

        }
        public ActionResult LSRut(int MemberID)
        {
            var ds = db.RutTiens.Where(n => n.MemberID == MemberID && n.status == 1);
            return PartialView(ds);

        }
        public ActionResult Recharge()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Edit(HttpPostedFileBase f)
        {
            var sFileName = Path.GetFileName(f.FileName);
            var path = Path.Combine(Server.MapPath("~/Image"), sFileName);
            if (!System.IO.File.Exists(path))
            {
                f.SaveAs(path);
            }
            var member = db.Members.SingleOrDefault(n => n.MemberID == Convert.ToInt32( Session["MemberID"]));
                member.Avatar = sFileName;
            db.SubmitChanges();
            return RedirectToAction("Information", new {MemberID = Convert.ToInt32(Session["MemberID"] )});
        }
        public ActionResult BuyChap(int MemberID)
        {
            var member = db.Readers.SingleOrDefault(n => n.MemberID == MemberID);
            var ds = db.HistoryBuys.Where(n => n.ReaderID == member.ReaderID).OrderBy(n => n.BuyDay).ToList();
            return PartialView(ds);
        }
        public ActionResult DangXuat(String url)
        {
            Session["TaiKhoan"] = null;
            Session["MemberID"] = null;
            Session["Nen"] = null;
            Session["Mau"] = null;
            Session["Font"] = null;
            Session["Line"] = null;
            Session["Size"] = null;
            return Redirect(url);
        }
    }
}
