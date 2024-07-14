using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;


namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/News
        public ActionResult Index(string Searchtext, int? page)
        {
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<News> items = db.News.OrderByDescending(x => x.Id);
            if (!string.IsNullOrEmpty(Searchtext))
            {
                items = items.Where(x => x.Alias.Contains(Searchtext) || x.Title.Contains(Searchtext));
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(News model)
        {
            if (ModelState.IsValid)
            {
                var existingNews = db.News.Find(model.Id);
                if (existingNews != null)
                {
                    // Check if the entity is still in the Modified state
                    if (db.Entry(existingNews).State == EntityState.Modified)
                    {
                        existingNews.ModifiedDate = DateTime.Now;
                        existingNews.CategoryId = 3; // Assuming you want to set CategoryId to 3
                        existingNews.Alias = Models.Common.Filter.FilterChar(model.Title);

                        // You may want to update other properties here

                        // Save changes
                        try
                        {
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            // Handle concurrency conflict
                            // You may want to reload the entity and apply the changes again or show an error message
                            ModelState.AddModelError("", "Another user has modified the data. Please try again.");
                        }
                    }
                    else
                    {
                        // Entity has been modified by another process
                        // Handle the concurrency conflict
                        ModelState.AddModelError("", "Another user has modified the data. Please try again.");
                    }
                }
                else
                {
                    // Entity not found
                    // Handle the situation accordingly
                    ModelState.AddModelError("", "Entity not found.");
                }
            }

            // If ModelState is not valid or there's an error, return to the view with the model
            return View(model);
        }


        public ActionResult Edit(int id)
        {
            var item = db.News.Find(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(News model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.News.Attach(model);
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.News.Find(id);
            if (item != null)
            {
                db.News.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = db.News.Find(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true, isAcive = item.IsActive });
            }

            return Json(new { success = false });
        }

        //[HttpPost]
        //public ActionResult DeleteAll(string ids)
        //{
        //    if (!string.IsNullOrEmpty(ids))
        //    {
        //        var items = ids.Split(',');
        //        if (items != null && items.Any())
        //        {
        //            foreach (var item in items)
        //            {
        //                var obj = db.News.Find(Convert.ToInt32(item));
        //                db.News.Remove(obj);
        //                db.SaveChanges();
        //            }
        //        }
        //        return Json(new { success = true });
        //    }
        //    return Json(new { success = false });
        //}

    }
}