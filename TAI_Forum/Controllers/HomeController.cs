using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TAI_Forum.Infrastructure;
using TAI_Forum.Models;

namespace TAI_Forum.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string message = null)
        {
            DatabaseAccess client = DatabaseAccess.Instance;
            var th = client.GetAllThreads().OrderByDescending(o => o.Item5.ToDate());
            
            List<IndexModel.SingleThread> tList = new List<IndexModel.SingleThread>();
            foreach (var t in th)
                tList.Add(new IndexModel.SingleThread() { Id = t.Item1, Topic = t.Item2, ContentLead = t.Item3, Tags = t.Item4, Author = t.Item6 });

            IndexModel model = new IndexModel() { ThreadsList = tList };
            if (message != null)
                model.IndexMessage = message;

            return View("Index", model);
        }
    }
}