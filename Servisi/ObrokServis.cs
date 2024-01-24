namespace API.Servisi
{
    public class ObrokServis : IObrok
    {
        private IMongoDB mdb;
        public PomocneFunkcije funkcije;
        public const string obroci = "Obroci";
        public ObrokServis(IMongoDB servis, IHttpContextAccessor hca) 
        {
            mdb = servis;
            funkcije = new PomocneFunkcije(hca);
        }
        public async Task<Obrok> ObrokPoIDu(string id) 
        {
            var obrok = await mdb.NadjiPoIDu<Obrok>(obroci, id);                       
            if(obrok == null ||  obrok.korisnikID != funkcije.PrijavljenID())
                return null;

            return obrok;
        }

        public async Task<ICollection<Obrok>> Obroci()
        {
            return await mdb.NadjiPoUslovu<Obrok>(obroci, "korisnikID", funkcije.PrijavljenID());
        }


        public async Task<Obrok> ObrokPoNazivu(string naziv)
        {
            var obrok = await mdb.NadjiPoUslovu<Obrok>(obroci, "Naziv", naziv);             
            if (obrok == null)
                return null;

            return obrok.FirstOrDefault();
        }


        public async Task<ICollection<Obrok>> ObrociPoNazivu(string naziv)
        {
            var rez = await mdb.NadjiSlicne<Obrok>(obroci, "Naziv", naziv);
            return rez.Intersect(await Obroci()).ToList();
        }


        public async Task<ICollection<Obrok>> ObrociDana(Dan d)
        {
            var sviObrociDana = await mdb.NadjiPoUslovu<DanObrok>("DaniObroci", "danID", d.Id); 
            List<string> obrociIDs = new List<string>();
            foreach (var o in sviObrociDana)
                obrociIDs.Add(o.obrokID);

            var sviObroci = new List<Obrok>();
            foreach (var obrokID in obrociIDs)
                sviObroci.Add(await mdb.NadjiPoIDu<Obrok>(obroci, obrokID));

            return sviObroci;
        }

 


        public async Task<bool> ObrokVecDodatDanas(Obrok obrok) 
        {
            var dodat = false;

            var danasnjiObroci = await DanasnjiObroci();
            if (danasnjiObroci != null && danasnjiObroci.Contains(obrok))
                dodat = true;

            return dodat;
        }

        public async Task<ICollection<Obrok>> DanasnjiObroci() 
        {
            var danas = (await mdb.NadjiPoUslovu<Dan>("Dani", "korisnikID", funkcije.PrijavljenID()))
             .Where(dan => dan.Datum.Date.Equals(DateTime.Today)).FirstOrDefault();              

            List<Obrok> sviObroci = new List<Obrok>();
            var danasnjiObroci = await mdb.NadjiPoUslovu<DanObrok>("DaniObroci", "danID", danas.Id);

            foreach (var d in danasnjiObroci)
                sviObroci.Add(await mdb.NadjiPoIDu<Obrok>(obroci, d.obrokID));


            return sviObroci;
        }

        public async Task<Jelo> JeloObroka(Obrok o, Jelo j) 
        {
            Jelo jelo = new Jelo();

            var jo = await mdb.NadjiPoveznik<ObrokJelo>("ObrociJela", "obrokID", "jeloID", o.Id, j.Id);

            jelo.Id = j.Id;
            jelo.Naziv = j.Naziv;
            jelo.Masa = jo.masa;
            jelo.EnergetskaVrednost = Math.Ceiling(j.EnergetskaVrednost * jo.masa / j.Masa);
            jelo.UgljeniHidrati = Math.Ceiling(j.UgljeniHidrati * jo.masa / j.Masa);
            jelo.Protein = Math.Ceiling(j.Protein * jo.masa / j.Masa);
            jelo.Mast = Math.Ceiling(j.Mast * jo.masa / j.Masa);
            jelo.Recept = j.Recept;

            return jelo;
        }


        public async Task<ICollection<Jelo>> JelaObroka(Obrok o) 
        {
            List<Jelo> jela = new List<Jelo>();
            var jelaO = await mdb.NadjiPoUslovu<ObrokJelo>("ObrociJela", "obrokID", o.Id);
   
            foreach (var oj in jelaO)
                jela.Add(await mdb.NadjiPoIDu<Jelo>("Jela", oj.jeloID));

            return jela;
        }

        public async Task<ICollection<double>> MaseJela(Obrok o) 
        {
            List<double> mase = new List<double>();
            foreach (var j in await mdb.NadjiPoUslovu<ObrokJelo>("ObrociJela", "obrokID", o.Id))
                mase.Add((int)j.masa);

            return mase;
        }

        public async Task<ICollection<double>> EvJela(Obrok o) 
        {
            List<double> ev = new List<double>();
            var jela =  (await JelaObroka(o)).ToList();
            var mase = (await MaseJela(o)).ToList();
            for(int i = 0; i< jela.Count; i++)
                    ev.Add( Math.Round(jela[i].EnergetskaVrednost * mase[i] / jela[i].Masa, 2) );

            return ev;
        }

        public async Task<string> OpisiObrok(Obrok obrok) 
        {
            string opis = "Obrok " + obrok.Naziv + " sadrži: " + Environment.NewLine;
            var jela = (await JelaObroka(obrok)).ToList();
            var mase = (await MaseJela(obrok)).ToList();
            var vrednosti = (await EvJela(obrok)).ToList();

            for (int i = 0; i < jela.Count; i++)
                opis += "- " + mase[i] + " g " + jela[i].Naziv + " = " + vrednosti[i] + " kcal " + Environment.NewLine;

            opis += "- Ukupna masa: " + obrok.Masa + " g, od čega su: " + Environment.NewLine;
            opis +=  "  " + obrok.Protein + "g protein, " + obrok.UgljeniHidrati + " g ugljeni hidrati i " + obrok.Mast + " g masti." + Environment.NewLine;
            opis += "- Energetska vrednost iznosi: " + obrok.EnergetskaVrednost + " kcal.";

            return opis;
        }

        public async Task<Obrok> DodajObrok(string naziv)
        {
            Obrok obrok = new Obrok();
            obrok.Naziv = naziv;
            obrok.korisnikID = funkcije.PrijavljenID();
            obrok.korisnik = await mdb.NadjiPoIDu<Korisnik>("Korisnici", obrok.korisnikID);

            await mdb.Ubaci<Obrok>(obroci, obrok);
            return obrok;
        }



        public async Task<Obrok> DodajObrokDanas(Obrok obrok)
        {
            var danas = (await mdb.NadjiPoUslovu<Dan>("Dani", "korisnikID", funkcije.PrijavljenID())).
              Where(dan => dan.Datum.Date.Equals(DateTime.Today)).FirstOrDefault();
            if (danas == null)
                return null;

            DanObrok danObroka = new DanObrok();
            danObroka.obrok = obrok;
            danObroka.obrokID = obrok.Id;
            danObroka.dan = danas;
            danObroka.danID = danas.Id;

            danas.Rezultat = Math.Round(danas.Rezultat + obrok.EnergetskaVrednost, 3);
            if (danas.Prijava)
            {
                var izvestaj = await mdb.NadjiPoIDu<Izvestaj>("Izvestaji", danas.izvestajID);
                izvestaj.Poruka += " · " + obrok.Masa + " g " + obrok.Naziv + " = " + obrok.EnergetskaVrednost + " kcal " + Environment.NewLine;
                await mdb.Zameni<Izvestaj>("Izvestaji", izvestaj.Id, izvestaj);
            }

            await mdb.Ubaci<DanObrok>("DaniObroci", danObroka);
            await mdb.Zameni<Dan>("Dani", danas.Id, danas);
  
            return obrok;
        }


        public async Task<Objava> ObjaviObrok(Obrok obrok)
        {
            Objava objava = new Objava();
            objava.Vreme = DateTime.Now;
            objava.autorID = funkcije.PrijavljenID();
            objava.autor = await mdb.NadjiPoIDu<Korisnik>("Korisnici", objava.autorID);
            objava.Tekst = await OpisiObrok(obrok);

            await mdb.Ubaci<Objava>("Objave", objava);
            return objava;
        }

        public async Task<Obrok> PromeniNaziv(Obrok obrok, string naziv)
        {
            obrok.Naziv = naziv;
            return await mdb.Zameni<Obrok>(obroci, obrok.Id, obrok);
        }


        public async Task<Obrok> PromeniMasuObroka(Obrok obrok, double masa)
        {
            double koef = masa / obrok.Masa;
            obrok.Masa = Math.Round(masa, 3);
            obrok.EnergetskaVrednost = Math.Round(obrok.EnergetskaVrednost * koef, 3);
            obrok.Protein = Math.Round(obrok.Protein * koef, 3);
            obrok.Mast = Math.Round(obrok.Mast * koef, 3);
            obrok.UgljeniHidrati = Math.Round(obrok.UgljeniHidrati * koef, 3);

            return await mdb.Zameni<Obrok>(obroci, obrok.Id, obrok);
        }


        public async Task<Obrok> DodajJeloObroku(Obrok obrok, Jelo jelo, double masa)
        {
            var vecDodato = await mdb.NadjiPoveznik<ObrokJelo>("ObrociJela", "jeloID", "obrokID", jelo.Id, obrok.Id);
            if (vecDodato != null)
                return null;  

            double koef = masa / jelo.Masa;
            obrok.Masa = Math.Round(obrok.Masa + masa, 3);
            obrok.EnergetskaVrednost = Math.Round( obrok.EnergetskaVrednost + jelo.EnergetskaVrednost * koef, 3);
            obrok.Protein = Math.Round( obrok.Protein + jelo.Protein * koef, 3);
            obrok.Mast = Math.Round(obrok.Mast + jelo.Mast * koef, 3);
            obrok.UgljeniHidrati = Math.Round(obrok.UgljeniHidrati + jelo.UgljeniHidrati * koef, 3);

            ObrokJelo oj = new ObrokJelo();
            oj.obrokID = obrok.Id;
            oj.obrok = obrok;
            oj.jeloID = jelo.Id;
            oj.jelo = jelo;
            oj.masa = masa;

            await mdb.Ubaci<ObrokJelo>("ObrociJela", oj);
            return await mdb.Zameni<Obrok>(obroci, obrok.Id, obrok);
        }


        public async Task<Obrok> PromeniMasuJela(Obrok obrok, Jelo jelo, double masa)
        {
            var oj = await mdb.NadjiPoveznik<ObrokJelo>("ObrociJela", "jeloID", "obrokID", jelo.Id, obrok.Id);
            if (oj != null)
                return null;

    
            obrok.Masa = Math.Round(obrok.Masa - oj.masa, 3);
            double koef = oj.masa / jelo.Masa;
            obrok.EnergetskaVrednost = Math.Round(obrok.EnergetskaVrednost - jelo.EnergetskaVrednost * koef, 3);
            obrok.Protein = Math.Round(obrok.Protein - jelo.Protein * koef, 3);
            obrok.Mast = Math.Round(obrok.Mast - jelo.Mast * koef, 3);
            obrok.UgljeniHidrati = Math.Round(obrok.UgljeniHidrati - jelo.UgljeniHidrati * koef, 3);

            koef = masa / jelo.Masa;
            obrok.Masa = Math.Round(obrok.Masa + masa, 3);
            obrok.EnergetskaVrednost = Math.Round(obrok.EnergetskaVrednost + jelo.EnergetskaVrednost * koef, 3);
            obrok.Protein = Math.Round(obrok.Protein + jelo.Protein * koef, 3);
            obrok.Mast = Math.Round(obrok.Mast + jelo.Mast * koef, 3);
            obrok.UgljeniHidrati = Math.Round(obrok.UgljeniHidrati + jelo.UgljeniHidrati * koef, 3);

            oj.masa = Math.Round(masa, 3);
            await mdb.PromeniPoveznik<ObrokJelo>("ObrociJela", "jeloID", "obrokID", jelo.Id, obrok.Id, oj);
            return await mdb.Zameni<Obrok>(obroci, obrok.Id, obrok);
        }


        public async Task<Obrok> ObrisiJeloIzObroka(Obrok obrok, Jelo jelo)
        {
            var oj = await mdb.NadjiPoveznik<ObrokJelo>("ObrociJela", "jeloID", "obrokID", jelo.Id, obrok.Id);
            if (oj != null)
                return null;

            obrok.Masa = Math.Round(obrok.Masa - oj.masa, 3);
            double koef = oj.masa / jelo.Masa;
            obrok.EnergetskaVrednost = Math.Round(obrok.EnergetskaVrednost - jelo.EnergetskaVrednost * koef, 3);
            obrok.Protein = Math.Round(obrok.Protein - jelo.Protein * koef, 3);
            obrok.Mast = Math.Round(obrok.Mast - jelo.Mast * koef, 3);
            obrok.UgljeniHidrati = Math.Round(obrok.UgljeniHidrati - jelo.UgljeniHidrati * koef, 3);

            await mdb.ObrisiPoveznik<ObrokJelo>("ObrociJela", "jeloID", "obrokID", jelo.Id, obrok.Id);
            return await mdb.Zameni<Obrok>(obroci, obrok.Id, obrok);
        }


        public async Task<string> ObrisiObrok(string id)
        {
            var obrok = await ObrokPoIDu(id);
            if (obrok == null)
                return "Ne postoji traženi obrok.";

            await mdb.ObrisiVeze<ObrokJelo>("ObrociJela", id, "obrokID");
            await mdb.ObrisiVeze<DanObrok>("DaniObroci", id, "obrokID");
            await mdb.Obrisi<Obrok>(obroci, id);
  

            return "Obrok " + obrok.Naziv + " je obrisan.";
        }


        public async Task<string> ObrisiObrokDanas(string id) 
        {
            var obrok = await mdb.NadjiPoIDu<Obrok>(obroci, id);
            if (obrok == null)
                return "Ne postoji traženi obrok.";

            var danas = (await mdb.NadjiPoUslovu<Dan>("Dani", "korisnikID", funkcije.PrijavljenID())).
                                Where(dan => dan.Datum.Date.Equals(DateTime.Today)).FirstOrDefault();
            if (danas == null)
                return null;

            DanObrok danObroka = await mdb.NadjiPoveznik<DanObrok>("DaniObroci", "danID", "obrokID", danas.Id, obrok.Id);
            if (danObroka == null)
                return "Obrok nije unet danas.";

            danas.Rezultat -= obrok.EnergetskaVrednost;
            if (danas.Prijava)
            {
                var izvestaj = await mdb.NadjiPoIDu<Izvestaj>("Izvestaji", danas.izvestajID);
                var izbaciti = " · " + obrok.Masa + " g " + obrok.Naziv + " = " + obrok.EnergetskaVrednost + " kcal " + Environment.NewLine;
                izvestaj.Poruka = izvestaj.Poruka.Replace(izbaciti, "");
                await mdb.Zameni<Izvestaj>("Izvestaji", izvestaj.Id, izvestaj);
            }

            await mdb.Zameni<Dan>("Dani", danas.Id, danas);

            return "Obrok " + obrok.Naziv + " je uklonjen iz današnjih obroka." ;
        }





    }
}
