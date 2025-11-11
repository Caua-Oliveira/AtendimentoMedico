using System.ComponentModel.DataAnnotations;

namespace ClinicaBemEstar.Models
{
    public class Specialty : IEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da especialidade é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        public string? Name { get; set; }

        public string? ImageUrl { get; set; }

        public virtual ICollection<Doctor>? Doctors { get; set; }
    }
}