using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Potato.Dashboard.Controllers
{
    public class NotifyMeController : Controller
    {
        //
        // GET: /NotifyMe/
        public ActionResult Index()
        {
            throw new NotImplementedException("There is no index view for this controller!");
        }

        // GET: /NotifyMe/Save
        [HttpPost]
        public ActionResult Save(String email)
        {
            if (!String.IsNullOrEmpty(email))
            {
                AddEmail(email);
            }
            return RedirectToAction("HoldingPage", "Home");
        }

        protected void AddEmail(String email)
        {
            // TODO: Review if this (e.g. NotifyMe email log) needs to go into a database or other repository rather than a text file
            try
            {
                using (var sw = new StreamWriter(Server.MapPath("~/App_Data/NotifyMeEmails.txt"), true))
                {
                    // TODO: Decide if # identified timestamps would be handy in the NotifyMe registration log
                    //sw.WriteLine(String.Format("# New NotifyMe Registration: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sw.WriteLine(email);
                    sw.Close();
                }
                TempData["NotifyMeMessage"] =
                    String.Format("Thank you for registering to be notified about the PotatoTV.co.uk launch", email);
            }
            catch (Exception ex)
            {
                TempData["Error"] = new Exception("Failed to save email address to log file", ex).ToString();
            }
        }
    }
}