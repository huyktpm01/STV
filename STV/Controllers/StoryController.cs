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
        [HttpGet]
        public ActionResult Chapter(int ChapterID)
        {
            ViewBag.ChapterID = ChapterID;
            var chap = db.Chapters.SingleOrDefault(n => n.ChapterID == ChapterID);
            RecordReadingHistory(int.Parse(chap.StoryID.ToString()),chap.ChapterID);
            return View(chap);
        }
        [HttpPost]
        private void RecordReadingHistory(int storyId, int chapterId)
        {
            var userId = int.Parse(Session["MemberID"].ToString()); // Lấy định danh của người dùng từ hệ thống xác thực (hoặc sử dụng cookie).

            // Kiểm tra xem đã có lịch sử đọc cho chapter này chưa.
            var existingRecord = db.Histories
                .FirstOrDefault(r => r.ReaderID == userId && r.Chapter.StoryID == storyId && r.ChapterID == chapterId);

            if (existingRecord == null)
            {
                
                // Nếu chưa có lịch sử đọc cho chapter này, thêm một bản ghi mới.
                db.Histories.InsertOnSubmit(new History
                {
                    ReaderID = userId,
                    ChapterID = chapterId,
                    Chapter = db.Chapters.FirstOrDefault(r => r.StoryID == storyId && r.ChapterID == chapterId),
                    E_Time = DateTime.Now
                });
            }
            else
            {
                // Nếu đã có lịch sử đọc cho chapter này, cập nhật thời gian đọc gần đây.
                existingRecord.E_Time = DateTime.Now;
            }

            db.SubmitChanges();
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