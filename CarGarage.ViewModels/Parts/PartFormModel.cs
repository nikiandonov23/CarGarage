using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGarage.ViewModels.Parts
{
    public class PartFormModel
    {
        public int CarId { get; set; }

        [Required(ErrorMessage = "Категорията е задължителна")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Описанието е задължително")]
        [StringLength(250)]
        public string Description { get; set; } = null!;

        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;

        [Range(0.01, 100000)]
        public decimal UnitPrice { get; set; }

        public IEnumerable<PartCategoryViewModel>? Categories { get; set; }
    }
}
