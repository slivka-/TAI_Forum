using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TAI_Forum.Models;
using TAI_Forum.Infrastructure;

namespace TAI_Forum.Controllers
{
    public class MessagesController : Controller
    {
        public ActionResult CreateNewMessage(MessageModel model)
        {
            model.ErrorMessage = null;
            DatabaseAccess client = DatabaseAccess.Instance;
            if (model.NewMessage == null)
                model.ErrorMessage = "Brak treści!";
            if (model.ErrorMessage == null)
            {
                var msgResult = client.AddNewMessage(model.ThreadId, model.NewMessage, model.UserLogin);
                if (msgResult.Item1)
                {
                    return RedirectToAction("ShowThread", "Threads",new { threadId = msgResult.Item2 });
                }
                else
                {
                    model.ErrorMessage = "Brak treści!";
                }
            }
            return RedirectToAction("ShowThread", "Threads", new { threadId = model.ThreadId, mmodel = model });
        }

        public ActionResult UpvoteMessage(int threadId, int ordNum)
        {
            DatabaseAccess client = DatabaseAccess.Instance;
            int result = client.ScoreMessage(threadId, ordNum, '+');
            return RedirectToAction("ShowThread", "Threads", new { threadId = result });
        }

        public ActionResult DownvoteMessage(int threadId, int ordNum)
        {
            DatabaseAccess client = DatabaseAccess.Instance;
            int result = client.ScoreMessage(threadId, ordNum, '-');
            return RedirectToAction("ShowThread", "Threads", new { threadId = result });
        }

        public ActionResult DeleteMessage(int threadId, int ordNum)
        {
            DatabaseAccess client = DatabaseAccess.Instance;
            var result = client.DeleteMessage(threadId, ordNum);
            if(result.Item1)
                return RedirectToAction("ShowThread", "Threads", new { threadId = result.Item2 });
            else
                return RedirectToAction("ShowThread", "Threads", new { threadId = threadId });
        }
    }
}