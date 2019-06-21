using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FutbolZamani.Models;
using System.IO;

namespace FutbolZamani.Controllers
{
    [Auth]
    public class HomeController : Controller
    {
        public FutbolZamaniEntities1 db = new FutbolZamaniEntities1();
        // GET: Home
       
        public ActionResult Profil()//Profil görüntülemek için bu sayfa
        {
            Kullanici k = Session["Kullanici"] as Kullanici;
            return View(k);
        }
        [HttpPost]
        public ActionResult Profil(int? kontrol,Kullanici veri,string eskisifre,string yeni,string yenikontrol, HttpPostedFileBase ProfileImage)//Profil görüntülemek için bu sayfa
        {
            Kullanici k = Session["Kullanici"] as Kullanici;
            Kullanici YeniKul = db.Kullanici.Find(k.KullaniciID);
            MesajModel m = new MesajModel();
            if(kontrol==1)
            {
                try
                {
                    YeniKul.KullaniciAd = veri.KullaniciAd;
                    YeniKul.KullaniciSoyad = veri.KullaniciSoyad;
                    YeniKul.KullaniciTel = veri.KullaniciTel;
                    YeniKul.KullaniciAdres = veri.KullaniciAdres;
                    YeniKul.KullaniciMail = veri.KullaniciMail;
                    db.SaveChanges();
                    m.Status = 1;
                    m.Kontrol = 1;
                    m.Baslik = "Başarılı!";
                    m.Mesaj = "Bilgiler Başarılı Bir Şekilde Güncellendi.";
                }
                catch (Exception)
                {
                    m.Status = 0;
                    m.Baslik = "Dikkat! Güncelleme Hatası!";
                    m.Mesaj = "Bilgileri Güncellerken Hata Oluştu!";
                }
           
            }

            else if(kontrol==2)
            {
                    try
                    {
                        if (ProfileImage != null &&
                  (ProfileImage.ContentType == "image/jpeg" ||
                  ProfileImage.ContentType == "image/jpg" ||
                  ProfileImage.ContentType == "image/png"))
                        {
                            string filename = $"user_{k.KullaniciID}.{ProfileImage.ContentType.Split('/')[1]}";

                            ProfileImage.SaveAs(Server.MapPath($"~/userimages/{filename}"));
                        YeniKul.Resim = filename;
                        db.SaveChanges();
                        Session["Kullanici"] = YeniKul;
                        return View(YeniKul);
                    }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "ERROR:" + ex.Message.ToString();
                        return View();
                    }
                
            }

            else if(kontrol==3)
            {
                if(yeni==yenikontrol)
                {
                    if (eskisifre == k.KullaniciSifre)
                    {
                        if(yeni!=k.KullaniciSifre)
                        {
                            YeniKul.KullaniciSifre = yeni;
                            db.SaveChanges();
                            m.Status = 1;
                            m.Kontrol = 3;
                            m.Baslik = "Başarılı!";
                            m.Mesaj = "Şifre Başarılı Bir Şekilde Güncellendi.";
                        }
                       else
                        {
                            m.Status = 0;
                            m.Baslik = "Dikkat! Şifre Hatası!";
                            m.Mesaj = "Yeni Şifre ile Eski Şifre Aynı!!";
                        }
                    }
                    else
                    {
                        m.Status = 0;
                        m.Baslik = "Dikkat! Şifre Hatası!";
                        m.Mesaj = "Eski Şifre Doğru Değil";
                    }
                }
                else
                {
                    m.Status = 0;
                    m.Baslik = "Şifreler Aynı Değil!";
                    m.Mesaj = "Girilen Yeni Şifre ve Tekrar Yeni Şifre Kısımları Aynı Değil!";
                }
                
            }
            Session["Kullanici"] = YeniKul; 
            return Json(m, JsonRequestBehavior.AllowGet);
        }
      



        public ActionResult Arkadas()//Arkadasları görmek için bu sayfa
        {
            Kullanici k = Session["Kullanici"] as Kullanici;
            List<Kullanici> arkadaslarım = new List<Kullanici>(db.Database.SqlQuery<Kullanici>("select * from Kullanici where KullaniciID in(select KullaniciID from Arkadas where ArkadasID="+k.KullaniciID+" and Durum=1)"));
            List<Kullanici> istek = new List<Kullanici>(db.Database.SqlQuery<Kullanici>("select * from Kullanici where KullaniciID in(select KullaniciID from Arkadas where ArkadasID=" + k.KullaniciID + " and Durum=0)"));
            TempData["istek"] = istek;
            return View(arkadaslarım);
        }
        [HttpPost]
        public ActionResult Arkadas(int durum,int id)//Arkadas silmek için bu sayfa arkadas id
        {
            Kullanici k = Session["Kullanici"] as Kullanici;//k kendi ıd
            MesajModel m = new MesajModel();
            if (durum == 1)
            {
                Kullanici f = db.Kullanici.Where(x => x.KullaniciID == id).FirstOrDefault();
                if (f != null)
                {
                    Models.Arkadas ark1 = new Models.Arkadas();
                    ark1.KullaniciID = k.KullaniciID;
                    ark1.ArkadasID = id;
                    ark1.Durum = 0;
                    db.Arkadas.Add(ark1);
                    db.SaveChanges();
                    db.Database.ExecuteSqlCommand("exec spArkadasOlustur  " + k.KullaniciID + "," + id + "," + 1 + "");
                    m.Status = 1;
                    m.Baslik = "Arkadaş Eklendi";
                    m.Mesaj = "Kişi Artık Arkadaş Listenizde!";
                }
            }
            if (durum == 2)
            {
                Models.Arkadas ark1 =db.Arkadas.FirstOrDefault(x => x.KullaniciID == id && x.ArkadasID == k.KullaniciID);
                db.Arkadas.Remove(ark1);
                db.SaveChanges();
                m.Status = 1;
                m.Baslik = "Arkadaş Bilgisi";
                m.Mesaj = "Kişinin Arkadaşlık İsteğini Red Ettiniz!";

            }
            if (durum == 0)
            {

                Models.Arkadas ark1 = db.Arkadas.FirstOrDefault(x => x.KullaniciID == k.KullaniciID && x.ArkadasID == id);
                Models.Arkadas ark2 = db.Arkadas.FirstOrDefault(x => x.KullaniciID == id && x.ArkadasID == k.KullaniciID);
                db.Arkadas.Remove(ark1);
                db.Arkadas.Remove(ark2);
                db.SaveChanges();
                m.Status = 0;
                m.Baslik = "Arkadaşlıktan Çıkarıldı";
                m.Mesaj = "Kişi Artık Arkadaş Listenizde Değil!";

            }

            return Json(m, JsonRequestBehavior.AllowGet);
        }





        public ActionResult ArkadasEkle()//Arkadas eklemek için bu sayfa
        {
            Kullanici k = Session["Kullanici"] as Kullanici;
            List<Kullanici> Kullanicilar = db.Kullanici.Where(x=>x.KullaniciID!=k.KullaniciID).ToList();
            return View(Kullanicilar);
        }
        [HttpPost]
        public ActionResult ArkadasEkle(int id)//Arkadas eklemek için bu sayfa arkadas id
        {
            Kullanici k = Session["Kullanici"] as Kullanici;//k kendi ıd
            MesajModel m = new MesajModel();
            var deger =db.Arkadas.Where(x =>x.KullaniciID==k.KullaniciID && x.ArkadasID == id).FirstOrDefault();
            if(deger==null)
            {
                Kullanici f = db.Kullanici.Where(x => x.KullaniciID == id).FirstOrDefault();
                if(f!=null)
                {
                    //db.Database.ExecuteSqlCommand("exec spArkadasOlustur  " + k.KullaniciID + "," + id + "," + 0 + "");
                    Models.Arkadas ark1 = new Models.Arkadas();
                    ark1.KullaniciID = k.KullaniciID;
                    ark1.ArkadasID = id;
                    ark1.Durum = 0;
                    db.Arkadas.Add(ark1);
                    db.SaveChanges();
                    m.Status = 1;
                    m.Baslik = "Arkadaş Eklendi";
                    m.Mesaj = "Kişiye Arkadaşlık İsteği Gönderildi!";
                }
            }
            else
            {
                m.Status = 0;
                m.Baslik = "Beklemede";
                m.Mesaj = "Kişi Zaten Arkadaşınız veya Arkadaşlık İsteğiniz Beklemede!";
            }
            return Json(m, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ArkadasListesi(string Ad)//Arkadas eklemek için bu sayfa
        {
            Kullanici k = Session["Kullanici"] as Kullanici;
            List<Kullanici> arkadaslarım;
            List<Kullanici> istek;
            if (Ad != "")
            {
                  arkadaslarım = new List<Kullanici>(db.Database.SqlQuery<Kullanici>("select * from Kullanici where KullaniciID in(select KullaniciID from Arkadas where ArkadasID=" + k.KullaniciID + " and Durum=1)  and KullaniciAd like '%" + Ad + "%' or KullaniciSoyad like '%" + Ad + "%'"));
                  istek = new List<Kullanici>(db.Database.SqlQuery<Kullanici>("select * from Kullanici where KullaniciID in(select KullaniciID from Arkadas where ArkadasID=" + k.KullaniciID + " and Durum=0)  and KullaniciAd like '%" + Ad + "%' or KullaniciSoyad like '%" + Ad + "%'"));
            }
            else
            {
              arkadaslarım = new List<Kullanici>(db.Database.SqlQuery<Kullanici>("select * from Kullanici where KullaniciID in(select KullaniciID from Arkadas where ArkadasID=" + k.KullaniciID + " and Durum=1)"));
              istek = new List<Kullanici>(db.Database.SqlQuery<Kullanici>("select * from Kullanici where KullaniciID in(select KullaniciID from Arkadas where ArkadasID=" + k.KullaniciID + " and Durum=0)"));
            }
            TempData["istek"] = istek;
            return PartialView("_Arkadaslarım", arkadaslarım);
        }

        [HttpPost]
        public ActionResult KullaniciListesi(string Ad, string Telefon, string KullaniciAd, string Adres)//Arkadas eklemek için bu sayfa
        {
            Kullanici k = Session["Kullanici"] as Kullanici;
            List<Kullanici> Kullanicilar = db.Kullanici.Where(x =>
              (string.IsNullOrEmpty(Ad) || x.KullaniciAd.Contains(Ad) || x.KullaniciSoyad.Contains(Ad)) &&
              (string.IsNullOrEmpty(Telefon) || x.KullaniciAd == Telefon) &&
              (string.IsNullOrEmpty(KullaniciAd) || x.KullaniciAd == KullaniciAd) &&
              (string.IsNullOrEmpty(Adres) || x.KullaniciAdres.Contains(Adres))).ToList();
            return PartialView("_ArkadasBul", Kullanicilar);
        }

        public ActionResult Etkinlikler(int? yeniid)//Etkinlik kontrolu ve katılımı için bu sayfa
        {
            Kullanici k = Session["Kullanici"] as Kullanici;
            List<Etkinlik> etkinlik = new List<Etkinlik>(db.Database.SqlQuery<Etkinlik>("select * from Etkinlik where EtkinlikID in(select EtkinlikID from EtkinlikKatilim where KullaniciID="+k.KullaniciID+" and EtkinlikDurum=1)"));
            List<Etkinlik> digeretkinlik = new List<Etkinlik>(db.Database.SqlQuery<Etkinlik>("select * from Etkinlik where EtkinlikID not in(select EtkinlikID from EtkinlikKatilim where KullaniciID=" + k.KullaniciID + " and EtkinlikDurum=1)"));
            TempData["digeretkinlikler"] = digeretkinlik;
            if (Request.IsAjaxRequest())
            {
                if (yeniid != null)
                {
                    etkinlik = new List<Etkinlik>(db.Database.SqlQuery<Etkinlik>("select * from Etkinlik where EtkinlikID="+ yeniid + " and EtkinlikID in(select EtkinlikID from EtkinlikKatilim where KullaniciID=" + k.KullaniciID + " and EtkinlikDurum=1)"));
                    digeretkinlik = new List<Etkinlik>(db.Database.SqlQuery<Etkinlik>("select * from Etkinlik where EtkinlikID="+ yeniid + " and EtkinlikID not in(select EtkinlikID from EtkinlikKatilim where KullaniciID=" + k.KullaniciID + " and EtkinlikDurum=1)"));
                    TempData["digeretkinlikler"] = digeretkinlik;
                }
                return PartialView("_Etkinlikler", etkinlik);
            }
            return View(etkinlik);
        }
        [HttpPost]
        public ActionResult Etkinlikler(Etkinlik etkinlik)//Etkinlik kontrolu ve katılımı için bu sayfa
        {
            MesajModel m = new MesajModel();
            Kullanici k = Session["Kullanici"] as Kullanici;
            etkinlik.KullaniciID = k.KullaniciID;
            etkinlik.EtkinlikDurum = 1;
            etkinlik.EtkinlikTarih=etkinlik.EtkinlikTarih.Replace("T", " ");
            db.Etkinlik.Add(etkinlik);
            db.SaveChanges();
            m.Status = 1;
            m.Baslik = "Etkinlik Eklendi";
            m.Mesaj = etkinlik.EtkinlikBilgi+" Adlı Bir Etkinlik Başlattınız!";
            Etkinlik sonetkinlik = db.Etkinlik.OrderByDescending(x => x.EtkinlikID).FirstOrDefault();
            EtkinlikKatilim katilim=new EtkinlikKatilim();
            katilim.EtkinlikID = sonetkinlik.EtkinlikID;
            katilim.KullaniciID = k.KullaniciID;
            katilim.Durum = 1;
            db.EtkinlikKatilim.Add(katilim);
            db.SaveChanges();
            return Json(m,JsonRequestBehavior.AllowGet);
        }

        public ActionResult EtkinlikDetay(int? id,string Ad)
        {

            Kullanici k = Session["Kullanici"] as Kullanici;
            if (id != null)
            {
                Session["etkinlikid"] = id;
            }
            else
            {
                id = Convert.ToInt32(Session["etkinlikid"]);
            }
            List<Kullanici> kullanicilar = new List<Kullanici>(db.Database.SqlQuery<Kullanici>("select * from Kullanici where KullaniciID in(select KullaniciID from EtkinlikKatilim where EtkinlikID=" + id + ")"));
            if (Request.IsAjaxRequest())
            {
                if (Ad != null)
                {
                    kullanicilar = new List<Kullanici>(db.Database.SqlQuery<Kullanici>("select * from Kullanici where KullaniciID in(select KullaniciID from EtkinlikKatilim where EtkinlikID=" + id + ")  and KullaniciAd like '%" + Ad + "%' or KullaniciSoyad like '%" + Ad + "%'"));
                }
                return PartialView("_EtkinlikDetay", kullanicilar);
            }
            return View(kullanicilar);
        }
        public ActionResult EtkinlikIslem(int? id,int? kulid,int? etkinlikid,string kulad)
        {
            if (etkinlikid == null)
            {
                etkinlikid = Convert.ToInt32(Session["etkinlikid"]);
            }
            MesajModel m = new MesajModel();
            if (id == 0)
            {
                db.EtkinlikKatilim.Remove(db.EtkinlikKatilim.FirstOrDefault(x => x.KullaniciID == kulid && x.EtkinlikID == etkinlikid));
                db.SaveChanges();
                m.Status = 1;
                m.Baslik = "Etkinlik Güncellendi";
                m.Mesaj = " Oyuncu Kaldırıldı!";
            }
            else if (id == 1)
            {
                EtkinlikKatilim katilim =db.EtkinlikKatilim.FirstOrDefault(x => x.KullaniciID == kulid && x.EtkinlikID == etkinlikid);
                katilim.Durum = 2;
                db.SaveChanges();
                m.Status = 1;
                m.Baslik = "Etkinlik İsteği Onaylandı";
                m.Mesaj = " Yeni Bir Oyuncu Daha Ekipte!";
            }
            else if (id == 3)
            {
                EtkinlikKatilim katilimkontrol = db.EtkinlikKatilim.FirstOrDefault(x => x.KullaniciID == kulid && x.EtkinlikID == etkinlikid);
                if (katilimkontrol == null)
                {
                    EtkinlikKatilim katilim = new EtkinlikKatilim();
                    katilim.KullaniciID = kulid;
                    katilim.EtkinlikID = etkinlikid;
                    katilim.Durum = 0;
                    db.EtkinlikKatilim.Add(katilim);
                    db.SaveChanges();
                    m.Status = 1;
                    m.Baslik = "Etkinlik Başvurusu";
                    m.Mesaj = " Başvurul Yapıldı.Etkinlik Sahibinin Onayı Bekleniyor!";
                }
                else
                {
                    m.Status = 0;
                    m.Baslik = "Etkinlik Başvurusu";
                    m.Mesaj = " Zaten Bekleyen Bir Başvurunuz Mevcut!";
                }
                
            }
            else if (id == 4) // Etkinliği Sil
            {
                List<EtkinlikKatilim> e = db.EtkinlikKatilim.Where(x => x.EtkinlikID == etkinlikid).ToList();
                db.EtkinlikKatilim.RemoveRange(e);
                db.SaveChanges();
                db.Etkinlik.Remove(db.Etkinlik.FirstOrDefault(x => x.EtkinlikID==etkinlikid));
                db.SaveChanges();
                return RedirectToAction("Etkinlikler");
            }
            else if (id == 5)
            {
                Kullanici k = db.Kullanici.Where(x => x.UserName == kulad).FirstOrDefault();
                EtkinlikKatilim katilim2 = db.EtkinlikKatilim.FirstOrDefault(x => x.KullaniciID == k.KullaniciID && x.EtkinlikID == etkinlikid);
                if (katilim2 == null)
                {
                    EtkinlikKatilim katilim = new EtkinlikKatilim();
                    katilim.KullaniciID = k.KullaniciID;
                    katilim.EtkinlikID = etkinlikid;
                    katilim.Durum = 3;
                    db.EtkinlikKatilim.Add(katilim);
                    db.SaveChanges();
                    m.Status = 1;
                    m.Baslik = "Etkinlik Davet";
                    m.Mesaj = " Kullanıcıya Davet İsteği Gönderildi";
                }
                else
                {
                    m.Status = 0;
                    m.Baslik = "Etkinlik Davet";
                    m.Mesaj = " Kullanıcı Zaten Etkinlikte veya Daveti Mevcut";
                }
               
            }


            return Json(m,JsonRequestBehavior.AllowGet);
        }

        public ActionResult KullaniciBilgileri(int? id)
        {
            Kullanici k = db.Kullanici.Find(id);
            var myAnonymousType = new
            {
                Ad = k.KullaniciAd,
                Resim = k.Resim,
                UserName = k.UserName,
                GolSayisi = k.KullaniciProfil.GolSayisi.ToString(),
                AsistSayisi = k.KullaniciProfil.AsistSayisi.ToString(),
                ProfilPuani = k.KullaniciProfil.ProfilPuani.ToString(),
                MacSayisi = k.KullaniciProfil.MacSayisi.ToString()
        };
            string veri = k.KullaniciAd + "," + k.Resim + "," + k.UserName + "," +
                          k.KullaniciProfil.GolSayisi.ToString() + "," + k.KullaniciProfil.AsistSayisi.ToString() +"," +
                          k.KullaniciProfil.ProfilPuani.ToString() + "," + k.KullaniciProfil.MacSayisi.ToString();
            return Json(veri, JsonRequestBehavior.AllowGet);
        }
    }
}