using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShopOnline.Controllers
{
    public class DetailController : Controller
    {
        QLBH2025Entities db = new QLBH2025Entities();

        // GET: Detail/DetailShop/5
        [HttpGet]
        public ActionResult DetailShop(int id)
        {
            // Lấy sản phẩm theo ID
            var product = db.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null)
                return HttpNotFound();

            // Thumbnails (4 hình gồm 1 chính + 3 phụ)
            var thumbnails = new List<string>();
            if (!string.IsNullOrEmpty(product.ImagePro)) thumbnails.Add(product.ImagePro);
            if (!string.IsNullOrEmpty(product.ImageThumb1)) thumbnails.Add(product.ImageThumb1);
            if (!string.IsNullOrEmpty(product.ImageThumb2)) thumbnails.Add(product.ImageThumb2);
            if (!string.IsNullOrEmpty(product.ImageThumb3)) thumbnails.Add(product.ImageThumb3);

            ViewBag.Thumbnails = thumbnails;

            // Related products: cùng loại (category) trừ chính nó
            var relatedProducts = db.Products
                .Where(p => p.IDCate == product.IDCate && p.ProductID != product.ProductID)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            // Stock và size
            ViewBag.Stock = product.Stock;
            ViewBag.Sizes = product.AvailableSizes?.Split(',') ?? new string[0];

            return View(product);
        }
        [HttpGet]
        public ActionResult Chitiet(int id)
        {
            // 1. Bắt buộc phải là khách đã đăng nhập
            var account = Session["Account"] as Customer;
            if (account == null)
            {
                return RedirectToAction("Login", "User");
            }

            // 2. Lấy 1 đơn hàng thuộc về khách đang đăng nhập
            var order = db.OrderProes
                          .FirstOrDefault(o => o.ID == id && o.IDCus == account.IDCus);

            if (order == null)
            {
                return HttpNotFound();
            }

            // 3. Tính tổng tiền để hiển thị
            decimal total = 0;
            if (order.OrderDetails != null)
            {
                foreach (var d in order.OrderDetails)
                {
                    var qty = d.Quantity ?? 0;
                    var price = (decimal?)d.UnitPrice ?? 0m;
                    total += qty * price;
                }
            }
            ViewBag.TotalAmount = total;

            // 4. TRẢ VỀ 1 OBJECT OrderPro, KHÔNG PHẢI LIST
            return View(order);   // => sẽ tìm Views/User/Chitiet.cshtml
        }

    }
}