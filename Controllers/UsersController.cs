using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _301301555_301287005_Laylay_Muhammad__Lab3.Models;

namespace _301301555_301287005_Laylay_Muhammad__Lab3.Controllers
{
    public class UsersController : Controller
    {
        private readonly MovieappContext _context;

        public UsersController(MovieappContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Index", "Home");
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Users/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Users/Login
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Username,PasswordHash")] LoginUser user)
        {


            if (ModelState.IsValid)
            {
                // Attempt to find the user in the database
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == user.Username && u.PasswordHash == user.PasswordHash);

                if (existingUser != null)
                {
                    //// Successful login, redirect to Index
                    //return RedirectToAction(nameof(Index));


                    // Successful login, store user information in session if needed
                    HttpContext.Session.SetString("Username", existingUser.Username);
                    HttpContext.Session.SetString("FullName", existingUser.FullName);
                    HttpContext.Session.SetInt32("UserId", existingUser.UserId);

                    Console.WriteLine("Login successful!");

                    // Redirect to the Movie controller's Index action
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login credentials!");
                }
            }

            return View(user);
        }

        // GET: Users/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear the session

            var username = HttpContext.Session.GetString("Username");
            var userId = HttpContext.Session.GetString("UserId");


            // Print the username and user ID to the console
            Console.WriteLine($"Successfully logout!");

            return RedirectToAction("Index", "Home"); // Redirect to home or any other page
        }


        // GET: Users/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Users/Register
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("UserId,Username,PasswordHash,FullName")] User user)
        {
            // Check if a user with the same username already exists
            if (UserExists(user.Username))
            {
                TempData["RegisterError"] = "Username already exists.";
                return View(user);
            }

            if (ModelState.IsValid)
            {   
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return RedirectToAction("Index", "Home");
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Username,PasswordHash")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            return RedirectToAction("Index", "Home");
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        private bool UserExists(string userName)
        {
            return _context.Users.Any(e => e.Username == userName);
        }
    }
}
