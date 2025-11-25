using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopOnline.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            QLBH2025Entities db = new QLBH2025Entities();
            // Nếu chưa đăng nhập admin thì đá về trang Login
            if (Session["AdminUser"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Đếm số lượng cho dashboard
            ViewBag.TotalCategories = db.Categories.Count();
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalOrders = db.OrderProes.Count();

            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login","User");
        }
    }
}