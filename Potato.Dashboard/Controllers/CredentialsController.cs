using SocialAlliance.Models;
using SocialAlliance.Models.WebConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Potato.Dashboard.Controllers
{
    public class CredentialsController : Controller
    {
        public ActionResult Index()
        {
            var config = SocialAllianceConfig.Read();
            return View(config.Authorization.Credentials);
        }

        [HttpPost]
        public ActionResult Add(CredentialsConfig credentialsConfig)
        {
            SocialAllianceConfig.CreateOrUpdateCredentials(credentialsConfig);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Delete(AccountType accountType)
        {
            SocialAllianceConfig.DeleteCredentials(accountType);
            
            return RedirectToAction("Index");
        }
    }
}
