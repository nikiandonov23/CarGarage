using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGarage.DataModels
{
    public class PartCategory
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        // Връзка към всички записи/части, които потребителят ще въвежда ръчно
        public virtual ICollection<Part> ServiceParts { get; set; } = new HashSet<Part>();

    }
}
