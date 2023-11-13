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
        dbSTVDataContext db = new dbSTVDataContext("Data Source=LAPTOP-4PHTMN7E;Initial Catalog=Nhom6;Integrated Security=True");
        [Route("Story/Truyen/{StoryID}")]
        public ActionResult Truyen(int StoryID)
        {
                Session["StoryID"] = StoryID;
                ViewBag.AuthorID = StoryID;
                var sach = db.Stories.SingleOrDefault(n => n.StoryID == StoryID);
                return View(sach);
        }
        public ActionResult Muachuong(int chapterid)
        {
            var user = db.Readers.FirstOrDefault(r => r.MemberID == int.Parse(Session["MemberID"].ToString()));
            var chap = db.Chapters.FirstOrDefault(r => r.ChapterID == chapterid);
            var author = db.Members.FirstOrDefault(r => r.MemberID == chap.Story.Author.MemberID);
            float p = 0.9f;
            if (user.Member.Money >= chap.Money)
            {
                HistoryBuy a = new HistoryBuy();
                a.ChapterID = chapterid;
                a.ReaderID = user.ReaderID;
                a.BuyDay = DateTime.Now;
                a.Money = chap.Money;
                author.Money = Convert.ToInt16(chap.Money * p);
                user.Member.Money =  user.Member.Money - Convert.ToInt16(chap.Money);
                Withdraw b = new Withdraw();
                b.AuthorID = author.MemberID;b.Money = Convert.ToInt16(chap.Money * p);
                b.Withdrawaldate = DateTime.Now; b.contend = "Mua Chuong cua truyen" + chap.Story.Title.ToString();
                db.Withdraws.InsertOnSubmit(b);
                db.HistoryBuys.InsertOnSubmit(a);
                db.SubmitChanges();
                return Content("true");
            }
            return Content("false");

        }
        [HttpPost]
        public ActionResult Follow(int storyId)
        {
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
            return null;
        }
        [Route("Story/Truyen/{StoryID}/{ChapterID}")]
        [HttpGet]
        public ActionResult Chapter(int ChapterID)
        {
            ViewBag.ChapterID = ChapterID;
            
            var chap = db.Chapters.SingleOrDefault(n => n.ChapterID == ChapterID);
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
                }
            }
           
            return View(chap);
        }
        [Route("Story/Search")]
        public ActionResult Search(FormCollection f)
        {
            String key = f["id"];
            var ds = db.Stories.Where(b => b.Title.Contains(key) || b.Author.Pen_Name.Contains(key)).ToList();
            return View(ds);
        }
        [Route("Story/Search/find={key}/minc={minc}/cat={cat}/sort={sort}/status={status}")]
        public ActionResult Search(string key,int minc, string cat, string sort,int status)
        {
            var ds = db.Stories.Where(b => (b.Title.Contains(key) || b.Author.Pen_Name.Contains(key))&& b.N_O_Chapter >= minc).ToList();
            if(cat != "all")
            {
                ds = db.Stories.Where(b => b.category.CatName.Contains(cat)).ToList();
            }
            if(status != 0)
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
                .FirstOrDefault(r => r.ReaderID == userId.ReaderID && r.Chapter.StoryID == storyId && r.ChapterID == chapterId);

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
        public ActionResult XoaLS(int hisID)
        {
            var his = db.Histories.FirstOrDefault(r => r.HistoryID == hisID); // Lấy định danh của người dùng từ hệ thống xác thực (hoặc sử dụng cookie).

            // Kiểm tra xem đã có lịch sử đọc cho chapter này chưa.
           
                // Nếu đã có lịch sử đọc cho chapter này, cập nhật thời gian đọc gần đây.
                db.Histories.DeleteOnSubmit(his);
                db.SubmitChanges();
            return null;
        }
        [HttpPost]
        public ActionResult Comment(FormCollection f,int StoryID)
        {
            var com = f["cmtx"];
            var id = Session["MemberID"];
            var readerid = db.Readers.FirstOrDefault(r => r.MemberID == Convert.ToInt16(id));
            Comment n = new Comment();
            n.Content = com;
            n.StoryID = StoryID;
            n.ReaderID = readerid.ReaderID;
            n.PublishDate = DateTime.Now;
            db.Comments.InsertOnSubmit(n);
            db.SubmitChanges();
            return RedirectToAction("Truyen","Story",StoryID);
        }
        public ActionResult DSComment(int StoryID)
        {
            var ds = db.Stories.Where(b => b.StoryID == StoryID).OrderBy(b => b.Publishdate).ToList();
            return PartialView(ds);
        }
        public ActionResult DSChap(int StoryID)
        {
            var ds = from s in db.Chapters where s.StoryID == StoryID && s.status == 1  select s;
            Session["StoryID"] = StoryID;
            ViewBag.SC = ds.Count();
            return PartialView(ds);
        }
        

    }
}