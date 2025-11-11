using System.ComponentModel.DataAnnotations;

namespace ClinicaBemEstar.Models
{
    public class Appointment : IEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Por favor, selecione um horário.")]
        [Range(typeof(DateTime), "1/1/2020", "1/1/2100", ErrorMessage = "Por favor, selecione um horário válido.")]
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int DoctorId { get; set; }
        public int PatientId { get; set; } 
        public string? Status { get; set; }

        public virtual Doctor? Doctor { get; set; }
        public virtual User? Patient { get; set; }
    }
}