﻿using System;
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
    public class MemberController : Controller
    {
        // GET: Member
        dbSTVDataContext db = new dbSTVDataContext("Data Source=LAPTOP-4PHTMN7E;Initial Catalog=Nhom6;Integrated Security=True");
        // GET: User
        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
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
                ViewData["err1"] = "UserName không được rỗng";
            }
            else if (String.IsNullOrEmpty(Pass))
            {
                ViewData["err2"] = "Pass không được rỗng";
            }
            else if (String.IsNullOrEmpty(PassNL))
            {
                ViewData["err4"] = "Phải nhập lại mật khẩu";
            }
            else if (Pass != PassNL)
            {
                ViewData["err4"] = "Mật khẩu không khớp";
            }
            else if (String.IsNullOrEmpty(Email))
            {
                ViewData["err5"] = "Email không được rỗng";
            }
            else if (String.IsNullOrEmpty(Gender))
            {
                ViewData["err6"] = "Điện Thoại không được rỗng";
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
                kh.A_Level = int.Parse(A_Level);
                kh.Status = 1;
                kh.Money = Money;
                db.Members.InsertOnSubmit(kh);
                db.SubmitChanges();
                if(kh.A_Level == 1)
                {
                    Reader reader = new Reader();
                    var n = db.Members.SingleOrDefault(a => a.UserName == UserName);
                    reader.MemberID = n.MemberID;
                    reader.Name = n.UserName;
                    db.Readers.InsertOnSubmit(reader);
                    db.SubmitChanges();
                }
                else if(kh.A_Level == 2)
                {
                    Author Author = new Author();
                    var n = db.Members.SingleOrDefault(a => a.UserName == UserName);
                    Author.MemberID = n.MemberID;
                    Author.Pen_Name = n.UserName;
                    db.Authors.InsertOnSubmit(Author);
                    db.SubmitChanges();
                }
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
                    return Redirect(@Url.Action("Index", "Home"));
                }
                else
                {
                    ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }

            }
            return View();

        }
    }
}