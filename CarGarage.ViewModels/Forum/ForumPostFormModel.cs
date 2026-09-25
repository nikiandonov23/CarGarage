using System.ComponentModel.DataAnnotations;

namespace CarGarage.ViewModels.Forum
{
    public class ForumPostFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заглавието е задължително.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Заглавието трябва да бъде между 3 и 200 символа.")]
        [Display(Name = "Заглавие")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Съдържанието е задължително.")]
        [StringLength(4000, MinimumLength = 5, ErrorMessage = "Съдържанието трябва да бъде поне 5 символа.")]
        [Display(Name = "Съдържание")]
        public string Content { get; set; } = null!;

        [Url(ErrorMessage = "Моля, въведете валиден URL адрес.")]
        [Display(Name = "URL адрес на снимка")]
        public string? ImageUrl { get; set; }
    }
}
