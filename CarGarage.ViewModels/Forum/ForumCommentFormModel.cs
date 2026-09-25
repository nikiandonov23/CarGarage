using System.ComponentModel.DataAnnotations;

namespace CarGarage.ViewModels.Forum
{
    public class ForumCommentFormModel
    {
        [Required]
        public int PostId { get; set; }

        [Required(ErrorMessage = "Коментарът не може да бъде празен.")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Коментарът трябва да бъде между 1 и 2000 символа.")]
        public string Content { get; set; } = null!;
    }
}
