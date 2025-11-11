using System.ComponentModel.DataAnnotations;

namespace ClinicaBemEstar.Models
{
    public class Doctor : IEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do médico é obrigatório.")]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required(ErrorMessage = "O CRM é obrigatório.")]
        [StringLength(20)]
        public string? Crm { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecione uma especialidade.")]
        public int SpecialtyId { get; set; }

        public virtual Specialty? Specialty { get; set; }
        public virtual ICollection<Appointment>? Appointments { get; set; }
    }
}