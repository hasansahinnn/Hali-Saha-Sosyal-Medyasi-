using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FutbolZamani.Models;
namespace FutbolZamani.Controllers
{
    public class AnasayfaController : Controller
    {
        public FutbolZamaniEntities1 db = new FutbolZamaniEntities1();
        // GET: Anasayfa
        public ActionResult Giris()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Giris(Kullanici k)
        {
            Kullanici kontrol = db.Kullanici.FirstOrDefault(x=>x.UserName==k.UserName&& k.KullaniciSifre==x.KullaniciSifre);
            if(kontrol!=null)
            {
                Session["Kullanici"] = kontrol;
                Session["KullaniciAd"] = kontrol.KullaniciAd + " " + kontrol.KullaniciSoyad;
                CurrentSession.Set<int>("giris", 1);
                return Redirect("/Home/Etkinlikler");
            }
            TempData["error"] = "Kullanıcı Adi Veya Şifre Yanlış!";
            TempData["renk"] = "red";
            return View();
        }
        [HttpPost]
        public ActionResult Kayit(Kullanici k)
        {
            Kullanici kontrol = db.Kullanici.FirstOrDefault(x => x.UserName == k.UserName);

            if (kontrol != null)
            {
                
                TempData["error"] = "Kullanıcı Adı Mevcut!";
                return Redirect("/Anasayfa/Giris");
            }
            else
            {
                db.Database.ExecuteSqlCommand("exec spKullaniciOlustur '"+k.UserName+"','"+k.KullaniciAd+"','"+k.KullaniciSoyad+ "','" + k.KullaniciTel + "','" + k.KullaniciAdres + "','" + k.KullaniciMail + "','" + k.KullaniciSifre + "'");
                db.SaveChanges();
                TempData["error"] = "Kayıt Başarıyla Oluşturuldu.";
                TempData["renk"] = "green";
                TempData["kullanici"] = k.UserName;
            }
            return Redirect("/Anasayfa/Giris");
        }

        public ActionResult Cikis()
        {
            CurrentSession.Set<int>("giris", 0);
            return Redirect("/Anasayfa/Giris");
        }

    }
}