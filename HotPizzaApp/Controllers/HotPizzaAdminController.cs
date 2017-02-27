using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HotPizzaApp.Models;
using System.IO;
using System.Web.Security;

namespace HotPizzaApp.Controllers
{
    public class HotPizzaAdminController : Controller
    {
        // GET: HotPizzaAdmin
        public ActionResult IndexAdmin()
        {
            return View();
        }

        [HttpGet]
        public ActionResult LoggInn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoggInn(Ansatt bruker)
        {
            try//Testing/sikkerhetsnett dersom databasen skulle feile.
            {
                //1. Databaseoppkobling
                using (var hotPizzaDBKobling = new HotPizzaDBEntities())
                {
                    //2. LINQ-spørring
                    var admin = (from ansatt in hotPizzaDBKobling.Ansatt
                                 where ansatt.Brukernavn == bruker.Brukernavn && ansatt.Passord == bruker.Passord && ansatt.RolleId == 100
                                 select ansatt);

                    if (admin.SingleOrDefault() != null)
                    {
                        Session["LoggetInnBruker"] = admin;

                        ViewBag.LoggInnStatus = true;
                        return RedirectToAction("IndexAdmin");
                    }
                    else
                    {
                        ViewBag.LoggInnstatus = false;
                    }

                    //3. Gjer noko med resultatet
                    return View();
                }
            }
            catch
            {
                return View();
            }
        }

        public ActionResult LoggUt()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Index", "HotPizza");
        }

        public ActionResult VisAlleHenvendelser()
        {
            try 
            {
                //1. Databaseoppkobling
                using (HotPizzaDBEntities hotPizzaDBKobling = new HotPizzaDBEntities())
                {
                    //2. LINQ-spørring
                    var henvendelseliste = (from henvendelse in hotPizzaDBKobling.Henvendelse
                                            select henvendelse).ToList();

                    //3. Gjer noko med resultatet
                    return View(henvendelseliste); //Send festliste til View
                }
            }
            catch
            {
                return View();
            }
        }

        public ActionResult VisAlleAnsatte()
        {
            try
            {
                //1. Databaseoppkobling
                using (HotPizzaDBEntities hotPizzaDBKobling = new HotPizzaDBEntities())
                {
                    //2. LINQ-spørring
                    var ansattliste = (from ansatte in hotPizzaDBKobling.Ansatt.Include("Avdeling").Include("Rolle")
                                       select ansatte).ToList();

                    //3. Gjer noko med resultatet
                    return View(ansattliste); //Send ansattliste til View
                }
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult RegistrerNyAnsatt()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegistrerNyAnsatt(Ansatt nyAnsatt, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)//Sjekker om alle felt er valide
            {
                if(upload != null)//Dersom eit bilde blir lagt til
                {
                    var filnavn = Path.GetFileName(upload.FileName);
                    var filsti = Path.Combine(Server.MapPath("~/Content/Images_Ansatte"), filnavn);

                    upload.SaveAs(filsti);

                    nyAnsatt.BildeSrc = filnavn;
                }
                //1. Databaseoppkobling
                using (var hotPizzaDBKobling = new HotPizzaDBEntities())
                {
                    //2. LINQ-spørring
                    var opprettAnsatt = (from ansatte in hotPizzaDBKobling.Ansatt.Include("Avdeling").Include("Rolle")
                                         select ansatte).ToList();

                    //2.b. Legg til i databasen
                    hotPizzaDBKobling.Ansatt.Add(nyAnsatt);
                    hotPizzaDBKobling.SaveChanges();

                    Session["nyasteAnsatt"] = nyAnsatt.Fornavn + " " + nyAnsatt.Etternavn;

                    //3. Gjer noko med resultatet
                    return RedirectToAction("VisAlleAnsatte");
                }
            }
            else
            {
                ViewBag.Feilmelding = "Noko gjekk galt ved kobling til databasen";
                return View();
            }
        }

        [HttpGet]
        public ActionResult RedigerAnsatt(int id)
        {
            try
            {
                //1. Databaseoppkobling
                using (var hotPizzaDBKobling = new HotPizzaDBEntities())
                {
                    //2. LINQ-spørring
                    var valgtAnsatt = (from ansatt in hotPizzaDBKobling.Ansatt.Include("Avdeling").Include("Rolle")
                                       where ansatt.Id == id
                                       select ansatt).SingleOrDefault();

                    //3. Gjer noko med resultatet
                    return View(valgtAnsatt);
                }
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult RedigerAnsatt(Ansatt redigerAnsatt, HttpPostedFileBase upload)
        {
            try
            {
                if (upload != null)//Dersom eit bilde blir lagt til
                {
                    var filnavn = Path.GetFileName(upload.FileName);
                    var filsti = Path.Combine(Server.MapPath("~/Content/Images_Ansatte"), filnavn);

                    upload.SaveAs(filsti);

                    redigerAnsatt.BildeSrc = filnavn;
                }

                //1. Databaseoppkobling
                using (var hotPizzaDBKobling = new HotPizzaDBEntities())
                {
                    //2. LINQ-spørring
                    var valgtAnsatt = (from ansatt in hotPizzaDBKobling.Ansatt.Include("Avdeling").Include("Rolle")
                                       where ansatt.Id == redigerAnsatt.Id
                                       select ansatt).SingleOrDefault();

                    //2.b. Databasendring
                    valgtAnsatt.Fornavn = redigerAnsatt.Fornavn;
                    valgtAnsatt.Etternavn = redigerAnsatt.Etternavn;
                    valgtAnsatt.Mobil = redigerAnsatt.Mobil;

                    if (upload != null)//Dersom eit nytt bilde blir lagt til, oppdater BildeSrc
                    {
                        valgtAnsatt.BildeSrc = redigerAnsatt.BildeSrc;
                    }

                    valgtAnsatt.AvdelingId = redigerAnsatt.AvdelingId;
                    valgtAnsatt.RolleId = redigerAnsatt.RolleId;
                    hotPizzaDBKobling.SaveChanges();

                    Session["redigertAnsatt"] = redigerAnsatt.Fornavn + " " + redigerAnsatt.Etternavn;

                    //3. Gjer noko med resultatet
                    return RedirectToAction("VisAlleAnsatte");
                }
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult SlettAnsatt(int id)
        {
            try
            {
                //1. Databaseoppkobling
                using (var hotPizzaDBKobling = new HotPizzaDBEntities())
                {
                    //2. LINQ-spørring
                    var valgtAnsatt = (from ansatt in hotPizzaDBKobling.Ansatt
                                       where ansatt.Id == id
                                       select ansatt).SingleOrDefault();

                    //3. Gjer noko med resultatet
                    return View(valgtAnsatt);
                }
            }
            catch
            {
                ViewBag.Feilmelding = "Noko gjekk galt ved kobling til databasen";//Feilmelding
                return View();
            }
        }

        [HttpPost]
        public ActionResult SlettAnsatt(Ansatt slettAnsatt)
        {
            try
            {
                //1. Databaseoppkobling
                using (var hotPizzaDBKobling = new HotPizzaDBEntities())
                {
                    //2. LINQ-spørring
                    var valgtAnsatt = (from ansatt in hotPizzaDBKobling.Ansatt
                                       where ansatt.Id == slettAnsatt.Id
                                       select ansatt).SingleOrDefault();

                    //3 For å slette:
                    hotPizzaDBKobling.Ansatt.Remove(valgtAnsatt);
                    hotPizzaDBKobling.SaveChanges();

                    Session["slettAnsatt"] = valgtAnsatt.Fornavn + " " + valgtAnsatt.Etternavn;

                    return RedirectToAction("VisAlleAnsatte");
                }
            }
            catch
            {
                ViewBag.Feilmelding = "Noko gjekk galt ved kobling til databasen"; //Dersom sletting feila
                return View();
            }
        }
    }
}