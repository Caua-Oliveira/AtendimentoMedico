using ClinicaBemEstar.Models;
using ClinicaBemEstar.Data; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ClinicaBemEstar.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context) 
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Specialties()
        {
            var model = await _context.Specialties.ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSpecialty(Specialty specialty)
        {
            if (ModelState.IsValid)
            {
                _context.Specialties.Add(specialty); 
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Specialties));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSpecialty(Specialty specialty)
        {
            if (ModelState.IsValid)
            {
                _context.Specialties.Update(specialty); 
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Specialties));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpecialty(int id)
        {
            bool isLinked = await _context.Doctors.AnyAsync(d => d.SpecialtyId == id);
            if (isLinked)
            {
                TempData["ErrorMessage"] = "Não é possível excluir esta especialidade, pois ela está associada a um ou mais médicos.";
                return RedirectToAction(nameof(Specialties));
            }

            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty != null)
            {
                _context.Specialties.Remove(specialty);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Specialties));
        }


        public async Task<IActionResult> Doctors()
        {
            ViewBag.Specialties = await _context.Specialties.ToListAsync();
            var model = await _context.Doctors.Include(d => d.Specialty).ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDoctor(Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Doctors));
            }

            ViewBag.Specialties = await _context.Specialties.ToListAsync();
            var model = await _context.Doctors.Include(d => d.Specialty).ToListAsync();
            return View("Doctors", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                _context.Doctors.Update(doctor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Doctors));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            bool hasFutureAppointments = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == id &&
                a.StartTime > DateTime.Now &&
                (a.Status == "Pending" || a.Status == null));

            if (hasFutureAppointments)
            {
                TempData["ErrorMessage"] = "Não é possível excluir este médico, pois ele possui agendamentos futuros.";
                return RedirectToAction(nameof(Doctors));
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Doctors));
        }


        public async Task<IActionResult> ViewAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Completed";
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ViewAppointments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Canceled";
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ViewAppointments));
        }
    }
}