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
            DatabaseAccess client = DatabaseAccess.Instance;
            var result = client.GetUserInfo(SessionAccess.UserLogin);
            var model = new AccountEditModel { UserLogin = result.Item1, Status = result.Item2 };
            return View("EditAccountInfo", model);
        }

        public ActionResult ChangeLogin(AccountEditModel model)
        {
            DatabaseAccess client = DatabaseAccess.Instance;
            if (client.ConfirmUserPassword(SessionAccess.UserLogin, model.NewLoginPassword))
            {
                var result = client.ChangeUserLogin(SessionAccess.UserLogin, model.Login);
                if (result.Item1)
                {
                    string status = model.Status;
                    bool isAdmin = SessionAccess.IsAdmin;
                    SetUserInfo(result.Item2, isAdmin);
                    model = new AccountEditModel()
                    {
                        EditMessage = "Login zmieniony",
                        EditMessageClass = "success",
                        Login = result.Item2,
                        Status = status
                    };
                    return View("EditAccountInfo", model);
                }
                else
                {
                    model.EditMessage = "Błąd, spróbuj ponownie później.";
                    model.EditMessageClass = "warning";
                    return View("EditAccountInfo", model);
                }
            }
            else
            {
                model.EditMessage = "Błędne hasło!";
                model.EditMessageClass = "warning";
                return View("EditAccountInfo", model);
            }
        }

        public ActionResult ChangePassword(AccountEditModel model)
        {
            DatabaseAccess client = DatabaseAccess.Instance;
            if (client.ConfirmUserPassword(SessionAccess.UserLogin, model.NewPasswordPassword))
            {
                bool result = client.ChangeUserPassword(SessionAccess.UserLogin, model.NewPassword);
                if (result)
                {
                    model.NewPassword = "";
                    model.EditMessage = "Hasło zmienione";
                    model.EditMessageClass = "success";
                    return View("EditAccountInfo", model);
                }
                else
                {
                    model.EditMessage = "Błąd, spróbuj ponownie później.";
                    model.EditMessageClass = "warning";
                    return View("EditAccountInfo", model);
                }
            }
            else
            {
                model.EditMessage = "Błędne hasło!";
                model.EditMessageClass = "warning";
                return View("EditAccountInfo", model);
            }
        }
    }
}