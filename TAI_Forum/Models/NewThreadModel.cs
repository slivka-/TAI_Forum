using System.ComponentModel.DataAnnotations;

namespace TAI_Forum.Models
{
    public class NewThreadModel : MainViewModel
    {
        [Required]
        [Display(Name = "Temat")]
        [StringLength(150, ErrorMessage = "{0} musi mieć co najmniej {2} znaków.", MinimumLength = 10)]
        public string Topic { get; set; }

        [Required]
        [Display(Name = "Treść")]
        [StringLength(4000, ErrorMessage = "{0} musi mieć co najmniej {2} znaków.", MinimumLength = 30)]
        [DisplayFormat(HtmlEncode =true)]
        public string Content { get; set; }

        [Display(Name = "Tagi")]
        public string Tags { get; set; }
    }
}