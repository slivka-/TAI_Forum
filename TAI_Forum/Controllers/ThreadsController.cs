using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TAI_Forum.Models;
using TAI_Forum.Infrastructure;

namespace TAI_Forum.Controllers
{
    public class ThreadsController : Controller
    {
        public ViewResult DisplayNewThread()
        {
            return View("NewThread", new NewThreadModel());
        }

        public ActionResult CreateNewThread(NewThreadModel model)
        {
            DatabaseAccess client = DatabaseAccess.Instance;
            if (model.Topic == null)
                ModelState.AddModelError("", "Brak nazwy wątku!");
            if (model.Content == null)
                ModelState.AddModelError("", "Treść nie może być pusta");
            if (ModelState.Values.SelectMany(s => s.Errors).Count() == 0)
            {
                var registerResult = client.RegisterNewThread(model.UserLogin, model.Topic, model.Content, model.Tags);
                if (registerResult.Item1)
                {
                    return RedirectToAction("ShowThread", new { threadId = registerResult.Item2 });
                }
                else
                {
                    ModelState.AddModelError("", "Nastąpił błąd, spróbuj ponownie później");
                }
            }
            return RedirectToAction("NewThread", model);
        }

        public ViewResult ShowThread(int threadId, MessageModel mmodel = null)
        {
            ViewThreadModel model = new ViewThreadModel() { ThreadId = threadId };
            if (mmodel != null)
            {
                mmodel.ThreadId = threadId;
                model.MessageModel = mmodel;
            }
            else
            {
                model.MessageModel = new MessageModel();
            }

            DatabaseAccess client = DatabaseAccess.Instance;
            var TitleAndTags = client.GetThreadTitleAndTags(threadId);
            model.ThreadTopic = TitleAndTags.Item1;
            model.Tags = TitleAndTags.Item2;
            var messages = client.GetThreadContent(threadId);
            model.Messages = messages.Select(s => new ViewThreadModel.SingleMessage() { Content = s.Item1, Author = s.Item4, Score = s.Item3, PostDate = s.Item2, OrdNum = s.Item5 }).ToList();

            return View("ViewThread",model);
        }

        public ActionResult DeleteThread(int threadId)
        {
            DatabaseAccess client = DatabaseAccess.Instance;
            client.DeleteThread(threadId);

            return RedirectToAction("Index", "Home",new { message = "Wątek pomyślnie usunięty!"});

        }
    }
}