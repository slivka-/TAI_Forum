using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TAI_Forum.Models
{
    public class MessageModel : MainViewModel
    {
        [Display(Name = "Treść")]
        [StringLength(4000, ErrorMessage = "{0} musi mieć co najmniej {2} znaków.", MinimumLength = 30)]
        [DisplayFormat(HtmlEncode = true)]
        public string NewMessage { get; set; }
        
        public string ErrorMessage { get; set; }

        public int ThreadId { get; set; }
    }
}