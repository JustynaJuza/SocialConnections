using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Potato.SocialDashboard.Controllers
{
    public class TimelineController : Controller
    {
        //
        // GET: /Timeline/
        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Timeline/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Timeline/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Timeline/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Timeline/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Timeline/Edit/5
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

        //
        // GET: /Timeline/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Timeline/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
