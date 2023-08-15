using ASPProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace ASPProject.Controllers
{
    public class MenuController : Controller
    {
        private readonly Team104DBContext _context;

        public MenuController(Team104DBContext context)
        {
            _context = context;
        }

        public IActionResult ProductView()
        {
            var products = _context.Products.Include(p => p.ProductType);

            return View(products.ToList());
        }

        [HttpGet]
        public IActionResult SearchView()
        {
            List<Product> productList = new();
            return View(productList);
        }
        [HttpPost]
        public IActionResult SearchView(string searchName, string searchType)
        {
            var products = _context.Products.Include(p => p.ProductType).AsQueryable();

            if (!string.IsNullOrEmpty(searchName) )
            {
                products = products.Where(p => p.ProductName.Contains(searchName));
            }
            if(!string.IsNullOrEmpty(searchType) )
            {
                products = products.Where(p => p.ProductType.ProductTypeName.Contains(searchType));
            }

            ViewData["NameSearch"] = searchName;
            ViewData["TypeSearch"] = searchType;

            return View(products.OrderBy(p => p.ProductName));
        }

        public async Task<IActionResult> AddToCart(int? id)
        {
            if(id == null)
            {
                return RedirectToAction(nameof(SearchView));
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == id);

            if(product == null)
            {
                return RedirectToAction(nameof(SearchView));
            }

            Cart aCart = GetCart();

            aCart.AddItem(product);

            SaveCart(aCart);

            return RedirectToAction(nameof(MyCart));
        }

        public IActionResult MyCart()
        {
            Cart aCart = GetCart();

            if (aCart.CartItems().Any())
            {
                return View(aCart);
            }

            return RedirectToAction(nameof(SearchView));
        }

        public IActionResult UpdateCart(int? productID, int qty)
        {
            if (productID == null)
            {
                return RedirectToAction(nameof(SearchView));
            }

            Cart aCart = GetCart();

            CartItem? aItem = aCart.GetCartItem(productID);

            if(aItem == null)
            {
                return RedirectToAction(nameof(SearchView));
            }

            aCart.UpdateItem(aItem.Product?.ProductID, qty);

            SaveCart(aCart);

            return RedirectToAction(nameof(MyCart));
        }

        public IActionResult RemoveFromCart(int? productID)
        {
            if(productID == null)
            {
                return RedirectToAction(nameof(SearchView));
            }

            Cart aCart = GetCart();

            CartItem? aItem = aCart.GetCartItem(productID);

            if(aItem == null)
            {
                return RedirectToAction(nameof(SearchView));
            }

            aCart.RemoveItem(aItem.Product?.ProductID);

            SaveCart(aCart);

            return RedirectToAction(nameof(MyCart));
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
    }
}
