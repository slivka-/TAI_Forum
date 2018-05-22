using System.Web.Mvc;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Text;
using TAI_Forum.Models;
using TAI_Forum.Infrastructure;

namespace TAI_Forum.Controllers
{
    public class AccountController : Controller
    {
        public PartialViewResult GetPopover()
        {
            return PartialView("_LoginPopover");
        }

        
        public ViewResult DisplayRegister()
        {
            return View("Register", new RegisterModel());
        }
        

        public ActionResult Register(RegisterModel model)
        {
            DatabaseAccess client = DatabaseAccess.Instance;
            if (model.Login != null)
                if (!client.IsLoginFree(model.Login))
                    ModelState.AddModelError("", "Ten login jest już zajęty");
            if (ModelState.Values.SelectMany(s => s.Errors).Count() == 0)
            {
                if (client.RegisterUser(model.Login, model.Password.Trim()))
                {
                    return View(new RegisterModel() { Login = null, RegisterMessage = true });
                }
                else
                {
                    ModelState.AddModelError("", "Nastąpił błąd, spróbuj ponownie później");
                }
            }
            return View(model);
            
        }
        

        public ActionResult Login(string login, string password)
        {
            DatabaseAccess client = DatabaseAccess.Instance;
            var loginResult = client.LoginUser(login, password.Trim());
            if (loginResult.Item1)
                SetUserInfo(login,loginResult.Item2);
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        private void SetUserInfo(string userLogin, bool isAdmin)
        {
            SessionAccess.IsLoggedIn = true;
            SessionAccess.UserLogin = userLogin;
            SessionAccess.IsAdmin = isAdmin;
        }

        public ActionResult Logout()
        {
            SessionAccess.ClearSession();
            return RedirectToAction("Index", "Home", new { message = "Wylogowano pomyślnie!" });
        }

        public ActionResult DisplayInfo()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}