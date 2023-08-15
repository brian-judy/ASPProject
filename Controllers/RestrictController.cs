#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ASPProject.Models;

namespace ASPProject.Controllers
{
    public class RestrictController : Controller
    {
        private readonly Team104DBContext _context;

        public RestrictController(Team104DBContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            int userID = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid).Value);

            var orderDetails = _context.OrderLineItems.Include(od => od.Order).Include(od => od.Product)
                .Where(u => u.Order.CustomerID == userID);

            return View(await orderDetails.ToListAsync());
               
        }

        [Authorize]
        public IActionResult PlaceOrder()
        {
            Cart aCart = GetCart();

            if (aCart.CartItems().Any())
            {
                int userID = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid).Value);

                Order aOrder = new() { CustomerID = userID };

                _context.Add(aOrder);
                _context.SaveChanges();

                int orderID = aOrder.OrderID;

                foreach (CartItem aItem in aCart.CartItems())
                {
                    OrderLineItem aLineItem = new() { ProductID = aItem.Product.ProductID, Quantity = aItem.Quantity, OrderID = orderID };
                }

                _context.SaveChanges();

                aCart.ClearCart();

                SaveCart(aCart);

                return View(nameof(OrderConfirmation));
            }

            return RedirectToAction("SearchView", "Menu");
        }

        private IActionResult OrderConfirmation()
        {
            return View();
        }

        private Cart GetCart()
        {
            Cart aCart = HttpContext.Session.GetObject<Cart>("Cart") ?? new Cart();
            return aCart;
        }

        private void SaveCart(Cart aCart)
        {
            HttpContext.Session.SetObject("Cart", aCart);
        }

        [Authorize]
        public IActionResult CheckOut()
        {
            return RedirectToAction("MyCart", "Menu");
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            int userPK = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid).Value);

            var accInfo = await _context.Customers.FirstOrDefaultAsync(x => x.CustomerID == id);

            if (accInfo == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(accInfo);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerID,FirstName,LastName,Username,Address,City,State,Zip")] Customer aCustomer)
        {
            if(id != aCustomer.CustomerID)
            {
                return RedirectToAction("Index", "Home");
            }

            int userID = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid).Value);

            var accInfo = await _context.Customers.FirstOrDefaultAsync(x => x.CustomerID == id);

            if (accInfo == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                accInfo.CustomerID = id;
                accInfo.FirstName = aCustomer.FirstName;
                accInfo.LastName = aCustomer.LastName;
                accInfo.Username = aCustomer.Username;
                accInfo.Address = aCustomer.Address;
                accInfo.City = aCustomer.City;
                accInfo.State = aCustomer.State;
                accInfo.Zip = aCustomer.Zip;

                try
                {
                    _context.Update(accInfo);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    TempData["failure"] = $"Updating Account information Failed";
                    return RedirectToAction(nameof(MyOrders));
                }

                TempData["success"] = $"Account Update Successful!";
                return RedirectToAction(nameof(MyOrders));

            }

            return View(aCustomer);

        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // retrieve user's PK from the Claims collection
            // since the PK is stored as a string, it has to be parsed to an integer

            int userPK = Int32.Parse(HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid).Value);

            // retrieve user's review

            var aOrder = await _context.Orders
                .Include(fr => fr.Customer)
                .FirstOrDefaultAsync(fr => fr.OrderID == id && fr.CustomerID == userPK);

            //if reviewpk is not valid

            if (aOrder == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(aOrder);
        }


    }
}
