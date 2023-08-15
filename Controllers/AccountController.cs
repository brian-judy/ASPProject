using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASPProject.Models;
using System.Data;

using aBCryptNet = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace ASPProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly Team104DBContext _context;

        public AccountController(Team104DBContext context)
        {
            _context = context;
        }

        public IActionResult Login(string returnUrl)
        {
            returnUrl = String.IsNullOrEmpty(returnUrl) ? "~/" : returnUrl;

            return View(new LoginInput { ReturnURL = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind ("Username, UserPassword, ReturnURL")] LoginInput loginInput)
        {
            if (ModelState.IsValid)
            {
                var aUser = await _context.Customers.FirstOrDefaultAsync(u => u.Username == loginInput.Username);

                if(aUser != null && aBCryptNet.Verify(loginInput.UserPassword, aUser.Password))
                {
                    var claims = new List<Claim>();

                    claims.Add(new Claim(ClaimTypes.Name, aUser.Username));
                    claims.Add(new Claim(ClaimTypes.Sid, aUser.CustomerID.ToString()));

                    var identity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);

                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return Redirect(loginInput?.ReturnURL ?? "~/");
                }
                else
                {
                    ViewData["message"] = "Invalid credentials";
                }

            }

            return View(loginInput);
        }

        // GET: Account/Create
        public IActionResult SignUp()
        {
            return View();
        }

        // POST: Account/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("FirstName,LastName,Username,Password,Address,City,State,Zip")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                var aUser = await _context.Customers.FirstOrDefaultAsync(x => x.Username== customer.Username);

                if(aUser is null)
                {
                    customer.Password = aBCryptNet.HashPassword(customer.Password);
                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Account Created Successfully";

                    return RedirectToAction(nameof(Login));
                }
                ViewData["message"] = "Username is already taken, please choose another.";

            }
            return View(customer);
        }

        public async Task<RedirectToActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        private bool CustomerExists(int id)
        {
          return (_context.Customers?.Any(e => e.CustomerID == id)).GetValueOrDefault();
        }

    }
}
