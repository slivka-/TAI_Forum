using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TAI_Forum.Models
{
    public class AccountEditModel :MainViewModel
    {
        public string EditMessage { get; set; }
        public string EditMessageClass { get; set; }

        public string Status { get; set; }

        [Display(Name = "Login")]
        [StringLength(25, ErrorMessage = "{0} może mieć maksymalnie 25 znaków.")]
        public string Login { get; set; }

        [Display(Name = "Aktualne hasło")]
        public string NewLoginPassword { get; set; }

        [StringLength(50, ErrorMessage = "{0} musi mieć co najmniej {2} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nowe hasło")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź nowe hasło")]
        [Compare("NewPassword", ErrorMessage = "Potwierdzenie nie zgadza się z hasłem.")]
        public string ConfirmNewPassword { get; set; }

        [Display(Name = "Aktualne hasło")]
        public string NewPasswordPassword { get; set; }
    }
}