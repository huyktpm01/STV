using Microsoft.Ajax.Utilities;
using STV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;

namespace STV.Controllers
{
    public class StoryController : Controller
    {
        // GET: Story
        //dbSTVDataContext db = new dbSTVDataContext("Data Source=LAPTOP010502\\SQLEXPRESS;Initial Catalog=Nhom6;Integrated Security=True");
        dbSTVDataContext db = new dbSTVDataContext("Data Source=LAPTOP-4PHTMN7E;Initial Catalog=Nhom6;Integrated Security=True");
        int views = 0;
        [Route("Story/Truyen/{StoryID}")]
        public ActionResult Truyen(int StoryID)
        {
                Session["StoryID"] = StoryID;
                ViewBag.AuthorID = StoryID;

                var sach = db.Stories.SingleOrDefault(n => n.StoryID == StoryID);
                var ds = from s in db.Chapters where s.StoryID == sach.StoryID select s;
                
                foreach(var i in ds)
                {
                   views = views + Convert.ToInt32(i.View);
                }
                sach.View = Convert.ToInt32(views);
                return View(sach);
        }
        
        public ActionResult Muachuong(int chapterid)
        {
            var user = db.Readers.FirstOrDefault(r => r.MemberID == int.Parse(Session["MemberID"].ToString()));
            var chap = db.Chapters.FirstOrDefault(r => r.ChapterID == chapterid);
            var mau = db.Authors.FirstOrDefault(r => r.MemberID == chap.Story.Author.MemberID);
            var author = db.Members.FirstOrDefault(r => r.MemberID == chap.Story.Author.MemberID);
            float p = 0.9f;
            if (user.Member.Money >= chap.Money)
            {
                HistoryBuy a = new HistoryBuy();
                a.ChapterID = chapterid;
                a.ReaderID = user.ReaderID;
                a.BuyDay = DateTime.Now;
                a.Money = chap.Money;
                user.Member.Money =  user.Member.Money - Convert.ToInt16(chap.Money);
                Withdraw b = new Withdraw();
                b.AuthorID = mau.AuthorID;
                b.Money = Convert.ToInt16(chap.Money * p);
                author.Money += Convert.ToInt32(b.Money);
                b.Withdrawaldate = DateTime.Now; b.contend = "Mua Chuong cua truyen" + chap.Story.Title.ToString();
                db.Withdraws.InsertOnSubmit(b);
                db.HistoryBuys.InsertOnSubmit(a);
                db.SubmitChanges();
                return Content("true");
            }
            return Content("false");

        }
        public ActionResult BaoCao(int chapterid)
        {
            var chap = db.Chapters.SingleOrDefault(n => n.ChapterID == chapterid);
            chap.status = 3;
            db.SubmitChanges();
            return RedirectToAction("Truyen", "Story", new { StoryID = chap.StoryID });
        }
        public ActionResult luuconfig(String Nen, String Mau,String Font, String size, String line)
        {
            var Size = Convert.ToInt32(size);
            var Line = float.Parse(line);
            var user = db.Readers.FirstOrDefault(r => r.MemberID == Convert.ToInt32(Session["MemberID"]));
            var conf = db.ReaderConfigs.FirstOrDefault(r => r.ReaderID == user.ReaderID);
            if(conf == null)
            {
                ReaderConfig a = new ReaderConfig();
                a.ReaderID = user.ReaderID;
                a.Reader = user;
                a.Color_Theme = Nen;
                a.Color_Word = Mau;
                a.Line = Line+'f';
                a.Size_Word = Size;
                a.Style_Word = Font;
                Session["Nen"] = a.Color_Theme;
                Session["Mau"] = a.Color_Word;
                Session["Font"] = a.Style_Word;
                Session["Line"] = a.Line;
                Session["Size"] = a.Size_Word;
                db.ReaderConfigs.InsertOnSubmit(a);
            }
            else
            {
                ReaderConfig a = new ReaderConfig();
                a.Color_Theme = Nen; 
                a.Color_Word = Mau;
                a.Line = Line+'f';
                a.Size_Word = Size;
                a.Style_Word = Font;
                conf = a;
                Session["Nen"] = a.Color_Theme;
                Session["Mau"] = a.Color_Word;
                Session["Font"] = a.Style_Word;
                Session["Line"] = a.Line;
                Session["Size"] = a.Size_Word;

            }
            db.SubmitChanges();
            return null;

        }
        [HttpPost]
        public ActionResult Follow(int storyId)
        {
            if (Session["TaiKhoan"] == null)
            {
                return Redirect(Url.Action("Login", "Member", new { url = Request.Url.AbsolutePath.ToString() }));
            }
            else {
                var userId = db.Readers.FirstOrDefault(r => r.MemberID == int.Parse(Session["MemberID"].ToString())); // Lấy định danh của người dùng từ hệ thống xác thực (hoặc sử dụng cookie).

                // Kiểm tra xem đã có lịch sử đọc cho chapter này chưa.
                var existingRecord = db.Follows
                    .FirstOrDefault(r => r.ReaderID == userId.ReaderID && r.StoryID == storyId);

                if (existingRecord == null)
                {

                    // Nếu chưa có lịch sử đọc cho chapter này, thêm một bản ghi mới.
                    db.Follows.InsertOnSubmit(new Follow
                    {
                        ReaderID = userId.ReaderID,
                        StoryID = storyId,
                        FollowDate = DateTime.Now

                    });
                }
                else
                {
                    // Nếu đã có lịch sử đọc cho chapter này, cập nhật thời gian đọc gần đây.
                    db.Follows.DeleteOnSubmit(existingRecord);
                }
                db.SubmitChanges();
            }  
            
            return null;
        }
        [Route("Story/Truyen/{StoryID}/{ChapterID}")]
        [HttpGet]
        public ActionResult Chapter(int ChapterID)
        {
            ViewBag.ChapterID = ChapterID;
            
            var chap = db.Chapters.SingleOrDefault(n => n.ChapterID == ChapterID);
            Session["StoryID"] = chap.StoryID;
            RecordReadingHistory(int.Parse(chap.StoryID.ToString()), chap.ChapterID);
            if (chap.Vip == true)
            {
                var duyet = db.HistoryBuys.SingleOrDefault(n => n.ChapterID == chap.ChapterID && n.Reader.MemberID == Convert.ToInt16(Session["MemberID"]));
                if(duyet == null)
                {
                   ViewBag.Duyet = true;
                }else
                {
                    ViewBag.Duyet = false;
                    chap.View++;
                    db.SubmitChanges();
                }
            }
           
            return View(chap);
        }
        [Route("Story/Search")]
        public ActionResult Search(FormCollection f)
        {
            String key = f["id"];
            var ds = db.Stories.Where(b => (b.Title.Contains(key) || b.Author.Pen_Name.Contains(key)) && b.Status != 4 && b.Status != 0).ToList();
            return View(ds);
        }
        [Route("Story/Search/find={key}/minc={minc}/cat={cat}/sort={sort}/status={status}")]
        public ActionResult Search(string key,int minc, string cat, string sort,int status)
        {
            var ds = db.Stories.Where(b => (b.Title.Contains(key) || b.Author.Pen_Name.Contains(key))&& b.N_O_Chapter >= minc && b.Status != 4 && b.Status != 0).ToList();
            if(cat != "all")
            {
                ds = db.Stories.Where(b => b.category.CatName.Contains(cat)).ToList();
            }
            if(status != 0 && status != 4)
            {
                ds = ds.Where(b => b.Status == status).ToList();
            }
            var dsc = ds;
            if (sort != "no")
            {
                if (sort == "new")
                {
                    dsc = ds.OrderBy(b => b.Publishdate).ToList();
                }
                else if (sort == "update")
                {
                    dsc = ds.OrderBy(b => b.LastUpdate).ToList();
                }
                else if (sort == "view")
                {
                    dsc = ds.OrderBy(b => b.View).ToList();
                }
                else if (sort == "like")
                {
                    dsc = ds.OrderBy(b => b.Rating).ToList();
                }
            }
            return View(dsc);
        }
        [HttpPost]
        private void RecordReadingHistory(int storyId, int chapterId)
        {
            if(Session["MemberID"] == null)
            {
                Session["MemberID"] = 1;
            }
            var userId = db.Readers.FirstOrDefault(r => r.MemberID == int.Parse(Session["MemberID"].ToString())) ; // Lấy định danh của người dùng từ hệ thống xác thực (hoặc sử dụng cookie).

            // Kiểm tra xem đã có lịch sử đọc cho chapter này chưa.
            var existingRecord = db.Histories
                .FirstOrDefault(r => r.ReaderID == userId.ReaderID && r.Chapter.StoryID == storyId);

            if (existingRecord == null)
            {
                
                // Nếu chưa có lịch sử đọc cho chapter này, thêm một bản ghi mới.
                db.Histories.InsertOnSubmit(new History
                {
                    ReaderID = userId.ReaderID,
                    ChapterID = chapterId,
                    Chapter = db.Chapters.FirstOrDefault(r => r.StoryID == storyId && r.ChapterID == chapterId),
                    E_Time = DateTime.Now
                });
            }
            else
            {
                existingRecord.ChapterID = chapterId;
                existingRecord.Chapter = db.Chapters.FirstOrDefault(r => r.StoryID == storyId && r.ChapterID == chapterId);
                existingRecord.E_Time = DateTime.Now;
            }

            db.SubmitChanges();
        }
        [HttpPost]
        public ActionResult XoaLS(int HisID)
        {
            var his = db.Histories.FirstOrDefault(r => r.HistoryID == HisID); // Lấy định danh của người dùng từ hệ thống xác thực (hoặc sử dụng cookie).

            // Kiểm tra xem đã có lịch sử đọc cho chapter này chưa.
           
                // Nếu đã có lịch sử đọc cho chapter này, cập nhật thời gian đọc gần đây.
                if(his != null)
                {
                    db.Histories.DeleteOnSubmit(his);
                }
                
                db.SubmitChanges();
                return Content("True");
        }

        public ActionResult Comment(String cm)
        {
            var com = cm;
            var id = Session["MemberID"];
            var readerid = db.Readers.FirstOrDefault(r => r.MemberID == Convert.ToInt16(id));
            Comment n = new Comment();
            n.Content = com;
            n.StoryID = Convert.ToInt32( Session["StoryID"]);
            n.ReaderID = readerid.ReaderID;
            n.PublishDate = DateTime.Now;
            db.Comments.InsertOnSubmit(n);
            db.SubmitChanges();
            return Content("True"); 
        }
        public ActionResult DSComment(int StoryID)
        {
            StoryID = (int)Session["StoryID"];
            var ds = from s in db.Comments where s.StoryID == StoryID orderby s.PublishDate select s;
            return PartialView(ds);
        }
        [HttpGet]
        public ActionResult Reply(int CMID)
        {
            Session["CMID"] = CMID;
            
            return View();
        }
       
        public ActionResult ThemReply(String cm)
        {
            var com = cm;
            var id = Session["MemberID"];
            var readerid = db.Readers.FirstOrDefault(r => r.MemberID == Convert.ToInt16(id));
            Reply n = new Reply();
            n.Content = com;
            n.CommentID = Convert.ToInt32(Session["CMID"]);
            n.ReaderID = readerid.ReaderID;
            n.PublishDate = DateTime.Now;
            db.Replies.InsertOnSubmit(n);
            db.SubmitChanges();
            var a = Url.Action("Truyen", "Story", new { StoryID = Session["StoryID"] });
            return Content(a);
        }
        public ActionResult DSReply(int CommentID)
        {
            
            var ds = from s in db.Replies where s.CommentID == CommentID orderby s.PublishDate select s;
            
                return PartialView(ds);
            
           
           
        }
        public ActionResult like(int StoryID)
        {

            var s = db.Stories.SingleOrDefault(n => n.StoryID == StoryID);
            s.Rating++;
            db.SubmitChanges();
            return Redirect(Url.Action("Truyen", "Story", new { StoryID = Session["StoryID"] }));



        }
        public ActionResult DSChap(int StoryID)
        {
            var ds = from s in db.Chapters where s.StoryID == StoryID && s.status == 1  select s;
            Session["StoryID"] = StoryID;
            ViewBag.SC = ds.Count();
            return PartialView(ds);
        }
        public ActionResult DongThongBao(int StoryID)
        {
            var s = db.Stories.FirstOrDefault(r => r.StoryID == StoryID);
            return PartialView(s);
        }

    }
}