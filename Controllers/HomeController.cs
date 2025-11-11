using ClinicaBemEstar.Models;
using ClinicaBemEstar.Data; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 

namespace ClinicaBemEstar.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context) 
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var specialties = await _context.Specialties.ToListAsync();
            return View(specialties);
        }

        public async Task<IActionResult> Doctors(int id)
        {
            var filteredDoctors = await _context.Doctors
                .Where(d => d.SpecialtyId == id)
                .ToListAsync();

            var specialty = await _context.Specialties.FindAsync(id);
            ViewBag.SpecialtyName = specialty?.Name ?? "Especialidade não encontrada";

            return View(filteredDoctors);
        }
    }
}