using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ShopOnline.Models;

namespace ShopOnline.Controllers
{
    public class OrderAdminController : Controller
    {
        private QLBH2025Entities db = new QLBH2025Entities();

        private List<string> StatusOptions = new List<string>
        {
            "Chờ xác nhận",
            "Đã xác nhận",
            "Đang giao hàng",
            "Đã giao hàng",
            "Đã hủy"
        };

        private bool IsAdminLoggedIn()
        {
            return Session["AdminUser"] != null;
        }

        public ActionResult Index(DateTime? fromDate, DateTime? toDate, int? customerId, string status)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var orders = db.OrderProes.Include(o => o.Customer).AsQueryable();

            // Filter by date range
            if (fromDate.HasValue)
            {
                orders = orders.Where(o => o.DateOrder >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                var endDate = toDate.Value.AddDays(1);
                orders = orders.Where(o => o.DateOrder < endDate);
            }

            // Filter by customer
            if (customerId.HasValue)
            {
                orders = orders.Where(o => o.IDCus == customerId.Value);
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status.Trim() == status);
            }

            // Prepare ViewBag for filters
            ViewBag.CustomerList = new SelectList(db.Customers, "IDCus", "NameCus", customerId);
            ViewBag.StatusList = new SelectList(StatusOptions, status);
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(orders.OrderByDescending(o => o.DateOrder).ToList());
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

            OrderPro order = db.OrderProes.Include(o => o.Customer).Include(o => o.OrderDetails).FirstOrDefault(o => o.ID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            foreach (var detail in order.OrderDetails)
            {
                detail.Product = db.Products.Find(detail.IDProduct);
            }

            decimal totalAmount = 0;
            foreach (var detail in order.OrderDetails)
            {
                if (detail.Quantity.HasValue && detail.UnitPrice.HasValue)
                {
                    totalAmount += (decimal)(detail.Quantity.Value * detail.UnitPrice.Value);
                }
            }
            ViewBag.TotalAmount = totalAmount;

            return View(order);
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

            // Nên Include luôn Customer cho chắc
            var order = db.OrderProes
                          .Include("Customer")
                          .FirstOrDefault(o => o.ID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            // Gửi tên khách sang View
            ViewBag.CustomerName = order.Customer != null
                ? order.Customer.NameCus
                : "Không xác định";

            ViewBag.StatusList = new SelectList(StatusOptions, order.Status?.Trim());

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,DateOrder,IDCus,AddressDeliverry,Status")] OrderPro order)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var customer = db.Customers.Find(order.IDCus);
            ViewBag.CustomerName = customer != null ? customer.NameCus : "Không xác định";
            ViewBag.StatusList = new SelectList(StatusOptions, order.Status?.Trim());

            return View(order);
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

            OrderPro order = db.OrderProes.Include(o => o.Customer).FirstOrDefault(o => o.ID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            OrderPro order = db.OrderProes.Find(id);

            var orderDetails = db.OrderDetails.Where(od => od.IDOrder == id).ToList();
            foreach (var detail in orderDetails)
            {
                db.OrderDetails.Remove(detail);
            }

            db.OrderProes.Remove(order);
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
