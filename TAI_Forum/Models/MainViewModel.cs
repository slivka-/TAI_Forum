using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TAI_Forum.Infrastructure;

namespace TAI_Forum.Models
{
    public class MainViewModel
    {
        public bool IsLoggedIn { get; set; }
        public string UserLogin { get; set; }
        public bool IsAdmin { get; set; }

        public MainViewModel()
        {
            IsLoggedIn = SessionAccess.IsLoggedIn;
            UserLogin = SessionAccess.UserLogin;
            IsAdmin = SessionAccess.IsAdmin;
        }
    }
}