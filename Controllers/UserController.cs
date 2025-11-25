using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopOnline.Controllers
{
    public class UserController : Controller
    {
        QLBH2025Entities db = new QLBH2025Entities();

        //Login chung
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //Login Customer
        //[HttpPost]
        //public ActionResult Login(Customer cus)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        var account = db.Customer.FirstOrDefault(k => k.EmailCus == cus.EmailCus && k.PasswordUser == cus.PasswordUser);
        //        if (account != null)
        //        {

        //            Session["Account"] = account;
        //            Session["IDCus"] = account.IDCus;
        //            return RedirectToAction("Index", "TrangChu");
        //        }
        //        else
        //            ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
        //    }
        //    return View();
        //}



        [HttpPost]

        public ActionResult Login(Customer model)
        {
            // 1. Kiểm tra nhập đủ chưa
            if (string.IsNullOrEmpty(model.EmailCus) || string.IsNullOrEmpty(model.PasswordUser))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View(model);
            }

            // 2. Check admin trước
            var admin = db.AdminUsers
                          .FirstOrDefault(a => a.Email == model.EmailCus
                                            && a.Password == model.PasswordUser);
            if (admin != null)
            {
                Session["AdminUser"] = admin;
                Session["Account"] = null;      

                return RedirectToAction("Index", "Admin");
            }

            // 3. Không phải admin -> check CUSTOMER
            var account = db.Customers
                            .FirstOrDefault(c => c.EmailCus == model.EmailCus
                                              && c.PasswordUser == model.PasswordUser);
            if (account != null)
            {
                Session["Account"] = account;
                Session["IDCus"] = account.IDCus;
                Session["AdminUser"] = null;        // clear session admin nếu có

                // Chuyển tới trang khách
                return RedirectToAction("Index", "TrangChu");
            }

            // 4. Sai cả 2
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View(model);
        }

        //Đăng kí
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Customer customer, string rePassword)
        {
            // 1) Check confirm password
            if (customer.PasswordUser != rePassword)
            {
                ViewBag.Notification = "Mật khẩu xác nhận không chính xác";
                return View(customer);
            }

            // 2) Check email trùng
            var existed = db.Customers.FirstOrDefault(k => k.EmailCus == customer.EmailCus);
            if (existed != null)
            {
                ViewBag.NotificationEmail = "Đã có người đăng ký email này";
                return View(customer);
            }

            // 3) Lưu DB
            db.Customers.Add(customer);
            db.SaveChanges();

            // 4) Redirect về trang Login
            return RedirectToAction("Login", "User");
        }

        [HttpGet]
        public ActionResult Info()
        {
            var account = Session["Account"] as Customer;
            if (account == null)
            {
                return RedirectToAction("Login", "User");
            }
                return View(account);
        }

        [HttpPost]
        public ActionResult UpdateInfo(Customer customer)
        {
            var cus = db.Customers.Find(customer.IDCus);

            if (cus != null)
            {

                cus.NameCus = customer.NameCus;
                cus.EmailCus = customer.EmailCus;
                cus.PhoneCus = customer.PhoneCus;

                db.SaveChanges();

                Session["Account"] = cus;
            }

            return RedirectToAction("Info","User");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "User");
        }

       

    }
}