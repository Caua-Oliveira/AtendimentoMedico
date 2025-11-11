using ClinicaBemEstar.Models;
using ClinicaBemEstar.Data; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore; 

namespace ClinicaBemEstar.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context; 

        public AppointmentController(ApplicationDbContext context) 
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MySchedule()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var userAppointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == userId)
                .ToListAsync();

            return View(userAppointments);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Book(int doctorId)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return NotFound();

            ViewBag.DoctorName = doctor.Name;
            ViewBag.DoctorId = doctor.Id;
            ViewBag.AvailableSlots = await GetAvailableTimeSlots(doctorId, 7);

            return View(new Appointment { DoctorId = doctorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Book(Appointment appointment)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User ID not found in claims.");
            }
            appointment.PatientId = userId;

            ModelState.Remove("PatientName");
            ModelState.Remove("Patient");
            ModelState.Remove("Doctor");

            if (ModelState.IsValid)
            {
                var slotEnd = appointment.StartTime.AddMinutes(45);

                bool isSlotBooked = await _context.Appointments.AnyAsync(a =>
                    a.DoctorId == appointment.DoctorId &&
                    (a.Status == "Pending" || a.Status == null) &&
                    appointment.StartTime < a.EndTime && slotEnd > a.StartTime
                );

                if (isSlotBooked)
                {
                    TempData["ErrorMessage"] = "Desculpe, esse horário foi agendado por outra pessoa. Por favor, selecione um novo horário.";
                }
                else
                {
                    appointment.EndTime = slotEnd;
                    appointment.Status = "Pending";
                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Success");
                }
            }

            var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
            if (doctor != null)
            {
                ViewBag.DoctorName = doctor.Name;
                ViewBag.DoctorId = doctor.Id;
                ViewBag.AvailableSlots = await GetAvailableTimeSlots(appointment.DoctorId, 7);
            }
            return View(appointment);
        }

        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            var patientName = User.FindFirstValue(ClaimTypes.GivenName);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var patientAppointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == userId)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            ViewBag.PatientName = patientName;

            return View(patientAppointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (appointment != null && appointment.PatientId == userId)
            {
                appointment.Status = "Canceled";
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Agendamento cancelado com sucesso.";
            }
            else
            {
                TempData["ErrorMessage"] = "Não foi possível localizar o agendamento.";
            }

            return RedirectToAction("MyBookings");
        }


        private async Task<Dictionary<DateTime, List<TimeSpan>>> GetAvailableTimeSlots(int doctorId, int days)
        {
            var doctorAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .ToListAsync();

            var slots = new Dictionary<DateTime, List<TimeSpan>>();
            var today = DateTime.Today;

            for (int i = 0; i < days; i++)
            {
                var currentDate = today.AddDays(i);
                var daySlots = new List<TimeSpan>();

                for (var time = new TimeSpan(9, 0, 0); time < new TimeSpan(17, 0, 0); time = time.Add(TimeSpan.FromMinutes(60)))
                {
                    var slotStart = currentDate.Add(time);
                    var slotEnd = slotStart.AddMinutes(45);

                    bool isSlotBooked = doctorAppointments.Any(a =>
                        (a.Status == "Pending" || a.Status == null) &&
                        slotStart < a.EndTime && slotEnd > a.StartTime
                    );

                    if (slotStart > DateTime.Now && !isSlotBooked)
                    {
                        daySlots.Add(time);
                    }
                }

                if (daySlots.Any())
                {
                    slots.Add(currentDate.Date, daySlots);
                }
            }
            return slots;
        }
    }
}