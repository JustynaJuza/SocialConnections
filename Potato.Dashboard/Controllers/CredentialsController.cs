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
            var credentials = config.Authorization.Credentials;

            return View(credentials);
        }

        #region AJAX ACTIONS
        [HttpPost]
        public ActionResult Add(CredentialsConfig credentialsConfig)
        {
            if (ModelState.IsValid)
            {
                SocialAllianceConfig.CreateOrUpdateCredentials(credentialsConfig);
            }

            var config = SocialAllianceConfig.Read();
            var credentials = config.Authorization.Credentials;

            return PartialView("_CredentialsListPartial", credentials);
        }

        [HttpPost]
        public ActionResult Delete(AccountType accountType)
        {
            SocialAllianceConfig.DeleteCredentials(accountType);

            var config = SocialAllianceConfig.Read();
            var credentials = config.Authorization.Credentials;

            return PartialView("_CredentialsListPartial", credentials);
        }
        #endregion AJAX ACTIONS
    }
}
