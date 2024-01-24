namespace API.Servisi
{
    public class JeloServis : IJelo
    {
        private IMongoDB mdb;
        public PomocneFunkcije funkcije;
        public const string jela = "Jela";
        public const string jnamirnice = "JelaNamirnice";
        public JeloServis(IMongoDB servis, IHttpContextAccessor hca) 
        {
            mdb = servis;   
            funkcije = new PomocneFunkcije(hca);
        }

        public async Task<ICollection<Jelo>> Jela()
        {
            var jelaKorisnika = await mdb.NadjiPoUslovu<Jelo>(jela, "korisnikID", funkcije.PrijavljenID());
            foreach (var jelo in jelaKorisnika)
                jelo.namirnice = await mdb.NadjiPoUslovu<JeloNamirnica>(jnamirnice, "jeloID", jelo.Id);

            return jelaKorisnika;
        }

        public async Task<ICollection<Jelo>> JelaPoNazivu(string naziv)
        {
            //  return (await Jela()).Where(j => j.Naziv.Contains(naziv) || naziv.Contains(j.Naziv)).ToList();
            return await mdb.NadjiSlicne<Jelo>(jela, "Naziv", naziv);
        }

        public async Task<Jelo> JeloPoNazivu(string naziv)
        {
            return (await mdb.NadjiPoUslovu<Jelo>(jela, "Naziv", naziv)).FirstOrDefault();
           // return (await Jela()).Where(j => j.Naziv.Contains(naziv) || naziv.Contains(j.Naziv)).FirstOrDefault();
        }

        public async Task<Jelo> JeloPoIDu(string id)
        {
            return await mdb.NadjiPoIDu<Jelo>(jela, id);
        }

        public async Task<Jelo> SkalirajJelo(Jelo j, double masa) 
        {
            Jelo jelo = new Jelo();

            jelo.Id = j.Id;
            jelo.Naziv = j.Naziv;
            jelo.Masa = masa;
            jelo.EnergetskaVrednost = Math.Round(j.EnergetskaVrednost * masa / j.Masa, 2);
            jelo.UgljeniHidrati = Math.Round(j.UgljeniHidrati * masa / j.Masa, 2);
            jelo.Protein = Math.Round(j.Protein * masa / j.Masa, 2);
            jelo.Mast = Math.Round(j.Mast * masa / j.Masa, 2);
            jelo.Recept = j.Recept;

            return jelo;
        }

        public async Task<ICollection<Namirnica>> NamirniceJela(Jelo jelo) 
        {
            List<Namirnica> namirnice = new List<Namirnica>();
            var jNamirnice = await mdb.NadjiPoUslovu<JeloNamirnica>(jnamirnice, "jeloID", jelo.Id);

            foreach (var jn in jNamirnice) 
            {
                var n = await mdb.NadjiPoIDu<Namirnica>("Namirnice", jn.namirnicaID); 

                double koef = jn.masa / 100;

                n.EnergetskaVrednost = Math.Ceiling(n.EnergetskaVrednost * koef);
                n.Protein = Math.Ceiling(n.Protein * koef);
                n.UgljeniHidrati = Math.Ceiling(n.UgljeniHidrati * koef);
                n.Mast = Math.Ceiling(n.Mast * koef);

                namirnice.Add(n);
            }

            return namirnice;
        }

        public async Task<double> MasaNamirniceUJelu(Jelo jelo, Namirnica namirnica) 
        {
            double masa = 0;
            var jn = (await mdb.NadjiPoUslovima<JeloNamirnica>(jnamirnice,
                new List<string> { "jeloID", "namirnicaID" }, new List<string> { jelo.Id, namirnica.Id })).FirstOrDefault();
           if (jn != null)
                masa = jn.masa;

            return masa; 
        }

        public async Task<ICollection<double>> MaseNamirnicaUJelu(Jelo jelo) 
        {
            var jNamirnice = await mdb.NadjiPoUslovu<JeloNamirnica>(jnamirnice, "jeloID", jelo.Id);
            List<double> mase = new List<double>();
            foreach (var jn in jNamirnice)
                mase.Add(jn.masa);

            return mase;
        }


        public async Task<Jelo> DodajNovoJelo(string naziv)
        {
            Jelo jelo = new Jelo();
            jelo.Naziv = naziv;
            jelo.korisnikID = funkcije.PrijavljenID();
            jelo.korisnik = (await mdb.NadjiPoUslovu<Korisnik>("Korisnici", "Id", funkcije.PrijavljenID())).FirstOrDefault();

            await mdb.Ubaci<Jelo>(jela, jelo);

            return jelo;
        }

        public async Task<Objava> ObjaviJelo(Jelo jelo)
        {
            Objava objava = new Objava();
            await NapisiRecept(jelo);
            objava.Tekst = jelo.Recept;
            objava.Vreme = DateTime.Now;
            objava.autorID = funkcije.PrijavljenID();
            objava.autor = await mdb.NadjiPoIDu<Korisnik>("Korisnici", objava.autorID);

            await mdb.Ubaci<Objava>("Objave", objava);

            return objava;
        }



        public async Task<Jelo> PromeniNazivJela(Jelo jelo, string noviNaziv)
        {
            jelo.Naziv = noviNaziv;
            return await mdb.Zameni<Jelo>(jela, jelo.Id, jelo);
        }

        public async Task<Jelo> DodajJeluNamirnicu(Jelo jelo, Namirnica namirnica, double masa, bool pre)
        {
            double koef = masa;
            if (pre)
                koef *= namirnica.PromenaMase;

            JeloNamirnica jn = new JeloNamirnica();
            jn.jelo = jelo;
            jn.jeloID = jelo.Id;
            jn.namirnica = namirnica;
            jn.namirnicaID = namirnica.Id;
            jn.masa = koef;

            jelo.Masa += Math.Ceiling(koef);
            koef = koef / 100;
            jelo.EnergetskaVrednost = Math.Round(jelo.EnergetskaVrednost + namirnica.EnergetskaVrednost * koef, 3);
            jelo.Protein = Math.Round(jelo.Protein + namirnica.Protein * koef, 3);
            jelo.UgljeniHidrati = Math.Round(jelo.UgljeniHidrati + namirnica.UgljeniHidrati * koef, 3);
            jelo.Mast = Math.Round(jelo.Mast + namirnica.Mast * koef, 3);

            await mdb.Ubaci<JeloNamirnica>(jnamirnice, jn);
            return await mdb.Zameni<Jelo>(jela, jelo.Id, jelo);
        }


        public async Task<Jelo> PromeniMasuNamirnice(Jelo jelo, Namirnica namirnica, double masa)
        {
            var jn = await mdb.NadjiPoveznik<JeloNamirnica>(jnamirnice, "jeloID", "namirnicaID", jelo.Id, namirnica.Id);

            if (jn == null) 
                return null;

             double koef =  (masa - jn.masa) / 100;

               jelo.EnergetskaVrednost = Math.Round(jelo.EnergetskaVrednost + namirnica.EnergetskaVrednost * koef, 3);
               jelo.Protein = Math.Round(jelo.Protein + namirnica.Protein * koef, 3);
               jelo.UgljeniHidrati = Math.Round(jelo.UgljeniHidrati + namirnica.UgljeniHidrati * koef, 3);
               jelo.Mast = Math.Round(jelo.Mast + namirnica.Mast * koef, 3);
               jelo.Masa = Math.Round(jelo.Masa + masa - jn.masa, 3);
               jn.masa = Math.Round(masa);

            await mdb.PromeniPoveznik<JeloNamirnica>(jnamirnice, "jeloID", "namirnicaID", jelo.Id, namirnica.Id, jn);
            return await mdb.Zameni<Jelo>(jela, jelo.Id, jelo);
        }


        public async Task<Jelo> NapisiRecept(Jelo jelo) 
        {
            jelo.Recept = "Recept: " + jelo.Naziv + Environment.NewLine;

            var namirniceJela = (await mdb.NadjiPoUslovu<JeloNamirnica>(jnamirnice, "jeloID", jelo.Id)).ToList();
            if (namirniceJela.Count == 0)
                return null;

            foreach (var n in namirniceJela) 
            {
                var namirnica = await mdb.NadjiPoIDu<Namirnica>("Namirnice", n.namirnicaID);
                var kcal = n.masa * namirnica.EnergetskaVrednost / 100;
                jelo.Recept += " - " + n.masa + " g " + namirnica.Naziv + " = " + kcal + " kcal" + Environment.NewLine;
            }

            jelo.Recept += "U " + jelo.Masa + " g ovog jela ima ukupno " + jelo.EnergetskaVrednost + " kcal.";

            return await mdb.Zameni<Jelo>(jela, jelo.Id, jelo);
        }

        public async Task<Jelo> ObrisiNamirnicuIzJela(Jelo jelo, Namirnica namirnica)
        {
            var jn = await mdb.NadjiPoveznik<JeloNamirnica>(jnamirnice, "jeloID", "namirnicaID", jelo.Id, namirnica.Id);
            if (jn == null)
                return null;

            double koef = jn.masa / 100;
            jelo.EnergetskaVrednost = Math.Round(jelo.EnergetskaVrednost - namirnica.EnergetskaVrednost * koef, 3);
            jelo.Protein = Math.Round(jelo.Protein - namirnica.Protein*koef, 3);
            jelo.UgljeniHidrati = Math.Round(jelo.UgljeniHidrati - namirnica.UgljeniHidrati, 3);
            jelo.Mast = Math.Round(jelo.Mast - namirnica.Mast, 3);
            jelo.Masa = Math.Round(jelo.Masa - jn.masa, 3);

            await mdb.ObrisiPoveznik<JeloNamirnica>(jnamirnice, "jeloID", "namirnicaID", jelo.Id, namirnica.Id);
            return await mdb.Zameni(jela, jelo.Id, jelo);
        }

        public async Task<string> ObrisiJelo(string id)
        {
            var jelo = await JeloPoIDu(id);
            if (jelo == null)
                return "Ne postoji traženo jelo.";

            await mdb.ObrisiVeze<JeloNamirnica>(jnamirnice, id, "jeloID");
            await mdb.ObrisiVeze<ObrokJelo>("ObrociJela", id, "jeloID");
            await mdb.Obrisi<Jelo>(jela, id);

            return "Jelo " + jelo.Naziv + " je obrisano.";
        }


    }
}
