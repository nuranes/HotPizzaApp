using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HotPizzaApp.Models;

namespace HotPizzaApp.Controllers
{
    public class HotPizzaController : Controller
    {
        // GET: HotPizza
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult LagHenvendelse()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LagHenvendelse(Henvendelse nyHenvendelse)
        {
            if (ModelState.IsValid)
            {
                //1. Databaseoppkobling
                using (var hotPizzaDBKobling = new HotPizzaDBEntities())
                {
                    //2. LINQ-spørring
                    hotPizzaDBKobling.Henvendelse.Add(nyHenvendelse);
                    hotPizzaDBKobling.SaveChanges();

                    Session["SisteHenvendelse"] = nyHenvendelse.Tittel;

                    //3. Gjer noko med resultatet
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View();
            }
        }

        public ActionResult VisAllePizza()
        {
            try //Testing/sikkerhetsnett dersom databasen skulle feile.
            {
                using (HotPizzaDBEntities hotPizzaDBKobling = new HotPizzaDBEntities())
                {
                    var pizzaliste = (from pizza in hotPizzaDBKobling.Pizza.Include("Ansatt")//Henter data også fra Ansatt-tabellen
                                      select pizza).ToList();

                    return View(pizzaliste); //Send pizzaliste til View
                }
            }
            catch
            {
                return View();
            }
        }
    }
}