using ShopOnline.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

    namespace ShopOnline.Controllers
    {
        public class CartController : Controller
        {
            // GET: GioHang

            QLBH2025Entities db = new QLBH2025Entities();
            // GET: HienThiCart, chuẩn bị dữ liệu cho View
            public ActionResult HienThiCart()
            {
                if (Session["Cart"] == null)
                    return View("HienThiCart");
                Cart _cart = Session["Cart"] as Cart;
                
                return View(_cart);
            }
            // Tạo mới giỏ hàng, nguồn được lấy từ Session
            public Cart GetCart()
            {
                Cart cart = Session["Cart"] as Cart;
                if (cart == null || Session["Cart"] == null)
                {
                    cart = new Cart();
                    Session["Cart"] = cart;
                }
                return cart;
            }
            // Thêm sản phẩm vào giỏ hàng
            public ActionResult AddToCart(int ProductID, int Quantity)
            {
                var _pro = db.Products.SingleOrDefault(s => s.ProductID == ProductID);
                if (_pro != null)
                {
                    GetCart().Add_Product_Cart(_pro, Quantity);
                }
                return RedirectToAction("HienThiCart", "Cart");
            }
            // Cập nhật số lượng và tính lại tổng tiền
            public ActionResult Update_Cart_Quantity(FormCollection form)
            {
                Cart cart = Session["Cart"] as Cart;
                int id_pro = int.Parse(Request.Form["idPro"]);
                int _quantity = int.Parse(Request.Form["carQuantity"]);

                // Lấy thông tin sản phẩm từ database
                var product = db.Products.FirstOrDefault(p => p.ProductID == id_pro);
                if (product == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra số lượng vượt quá tồn kho
                if (_quantity > product.Stock)
                {
                    TempData["CartError"] = $"Số lượng đã đặt vượt quá tồn kho ({product.Stock})";
                    return RedirectToAction("HienThiCart", "Cart");
                }

                // Nếu hợp lệ thì cập nhật giỏ hàng
                cart.Update_quantity(id_pro, _quantity);

                return RedirectToAction("HienThiCart", "Cart");
            }
            // Xóa dòng sản phẩm trong giỏ hàng
            public ActionResult RemoveCart(int id)
            {
                Cart cart = Session["Cart"] as Cart;
                cart.Remove_CartItem(id);

                return RedirectToAction("HienThiCart", "Cart");
            }

            // Tính tổng tiền đơn hàng
            public PartialViewResult TongTienCart()
            {
                decimal total_money_item = 0;
                Cart cart = Session["Cart"] as Cart;
                if (cart != null)
                    total_money_item = cart.Total_money();
                ViewBag.TotalCart = total_money_item;
                return PartialView("TongTienCart");
            }

            // Tính tổng sản phẩm đơn hàng
            public PartialViewResult TongSanPhamCart()
            {
                int total_items = 0;
                Cart cart = Session["Cart"] as Cart;
                if (cart != null)
                    total_items = cart.Total_quantity(); // Gọi hàm đếm tổng số lượng

                ViewBag.TotalCart = total_items;
                return PartialView("TongSanPhamCart");
            }
            // Các phương thức cho đặt hàng thành công


            public ActionResult CheckOut(FormCollection form)
        {   // Kiểm tra đăng nhập
            if (Session["IDCus"] == null)
                return RedirectToAction("Login", "User");

            // Lấy giỏ hàng từ session
            var cart = Session["Cart"] as Cart;
            if (cart == null || cart.Count == 0)
            {
                return Content("Giỏ hàng trống, không thể đặt đơn");
            }


            // Lấy địa chỉ giao hàng từ form
            string address = form["AddressDeliverry"];
            if (string.IsNullOrWhiteSpace(address))
            {
                return Content("Vui lòng nhập địa chỉ giao hàng");
            }


            try
            {
                // Tạo đơn hàng mới
                OrderPro order = new OrderPro
                {
                    DateOrder = DateTime.Now,
                    IDCus = (int)Session["IDCus"],
                    AddressDeliverry = address,
                    Status = "Chưa xác nhận".Trim()

                };

                db.OrderProes.Add(order);
                db.SaveChanges(); // Lưu để có ID đơn hàng

                // Thêm chi tiết đơn hàng và cập nhật số lượng kho
                foreach (var item in cart.Items)
                {
                    OrderDetail detail = new OrderDetail
                    {
                        IDOrder = order.ID,
                        IDProduct = item._product.ProductID,
                        UnitPrice = (double)item._product.Price,
                        Quantity = item._quantity
                    };
                    db.OrderDetails.Add(detail);

                    // Cập nhật số lượng kho
                    var product = db.Products.SingleOrDefault(p => p.ProductID == item._product.ProductID);
                    if (product != null)
                    {
                        product.Stock -= item._quantity;
                        if (product.Stock < 0)
                        {
                            product.Stock = 0; // tránh âm kho
                        }

                    }
                }

                db.SaveChanges(); // Lưu chi tiết đơn hàng và cập nhật số lượng kho

                // Xóa giỏ hàng sau khi đặt
                Session["Cart"] = null;

                // Chuyển đến trang xác nhận thành công
                return RedirectToAction("CheckOut_Success", "Cart");
            }

            catch (Exception ex)
            {
                return Content("Lỗi khi xử lý đơn hàng: " + ex.Message);
            }
        }

            public ActionResult DonHangCuaBan()
            {
            // 1. Kiểm tra đã login chưa
            if (Session["IDCus"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            int idCus = (int)Session["IDCus"];

            // 2. Lấy tất cả đơn hàng của khách và sắp xếp theo thứ tự mã hóa đơn tăng dần
            var orders = db.OrderProes
                .Where(x => x.IDCus == idCus)
                .OrderBy(x => x.ID)
                .ToList();

            return View(orders);
        }

            //Xác nhận đơn hàng
            public ActionResult XacNhanDonHang()
            {
                var account = Session["Account"] as Customer;

                if (account == null)
                    return RedirectToAction("Login", "User"); // nếu chưa login thì bắt đăng nhập


                return View(account);
            }


            // Thông báo thanh toán thành công
            public ActionResult CheckOut_Success()
            {
                return View();
            }
        }
    }