using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TAI_Forum.Infrastructure
{
    public static class SessionAccess
    {
        public static bool IsLoggedIn
        {
            get
            {
                return HttpContext.Current.Session["IsLoggedIn"] != null ? (bool)HttpContext.Current.Session["IsLoggedIn"] : false;
            }
            set
            {
                HttpContext.Current.Session["IsLoggedIn"] = value;
            }
        }

        public static string UserLogin
        {
            get
            {
                return HttpContext.Current.Session["UserLogin"] != null ? (string)HttpContext.Current.Session["UserLogin"] : null;
            }
            set
            {
                HttpContext.Current.Session["UserLogin"] = value;
            }
        }

        public static bool IsAdmin
        {
            get
            {
                return HttpContext.Current.Session["IsAdmin"] != null ? (bool)HttpContext.Current.Session["IsAdmin"] : false;
            }
            set
            {
                HttpContext.Current.Session["IsAdmin"] = value;
            }
        }

        public static void ClearSession()
        {
            HttpContext.Current.Session.Clear();
        }
    }
}