using ClinicaBemEstar.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ClinicaBemEstar.Data;
using Microsoft.AspNetCore.Identity; 
using Microsoft.EntityFrameworkCore; 

namespace ClinicaBemEstar.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // --- PASSO 1 DO REGISTRO: Obter Email ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("email", "O email é obrigatório.");
                return View();
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                ModelState.AddModelError("email", "Este email já está cadastrado.");
                return View("Register", email);
            }

            return View("RegisterDetails", new User { Email = email });
        }


        // --- PASSO 2 DO REGISTRO: Obter Detalhes do Usuário ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterDetails(User user)
        {
            ModelState.Remove("Email");

            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Este email já está cadastrado.");
                    return View("Register", user.Email);
                }

                user.PasswordHash = _passwordHasher.HashPassword(user, user.Password);
                user.Role = "User";

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await SignInUser(user);
                return RedirectToAction("Index", "Home");
            }
            // Se modelo inválido, retornar à view com erros
            return View("RegisterDetails", user);
        }


        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // 10. Checar se o usuário existe e a senha está correta
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Success)
            {
                TempData["ErrorMessage"] = "Email ou senha inválidos.";
                return View();
            }

            // Se usuário válido, faça login
            await SignInUser(user);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email ?? ""),
                new Claim(ClaimTypes.GivenName, user.Name ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}