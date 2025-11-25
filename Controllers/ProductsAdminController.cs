using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ShopOnline.Models;

namespace ShopOnline.Controllers
{
    public class ProductsAdminController : Controller
    {
        private QLBH2025Entities db = new QLBH2025Entities();

        private bool IsAdminLoggedIn()
        {
            return Session["AdminUser"] != null;
        }

        public ActionResult Index()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var products = db.Products.Include(p => p.Category).ToList();
            return View(products);
        }

        public ActionResult Details(int? id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }

            return View(products);
        }

        public ActionResult Create()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            ViewBag.IDCate = new SelectList(db.Categories, "IDCate", "NameCate");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,NamePro,DecriptionPro,IDCate,Price,ImagePro,Stock,AvailableSizes,ImageThumb1,ImageThumb2,ImageThumb3")] Product products, HttpPostedFileBase file, HttpPostedFileBase fileThumb1, HttpPostedFileBase fileThumb2, HttpPostedFileBase fileThumb3)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (ModelState.IsValid)
            {
                if (file != null && file.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/pictures"), fileName);
                    file.SaveAs(path);
                    products.ImagePro = fileName;
                }

                if (fileThumb1 != null && fileThumb1.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(fileThumb1.FileName);
                    string path = Path.Combine(Server.MapPath("~/pictures"), fileName);
                    fileThumb1.SaveAs(path);
                    products.ImageThumb1 = fileName;
                }

                if (fileThumb2 != null && fileThumb2.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(fileThumb2.FileName);
                    string path = Path.Combine(Server.MapPath("~/pictures"), fileName);
                    fileThumb2.SaveAs(path);
                    products.ImageThumb2 = fileName;
                }

                if (fileThumb3 != null && fileThumb3.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(fileThumb3.FileName);
                    string path = Path.Combine(Server.MapPath("~/pictures"), fileName);
                    fileThumb3.SaveAs(path);
                    products.ImageThumb3 = fileName;
                }

                db.Products.Add(products);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IDCate = new SelectList(db.Categories, "IDCate", "NameCate", products.IDCate);
            return View(products);
        }

        public ActionResult Edit(int? id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }

            ViewBag.IDCate = new SelectList(db.Categories, "IDCate", "NameCate", products.IDCate);
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,NamePro,DecriptionPro,IDCate,Price,ImagePro,Stock,AvailableSizes,ImageThumb1,ImageThumb2,ImageThumb3")] Product products, HttpPostedFileBase file, HttpPostedFileBase fileThumb1, HttpPostedFileBase fileThumb2, HttpPostedFileBase fileThumb3)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (ModelState.IsValid)
            {
                var existingProduct = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == products.ProductID);

                if (file != null && file.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/pictures"), fileName);
                    file.SaveAs(path);
                    products.ImagePro = fileName;
                }
                else if (existingProduct != null)
                {
                    products.ImagePro = existingProduct.ImagePro;
                }

                if (fileThumb1 != null && fileThumb1.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(fileThumb1.FileName);
                    string path = Path.Combine(Server.MapPath("~/pictures"), fileName);
                    fileThumb1.SaveAs(path);
                    products.ImageThumb1 = fileName;
                }
                else if (existingProduct != null)
                {
                    products.ImageThumb1 = existingProduct.ImageThumb1;
                }

                if (fileThumb2 != null && fileThumb2.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(fileThumb2.FileName);
                    string path = Path.Combine(Server.MapPath("~/pictures"), fileName);
                    fileThumb2.SaveAs(path);
                    products.ImageThumb2 = fileName;
                }
                else if (existingProduct != null)
                {
                    products.ImageThumb2 = existingProduct.ImageThumb2;
                }

                if (fileThumb3 != null && fileThumb3.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(fileThumb3.FileName);
                    string path = Path.Combine(Server.MapPath("~/pictures"), fileName);
                    fileThumb3.SaveAs(path);
                    products.ImageThumb3 = fileName;
                }
                else if (existingProduct != null)
                {
                    products.ImageThumb3 = existingProduct.ImageThumb3;
                }

                db.Entry(products).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.IDCate = new SelectList(db.Categories, "IDCate", "NameCate", products.IDCate);
            return View(products);
        }

        public ActionResult Delete(int? id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product products = db.Products.Find(id);
            if (products == null)
            {
                return HttpNotFound();
            }

            return View(products);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            Product products = db.Products.Find(id);
            db.Products.Remove(products);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
