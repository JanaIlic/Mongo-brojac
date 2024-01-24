namespace API.Servisi
{
    public class NalogServis : INalog
    {
        private IMongoDB mdb;
        private readonly IConfiguration configuration;
        public PomocneFunkcije funkcije;

        public const string korisnici = "Korisnici";
        public const string adminia = "AdminiAktivnosti";
        public const string adminin = "AdminiNamirnica";
        public NalogServis(IMongoDB servis, IConfiguration c, IHttpContextAccessor hca) 
        {
            mdb = servis;
            configuration = c;
            funkcije = new PomocneFunkcije(hca);
        }


        public string NapraviToken(Nalog nalog)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, nalog.Ime),
                new Claim(ClaimTypes.NameIdentifier, nalog.Id),
                new Claim(ClaimTypes.Role, nalog.Uloga.ToString())
            };

            var kljuc = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value));
            var cred = new SigningCredentials(kljuc, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: cred);
         
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }


        public async Task<string> Prijava(string ime, string sifra)
        {
            string token = string.Empty;
            Korisnik nalog = await KorisnikPoImenu(ime);

            if (nalog == null)
                return("Uneto je pogrešno ime. Ne postoji korisnik sa imenom " + ime + ".");

            if (!funkcije.ProveriSifru(sifra, nalog.Sifra, nalog.Kljuc))
                return("Uneta je pogrešna šifra.");

            token = NapraviToken(nalog);
            return token;
        }

        public async Task<string> PrijavaAdminaAA(string ime, string sifra) 
        {
            Nalog nalog = null;
            string token = string.Empty;

            if ((await AdministratorAktivnosti()).Ime == ime)
                nalog = await AdministratorAktivnosti();

            if (nalog == null)
                return ("Ne postoji administrator aktivnosti sa imenom " + ime + ".");

            if (!funkcije.ProveriSifru(sifra, nalog.Sifra, nalog.Kljuc))
                return ("Uneta je pogrešna šifra.");

            token = NapraviToken(nalog);

            return token;
        }

        public async Task<string> PrijavaAdminaAN(string ime, string sifra)
        {
            Nalog nalog = null;
            string token = string.Empty;

            if ((await AdministratorNamirnica()).Ime == ime)
                nalog = await AdministratorNamirnica();
     

            if (nalog == null)
                return ("Ne postoji administrator namirnica sa imenom " + ime + ".");

            if (!funkcije.ProveriSifru(sifra, nalog.Sifra, nalog.Kljuc))
                return ("Uneta je pogrešna šifra.");

            token = NapraviToken(nalog);

            return token;
        }



        public string PrijavljenID()
        {
            return funkcije.PrijavljenID();
        }

        public string Prijavljen() 
        {
            return funkcije.Prijavljen();
        }


        public async Task<string> PrijavljenUloga() 
        {
            return funkcije.PrijavljenUloga();
        }
        public async Task<ICollection<Korisnik>> Korisnici()
        {
            var k = await mdb.UcitajListu<Korisnik>(korisnici);
     
            if (k.Count() == 0)
                return null;

            return k;
        }

        public async Task<AdministratorNamirnica> AdministratorNamirnica() 
        {
            var admin = (await mdb.UcitajListu<AdministratorNamirnica>(adminin)).FirstOrDefault();         
            if (admin == null)
                return null;

            return admin;
        }

        public async Task<AdministratorAktivnosti> AdministratorAktivnosti()
        {
            var admin = (await mdb.UcitajListu<AdministratorAktivnosti>(adminia)).FirstOrDefault();
            if (admin == null)
                return null;

            return admin;
        }


        public async Task<string> Registracija(string ime, string unetaSifra, int godina, int mesec, int dan, PolKorisnika pol, string slika)
        {
            if ((await KorisnikPoImenu(ime) != null) || ((await AdministratorAktivnosti()).Ime.Equals(ime))
            || ((await AdministratorNamirnica()).Ime.Equals(ime)))
                    return "Ime " + ime + " je već zauzeto. Izaberi drugo."; 


            byte[] kljuc;
            byte[] sifra;
            funkcije.SifraHash(unetaSifra, out sifra, out kljuc);

            string info = funkcije.ProveriDatumRodjenja(godina, mesec, dan);
            if (info != String.Empty)
                return (info);

            DateTime dr = new DateTime(godina, mesec, dan);               
           
            Korisnik korisnik = new Korisnik(ime, sifra, kljuc, dr, pol, slika);
            await mdb.Ubaci<Korisnik>(korisnici, korisnik);
            

            if (slika != String.Empty && slika != "*")
                   this.funkcije.UbaciProfilnuSliku(slika, korisnik);
            await mdb.Zameni<Korisnik>(korisnici, korisnik.Id, korisnik);

            return "Korisnik " + korisnik.Ime + " je uspešno registrovan.";
        }


        public int PonudiBrDana(int mesec)
        {
            return this.funkcije.PonudiBrDana(mesec);
        }

        public async Task<Korisnik> KorisnikPoID(string id)
        {
            var korisnik = await mdb.NadjiPoIDu<Korisnik>(korisnici, id);

            if (korisnik == null)
                return null;

            return korisnik;
        }

        public async Task<Korisnik> KorisnikPoImenu(string ime) 
        {
            var k = await mdb.NadjiPoUslovu<Korisnik>(korisnici, "Ime", ime);

            if (k.Count() == 0)
                return null;
            else return  k.FirstOrDefault();            
        }
        public async Task<ICollection<Korisnik>> KorisniciPoImenu(string ime)
        {
            ICollection<Korisnik> k = await mdb.NadjiSlicne<Korisnik>(korisnici, "Ime", ime);    
            if (k == null)
                return null;

            return k;
        }


        public async Task<ICollection<Korisnik>> Pratioci() 
        {
            var parovi = await mdb.NadjiPoUslovu<DvaKorisnika>("ParoviKorisnika", "pracenID", funkcije.PrijavljenID());
            ICollection<Korisnik> pratioci = new List<Korisnik>();
            foreach (var par in parovi)
                pratioci.Add(await KorisnikPoID( par.pratilacID));

            return pratioci;
        }

        public async Task<ICollection<Korisnik>> Praceni()
        {
            var parovi = await mdb.NadjiPoUslovu<DvaKorisnika>("ParoviKorisnika", "patilacID", funkcije.PrijavljenID());
            ICollection<Korisnik> praceni = new List<Korisnik>();
            foreach (var par in parovi)
                praceni.Add(await KorisnikPoID(par.pracenID));

            return praceni;
        }

        public async Task<bool> Pracen(Korisnik k) 
        {
            bool pratim = false;
          //  var par = await context.ParoviKorisnika.Where(par => par.pratilacID == PrijavljenID() && par.pracenID == k.Id).FirstOrDefaultAsync();
            if ((await Praceni()).Contains(k) )
                pratim = true;

            return pratim;
        }

        public async Task<bool> Pratilac(Korisnik k)
        {
            bool prati = false;
       //    var par = await context.ParoviKorisnika.Where(par => par.pracenID == PrijavljenID() && par.pratilacID == k.Id).FirstOrDefaultAsync();
            if ((await Pratioci()).Contains(k))
                prati = true;

            return prati;
        }

        public async Task<Korisnik> PromeniIme(string novoIme)
        {
            Korisnik korisnik = await mdb.NadjiPoIDu<Korisnik>(korisnici, funkcije.PrijavljenID()) ;
            korisnik.Ime = novoIme;

            return await mdb.Zameni<Korisnik>(korisnici, korisnik.Id, korisnik);
        }

        public async Task<Nalog> PromeniImeAdmina(string novoIme)
        {
            Nalog admin;

            if (funkcije.PrijavljenUloga().Equals(UlogaNaloga.AdministratorAktivnosti.ToString()))
            {
                admin = (await AdministratorAktivnosti());
                admin = await mdb.Zameni<AdministratorAktivnosti>(adminia, admin.Id, (AdministratorAktivnosti)admin);
            }
            else 
            {
                admin = await AdministratorNamirnica();
                admin = await mdb.Zameni<AdministratorNamirnica>(adminin, admin.Id, (AdministratorNamirnica)admin);
            }

            return admin;
        }


        public async Task<Korisnik> PromeniSifru(string novaSifra)
        {
             if (String.IsNullOrEmpty(novaSifra))
                    return null;

             byte[] kljuc;
             byte[] sifra;
             funkcije.SifraHash(novaSifra, out sifra, out kljuc);

             Korisnik korisnik = await mdb.NadjiPoIDu<Korisnik>(korisnici, funkcije.PrijavljenID());
             korisnik.Sifra = sifra;
             korisnik.Kljuc = kljuc;

            return await mdb.Zameni<Korisnik>(korisnici, korisnik.Id, korisnik);
        }

        public async Task<Nalog> PromeniSifruAdmina(string novaSifra) 
        {
            Nalog admin;

            byte[] kljuc;
            byte[] sifra;
            funkcije.SifraHash(novaSifra, out sifra, out kljuc);

            if (funkcije.PrijavljenUloga().Equals(UlogaNaloga.AdministratorAktivnosti.ToString()))
            {
                admin = (await AdministratorAktivnosti());
                admin.Sifra = sifra;
                admin.Kljuc = kljuc;
                admin = await mdb.Zameni<AdministratorAktivnosti>(adminia, admin.Id, (AdministratorAktivnosti)admin);
            }
            else
            {
                admin = await AdministratorNamirnica();
                admin.Sifra = sifra;
                admin.Kljuc = kljuc;
                admin = await mdb.Zameni<AdministratorNamirnica>(adminin, admin.Id, (AdministratorNamirnica)admin);
            }

            return admin;
        }



        public async Task<Korisnik> PromeniSliku(string slika)
        {
            Korisnik korisnik = await mdb.NadjiPoIDu<Korisnik>(korisnici, funkcije.PrijavljenID());
            funkcije.UbaciProfilnuSliku(slika, korisnik);

            return await mdb.Zameni<Korisnik>(korisnici, korisnik.Id, korisnik);
        }



        public async Task<string> PromeniDatumRodjenja( int godina, int mesec, int dan)
        {
             string info = funkcije.ProveriDatumRodjenja(godina, mesec, dan);
             if (info != String.Empty)
                return info;

             DateTime dr = new DateTime(godina, mesec, dan);
             var korisnik = await mdb.NadjiPoIDu<Korisnik>(korisnici, funkcije.PrijavljenID());
             korisnik.DatumRodjenja = dr;
             korisnik = await mdb.Zameni<Korisnik>(korisnici, korisnik.Id, korisnik);

             return "Datum rođenja je uspešno promenjen na " + dr.ToString();
        }

        public async Task<string> ObrisiKorisnika()
        {
            var korisnik = await mdb.NadjiPoIDu<Korisnik>(korisnici, funkcije.PrijavljenID());
            if (korisnik == null)
                return "Korisnik ne postoji.";

            this.funkcije.ObrisiSLiku(@"..\..\Brojač Kalorija\Brojac\src\assets\" + korisnik.Ime);

            var stanja = await mdb.NadjiPoUslovu<Stanje>("Stanja", "korisnikID", korisnik.Id);
            var dani = await mdb.NadjiPoUslovu<Dan>("Dani", "korisnikID", korisnik.Id);
            var jela = await mdb.NadjiPoUslovu<Jelo>("Jela", "korisnikID", korisnik.Id);
            var obroci = await mdb.NadjiPoUslovu<Obrok>("Obroci", "korisnikID", korisnik.Id); ;
            var treninzi = await mdb.NadjiPoUslovu<Trening>("Treninzi", "korisnikID", korisnik.Id);
            var objave = await mdb.NadjiPoUslovu<Objava>("Objave", "autorID", korisnik.Id);
            var ocene = await mdb.NadjiPoUslovu<Ocena>("Ocene", "korisnikID", korisnik.Id);
            var poslatePoruke = await mdb.NadjiPoUslovu<Poruka>("Poruke", "autorID", korisnik.Id);
            var poruke = await mdb.NadjiPoUslovu<Poruka>("Poruke", "primalacID", korisnik.Id);
            poruke.AddRange(poslatePoruke);

            var zahteviA = await mdb.NadjiPoUslovu<ZahtevAktivnosti>("ZahteviAktivnosti", "podnosilacID", korisnik.Id);
            var zahteviN = await mdb.NadjiPoUslovu<ZahtevNamirnice>("ZahteviNamirnica", "podnosilacID", korisnik.Id);

            /*  foreach (var z in zahteviA) 
              {
                  var zA = await context.ZahtevaneAktivnosti.Where(za => za.zahtevID == z.Id).FirstOrDefaultAsync();
                  if (zA != null)
                      context.ZahtevaneAktivnosti.Remove(zA);

               //   await context.SaveChangesAsync();
              } 


            foreach (var z in zahteviN)
            {
                var zN = await context.ZahtevaneNamirnice.Where(zn => zn.zahtevID == z.Id).FirstOrDefaultAsync();
                if (zN != null)
                    context.ZahtevaneNamirnice.Remove(zN);

       //         await context.SaveChangesAsync();
             */

            var poslati = await mdb.NadjiPoUslovu<ZahtevZaPracenje>("ZahteviZaPracenje", "podnosilacID", korisnik.Id);
            var zahteviP = await mdb.NadjiPoUslovu<ZahtevZaPracenje>("ZahteviZaPracenje", "pracenID", korisnik.Id);
            zahteviP.AddRange(poslati);


            if (jela != null && jela.Count > 0) 
            {
                await ObrisiVezeJeloNamirnica(jela);
                await mdb.ObrisiListu<Jelo>("Jela", jela);
              //  context.Jela.RemoveRange(jela);
            }

            if (treninzi != null && treninzi.Count > 0)
                await ObrisiVezeTreningAktivnost(treninzi);


            if (dani != null && dani.Count > 0)
            {
                await ObrisiIzvestaje(dani);
                await ObrisiVezeDanObrok(dani);
                await ObrisiVezeDanTrening(dani);
                //  context.Dani.RemoveRange(dani);
                await mdb.ObrisiListu<Dan>("Dani", dani);
            }


            if (obroci != null && obroci.Count > 0)
            {
                await ObrisiVezeJeloObrok(obroci);
                await mdb.ObrisiListu<Obrok>("Obroci",obroci);
               // context.Obroci.RemoveRange(obroci);
            }


            if (treninzi != null && treninzi.Count > 0)
                await mdb.ObrisiListu<Trening>("Treninzi", treninzi);
                // context.Treninzi.RemoveRange(treninzi);

            if (zahteviA != null && zahteviA.Count > 0) 
            {
                await ObrisiRZ(zahteviA as ICollection<Zahtev>);
                await mdb.ObrisiListu<ZahtevAktivnosti>("ZahteviAktivnosti", zahteviA);
                //context.ZahteviAktivnosti.RemoveRange(zahteviA);               
            }

            if (zahteviN != null && zahteviN.Count > 0)
            {               
                await ObrisiRZ(zahteviN as ICollection<Zahtev>);
                await mdb.ObrisiListu<ZahtevNamirnice>("ZahteviNamirnica", zahteviN);
               // context.ZahteviNamirnica.RemoveRange(zahteviN);
            }

            if (zahteviP != null && zahteviP.Count > 0)
            {
                await ObrisiRZ(zahteviP as ICollection<Zahtev>);
                await mdb.ObrisiListu<ZahtevZaPracenje>("ZahteviZaPracenje", zahteviP);
               // context.ZahteviZaPracenje.RemoveRange(zahteviP);              
            }


            if (ocene != null && ocene.Count > 0)
                await mdb.ObrisiListu<Ocena>("Ocene", ocene);
            //context.Ocene.RemoveRange(ocene);

            if (objave != null && objave.Count > 0)
                await mdb.ObrisiListu<Objava>("Objave", objave);
            //  context.Objave.RemoveRange(objave);

            if (poruke != null && poruke.Count > 0)
                await mdb.ObrisiListu<Poruka>("Poruke", poruke);
            // context.Poruke.RemoveRange(poruke);

            if (stanja != null && stanja.Count > 0)
                await mdb.ObrisiListu<Stanje>("Stanja", stanja);
            //   context.Stanja.RemoveRange(stanja);


            //   context.Korisnici.Remove(korisnik);
            await mdb.Obrisi<Korisnik>(korisnici, korisnik.Id);

            return "Korisnik je obrisan.";
        }


        public async Task<string> ObrisiVezeJeloNamirnica(ICollection<Jelo> jela)
        {
          //  ICollection <JeloNamirnica> jNamirnice = new List<JeloNamirnica>();

            foreach (Jelo j in jela) 
            {
                var jn = await mdb.NadjiPoUslovu<JeloNamirnica>("JelaNamirnice", "jeloID", j.Id);
                //context.JelaNamirnice.Where(jn => jn.jeloID == j.Id).FirstOrDefaultAsync();
                if (jn != null)
                    await mdb.ObrisiVeze<JeloNamirnica>("JelaNamirnice", "jeloID", j.Id);
            }

            //if (jNamirnice != null && jNamirnice.Count > 0)
              //  context.JelaNamirnice.RemoveRange(jNamirnice);

            return "Obrisane su veze jela i namirnica.";
        }

        public async Task<string> ObrisiVezeJeloObrok(ICollection<Obrok>obroci) 
        {
            ICollection<ObrokJelo> oJela = new List<ObrokJelo>();
            foreach (Obrok o in obroci) 
            {
                var oJelo = await mdb.NadjiPoUslovu<ObrokJelo>("ObrociJela", "obrokID", o.Id);
                if (oJelo != null)
                    await mdb.ObrisiVeze<ObrokJelo>("ObrociJela", "obrokID", o.Id);
                   // oJela.Add(oJelo);
            }

       //     if (oJela != null && oJela.Count > 0)
         //       context.ObrociJela.RemoveRange(oJela);

            return "Obrisane su veze jela i obroka.";
        }

        public async Task<string> ObrisiVezeDanObrok(ICollection<Dan>dani)
        {
            ICollection<DanObrok> dObroci = new List<DanObrok>();
            foreach (Dan dan in dani) 
            {
                var d = await mdb.NadjiPoUslovu<DanObrok>("DaniObroci", "danID", dan.Id);
                if (d != null)
                    await mdb.ObrisiVeze<DanObrok>("DaniObroci", "danID", dan.Id);
                   // dObroci.Add(d);
            }


        //    if (dObroci != null && dObroci.Count > 0)
          //      context.DaniObroci.RemoveRange(dObroci);

       //     await context.SaveChangesAsync();
            return "Obrisane su veze obroka i dana.";
        }

        public async Task<string> ObrisiVezeDanTrening(ICollection<Dan>dani)
        {
            ICollection<DanTrening> dTreninzi = new List<DanTrening>();
            foreach (Dan dan in dani) 
            {
                //   var dTrening = await context.DaniTreninzi.Where(dt => dt.danID == dan.Id).FirstOrDefaultAsync();
                var dTrening = await mdb.NadjiPoUslovu<DanTrening>("DaniTreninzi", "danID", dan.Id);
                if (dTrening != null)
                    await mdb.ObrisiVeze<DanTrening>("DaniTreninzi", "danID", dan.Id);
                  // dTreninzi.Add(dTrening);
            }

          //  if (dTreninzi != null && dTreninzi.Count > 0 )
            //    context.DaniTreninzi.RemoveRange(dTreninzi);

            return "Obrisane su veze treninga i dana.";
        }
        public async Task<string> ObrisiVezeTreningAktivnost(ICollection<Trening>treninzi)
        {
          //  ICollection<TreningAktivnost> tAktivnosti = new List<TreningAktivnost>();
            foreach (Trening t in treninzi) 
            {
               // var tAktivnost = await context.TreninziAktivnosti.Where(ta => ta.treningID == t.Id).FirstOrDefaultAsync();
               // if (tAktivnost != null)
                    await mdb.ObrisiVeze<TreningAktivnost>("TreninziAktivnosti", "treningID", t.Id);
            }

          //  if (tAktivnosti != null && tAktivnosti.Count > 0)
            //    context.TreninziAktivnosti.RemoveRange(tAktivnosti);
            return "Obrisane su veze treninga i aktivnosti.";
        }

        public async Task<string> ObrisiIzvestaje(ICollection<Dan>dani)
        {
            //    ICollection<Izvestaj> izvestaji = new List<Izvestaj>();
            foreach (Dan dan in dani)
                if (dan.izvestajID != null)
                    await mdb.ObrisiVeze<Izvestaj>("Izvestaji", "Id", dan.izvestajID);
               // izvestaji.Add(await context.Izvestaji.FindAsync(dan.izvestajID));

        //    if (izvestaji != null && izvestaji.Count > 0)
         //       context.Izvestaji.RemoveRange(izvestaji);
            return "Obrisani su dnevni izveštaji.";
        }

        public async Task<string> ObrisiRZ(ICollection<Zahtev> zahtevi) 
        {
           // ICollection<RezultatZahteva> rezultati = new List<RezultatZahteva>();
            if (zahtevi != null) 
            {
                foreach (var zahtev in zahtevi)
                    if (zahtev.rezultatID != null)
                        await mdb.ObrisiVeze<RezultatZahteva>("RezultatiZahteva", "Id", zahtev.rezultatID);
                     //   rezultati.Add(await context.RezultatiZahteva.FindAsync(zahtev.rezultatID));

              //  if (rezultati != null && rezultati.Count > 0)
               //     context.RezultatiZahteva.RemoveRange(rezultati);

            }

            return "Obrisani su rezultati poslatih zahteva.";
        } 


        public async Task<ICollection<Aktivnost>> DodateAktivnosti()
        {
            var admin = AdministratorAktivnosti();
            var aktivnosti = await mdb.NadjiPoUslovu<Aktivnost>("Aktivnosti", "adminID", admin.Result.Id);
            if (aktivnosti.Count == 0)
                return null;

            return aktivnosti;
        }

        public async Task<ICollection<Namirnica>> DodateNamirnice()
        {
            var admin = AdministratorNamirnica();
            var namirnice = await mdb.NadjiPoUslovu<Namirnica>("Namirnice", "adminID", admin.Result.Id);
            if (namirnice.Count == 0)
                return null;

            return namirnice; 
        }
    }
}
