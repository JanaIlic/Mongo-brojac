namespace API.Servisi
{
    public class TreningServis : ITrening
    {
        private IMongoDB mdb;
        public PomocneFunkcije funkcije;
        public const string trnz = "Treninzi";
        public const string tr_akt = "TreninziAktivnosti";
        public TreningServis(IMongoDB servis, IHttpContextAccessor hca) 
        { 
            mdb = servis;
            funkcije = new PomocneFunkcije(hca);
        }

        public async Task<ICollection<Trening>> Treninzi()
        {
            var treninzi = await mdb.NadjiPoUslovu<Trening>(trnz, "korisnikID", funkcije.PrijavljenID());
            foreach (Trening t in treninzi)
                t.aktivnosti = await mdb.NadjiPoUslovu<TreningAktivnost>(tr_akt, "treningID", t.Id);

            return treninzi;
        }

        public async Task<Trening> TreningPoIDu(string id)
        {
            return (await mdb.NadjiPoUslovuIzListe<Trening>((await Treninzi()), "Id", id)).FirstOrDefault();
                //context.Treninzi.Where(t => ((t.korisnikID == funkcije.PrijavljenID()) && (t.Id == id))).FirstOrDefaultAsync();
        }

        public async Task<Trening> TreningPoNazivu(string naziv)
        {
            return (await mdb.NadjiPoUslovuIzListe<Trening>((await Treninzi()), "Naziv", naziv )).FirstOrDefault();
           // return await context.Treninzi.Where(t => ((t.korisnikID == funkcije.PrijavljenID()) && (t.Naziv.Equals(naziv)))).FirstOrDefaultAsync();
        }

        public async Task<ICollection<Trening>> TreninziPoNazivu(string naziv)
        {
            var slicni = await mdb.NadjiSlicne<Trening>(trnz, "Naziv", naziv);
            return slicni.Intersect(await Treninzi()).ToList();
        }

        public async Task<double> VremeAktivnosti(Trening trening, Aktivnost aktivnost) 
        {
            var tAktivnost = await mdb.NadjiPoUslovu<TreningAktivnost>(tr_akt, "treningID", trening.Id);
            var rez = await mdb.NadjiPoUslovuIzListe<TreningAktivnost>(tAktivnost, "aktivnostID", aktivnost.Id);
            //context.TreninziAktivnosti.Where(ta => ta.treningID == trening.Id && ta.aktivnostID == aktivnost.ID).FirstOrDefaultAsync();
            return rez.FirstOrDefault().vreme;
        }

        public async Task<ICollection<Aktivnost>> AktivnostiTreninga(Trening trening) 
        {
            List<Aktivnost> aktivnosti = new List<Aktivnost>();
            var tAktivnosti = await mdb.NadjiPoUslovu<TreningAktivnost>(tr_akt, "treningID", trening.Id);
            foreach (var t in tAktivnosti)
                aktivnosti.Add(await mdb.NadjiPoIDu<Aktivnost>("Aktivnosti", t.aktivnostID));

            return aktivnosti;
        }


        public async Task<ICollection<double>> VremenaAktivnosti(Trening trening)
        {
            List<double> vremena = new List<double>();
            var tAktivnosti = await mdb.NadjiPoUslovu<TreningAktivnost>(tr_akt, "treningID", trening.Id);
            foreach (var t in tAktivnosti)
                vremena.Add(t.vreme);

            return vremena;
        }

        public async Task<ICollection<double>> PotrosnjePriAktivnostima(Trening trening) 
        {
            List<double> potrosnje = new List<double>();
            var stanje = (await mdb.NadjiPoUslovu<Stanje>("Stanja", "korisnikID", funkcije.PrijavljenID())).
                OrderBy(s => s.Datum).LastOrDefault();
               
            var tAktivnosti = (await VremenaAktivnosti(trening)).ToList();
            var aktivnosti = (await AktivnostiTreninga(trening)).ToList();

            for (int i = 0; i < aktivnosti.Count; i++)
            {
                var mirovanje = stanje.EnergetskePotrebe * tAktivnosti[i] / 1440;
                double proizvod = Math.Floor(mirovanje * aktivnosti[i].NivoTezine);
                potrosnje.Add(proizvod);
            }

            return potrosnje;
        }


        public async Task<bool> TreningVecDodatDanas(Trening trening) 
        {
            var dodat = false;
            var danas = (await mdb.NadjiPoUslovu<Dan>("Dani", "korisnikID", funkcije.PrijavljenID()))
                            .Where(dan => dan.Datum.Date.Equals(DateTime.Today)).FirstOrDefault();    

            var tr = await mdb.NadjiPoveznik<DanTrening>("DaniTreninzi", "danID", "treningID", danas.Id, trening.Id);
            if (tr != null )
                dodat = true;

            return dodat;
        }

        public async Task<ICollection<Trening>> DanasnjiTreninzi() 
        {
             var danas = (await mdb.NadjiPoUslovu<Dan>("Dani", "korisnikID", funkcije.PrijavljenID()))
                       .Where(dan => dan.Datum.Date.Equals(DateTime.Today)).FirstOrDefault();

            List<Trening> treninzi = new List<Trening>();
            var danasnjiTreninzi = await mdb.NadjiPoUslovu<DanTrening>("DaniTreninzi", "danID", danas.Id);
            foreach (var dt in danasnjiTreninzi)
                treninzi.Add(await mdb.NadjiPoIDu<Trening>(trnz, dt.treningID));


            return treninzi;
        }

        public async Task<ICollection<Trening>> TreninziDana(Dan d) 
        {
            var sviTreninziDana = await mdb.NadjiPoUslovu<DanTrening>("DaniTreninzi", "danID", d.Id);
            List<Trening> treninzi = new List<Trening>();
            foreach (var t in sviTreninziDana)
                treninzi.Add(await mdb.NadjiPoIDu<Trening>(trnz, t.treningID));

            return treninzi;
        }

        public async Task<Trening> DodajTrening(string naziv)
        {
            Trening trening = new Trening(naziv, 0);
            trening.NivoTezine = 0;
            trening.korisnikID = funkcije.PrijavljenID();
            trening.korisnik = await mdb.NadjiPoIDu<Korisnik>("Korisnici", funkcije.PrijavljenID());
            await mdb.Ubaci<Trening>(trnz, trening);

            return trening;
        }

        public async Task<Trening> DodajDnevniTrening(Trening trening) 
        {
            var danas = (await mdb.NadjiPoUslovu<Dan>("Dani", "korisnikID", funkcije.PrijavljenID()))
                  .Where(dan => dan.Datum.Date.Equals(DateTime.Today)).FirstOrDefault();

            if (danas == null)
                return null;

            DanTrening dt = new DanTrening();
            dt.dan = danas;
            dt.danID = danas.Id;
            dt.trening = trening;
            dt.treningID = trening.Id;

            await mdb.Ubaci<DanTrening>("DaniTreninzi", dt);

            var stanje = (await mdb.NadjiPoUslovu<Stanje>("Stanja", "korisnikID", funkcije.PrijavljenID())).OrderBy(s => s.Datum).Last();
            if (stanje == null)
                return null;

            var potrosenje = await  PotrosnjePriAktivnostima(trening);
            double potroseno = 0;
            foreach (var p in potrosenje)
                potroseno += p;

            danas.Rezultat = Math.Round(danas.Rezultat - potroseno, 3);

            if (danas.Prijava)
            {
                var izvestaj = await mdb.NadjiPoIDu<Izvestaj>("Izvestaji", danas.izvestajID);
                izvestaj.Poruka += " · " + trening.Vreme + " min " + trening.Naziv + " = - " + potroseno + " kcal " + Environment.NewLine;
                await mdb.Zameni<Izvestaj>("Izvestaji", izvestaj.Id, izvestaj);
            }

            await mdb.Zameni<Dan>("Dani", danas.Id, danas);


            return trening;
        }


        public async Task<Objava> ObjaviTrening(Trening trening)
        {
            Objava objava = new Objava();
            objava.Vreme = DateTime.Now;
            objava.autorID = funkcije.PrijavljenID();
            objava.autor = await mdb.NadjiPoIDu<Korisnik>("Korisnici", objava.autorID);
            objava.Tekst = await OpisiTrening(trening);

            await mdb.Ubaci<Objava>("Objave", objava);
            return objava;
        }


        public async Task<Trening> PromeniNaziv(Trening trening, string naziv) 
        {
            trening.Naziv = naziv;
            return await mdb.Zameni<Trening>(trnz, trening.Id, trening);
        }

        public async Task<Trening> DodajAktivnostTreningu(Trening trening, Aktivnost aktivnost, int vreme)
        {
            var akt = await mdb.NadjiPoveznik<TreningAktivnost>(tr_akt, "treningID", "aktivnostID", trening.Id, aktivnost.Id);
            if(akt != null)  
                 return null;

            double proizvod = trening.NivoTezine * trening.Vreme;
            proizvod += vreme * aktivnost.NivoTezine;
            trening.Vreme += vreme;
            trening.NivoTezine = Math.Round(proizvod / trening.Vreme, 3);

            TreningAktivnost ta = new TreningAktivnost();
            ta.trening = trening;
            ta.treningID = trening.Id;
            ta.aktivnost = aktivnost;
            ta.aktivnostID = aktivnost.Id;
            ta.vreme = vreme;

            await mdb.Ubaci<TreningAktivnost>(tr_akt, ta);
            return await mdb.Zameni<Trening>(trnz, trening.Id, trening);

        }

        public async Task<Trening> PromeniVremeAktivnosti(Trening trening, Aktivnost aktivnost, int vreme)
        {
            var ta = await mdb.NadjiPoveznik<TreningAktivnost>(tr_akt, "treningID", "aktivnostID", trening.Id, aktivnost.Id);
            if (ta != null)
                return null;

            double proizvod = trening.NivoTezine * trening.Vreme;
            proizvod -= ta.vreme * aktivnost.NivoTezine;
            trening.Vreme -= ta.vreme;
            ta.vreme = vreme;
            trening.Vreme += vreme;
            proizvod += vreme * aktivnost.NivoTezine;
            trening.NivoTezine = Math.Round(proizvod / trening.Vreme, 3);

            await mdb.PromeniPoveznik<TreningAktivnost>(tr_akt, "treningID", "aktivnostID", trening.Id, aktivnost.Id, ta);
            return await mdb.Zameni<Trening>(trnz, trening.Id, trening);
        }


        public async Task<string> OpisiTrening(Trening trening) 
        {
            var stanje = (await mdb.NadjiPoUslovu<Stanje>("Stanja", "korisnikID", funkcije.PrijavljenID())).
                        OrderBy(s => s.Datum).LastOrDefault();
          
            string opis = "Trening: " + trening.Naziv + " 🔥" + Environment.NewLine;

            var aktivnosti = await mdb.NadjiPoUslovu<TreningAktivnost>(tr_akt, "treningID", trening.Id);
            if (aktivnosti.Count == 0)
                return "Niste dodali aktivnosti za ovaj trening.";

            foreach (var a in aktivnosti)
                opis += " - " + a.vreme + " minuta " + 
                    (await mdb.NadjiPoIDu<Aktivnost>("Aktivnosti", a.aktivnostID)).Naziv + Environment.NewLine;

            double potroseno = 0;
            foreach (var p in await PotrosnjePriAktivnostima(trening))
                potroseno += p;
            
            opis += " - Za " + trening.Vreme + " minuta biste potrošili " + potroseno + 
                " kcal više nego što biste u stanju mirovanja. " + Environment.NewLine;

            return opis;
        }


        public async Task<Trening> ObrisiAktivnostIzTreninga(Trening trening, Aktivnost aktivnost)
        {
            var ta = await mdb.NadjiPoveznik<TreningAktivnost>(tr_akt, "treningID", "aktivnostID", trening.Id, aktivnost.Id);
            if (ta != null)
                return null;

            double proizvod = trening.NivoTezine * trening.Vreme; 
            proizvod -= ta.vreme * aktivnost.NivoTezine;
            trening.Vreme -= ta.vreme;
            trening.NivoTezine = Math.Round(proizvod / trening.Vreme, 3);

            await mdb.ObrisiPoveznik<TreningAktivnost>(tr_akt, "treningID", "aktivnostID", trening.Id, aktivnost.Id);        
            return await mdb.Zameni<Trening>(trnz, trening.Id, trening);
        }

        public async Task<string> ObrisiTrening(string id)
        {
            var trening = await mdb.NadjiPoIDu<Trening>(trnz, id);
            if (trening == null)
                return "Ne postoji traženi trening.";

            await mdb.ObrisiVeze<TreningAktivnost>(tr_akt, id, "treningID");
            await ObrisiDanasnjiTrening(id);
            await mdb.Obrisi<Trening>(trnz, id);

            return "Trening " + trening.Naziv + " je obrisan.";
        }


        public async Task<string> ObrisiDanasnjiTrening(string id) 
        {
            var trening = await mdb.NadjiPoIDu<Trening>(trnz, id);
            if (trening == null)
                return "Ne postoji trening sa ID-em " + id + ".";


            var danas = (await mdb.NadjiPoUslovu<Dan>("Dani", "korisnikID", funkcije.PrijavljenID()))
                 .Where(dan => dan.Datum.Date.Equals(DateTime.Today)).FirstOrDefault();

            if (danas == null)
                return null;

            await mdb.ObrisiPoveznik<DanTrening>("DaniTreninzi", "danID", "treningID", danas.Id, trening.Id);
        
               
            var stanje = (await mdb.NadjiPoUslovu<Stanje>("Stanja", "korisnikID", funkcije.PrijavljenID())).OrderBy(s => s.Datum).Last();
            if (stanje == null)
                return null;

            var potrosenje = await PotrosnjePriAktivnostima(trening);
            double potroseno = 0;
            foreach (var p in potrosenje)
                potroseno += p;

            danas.Rezultat = Math.Round(danas.Rezultat + potroseno, 3);

            if (danas.Prijava)
            {
                var izvestaj = await mdb.NadjiPoIDu<Izvestaj>("Izvestaji", danas.izvestajID);
                string izbaciti = " · " + trening.Vreme + " min " + trening.Naziv + " = - " + potroseno + " kcal " + Environment.NewLine;
                izvestaj.Poruka =  izvestaj.Poruka.Replace(izbaciti, "");
                await mdb.Zameni<Izvestaj>("Izvestaji", izvestaj.Id, izvestaj);
            }

            await mdb.Zameni<Dan>("Dani", danas.Id, danas);
            return "Uklonili ste trening " + trening.Naziv + " iz današnjih treninga.";
        }



    }
}
