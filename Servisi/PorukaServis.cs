namespace API.Servisi
{
    public class PorukaServis  : IPoruka
    {
        private IMongoDB mdb;
        public PomocneFunkcije funkcije;
        public const string poruke = "Poruke";
        public PorukaServis(IMongoDB servis, IHttpContextAccessor hca) 
        {  
            mdb = servis;
            funkcije = new PomocneFunkcije(hca);
        }

        public async Task<bool> Pratilac(string pratilacID)
        {
            var pratilac = await mdb.NadjiPoUslovima<DvaKorisnika>("ParoviKorisnika", 
                new List<string> { "pratilacID", "pracenID" }, new List<string> {pratilacID, funkcije.PrijavljenID() });
            if (pratilac == null)
                return false;
            else return true;
        }

        public async Task<bool> Pracen(string pracenID)
        {
            var pracen = await mdb.NadjiPoUslovima<DvaKorisnika>("ParoviKorisnika",
                new List<string> { "pratilacID", "pracenID" }, new List<string> { funkcije.PrijavljenID(), pracenID });
            if (pracen == null)
                return false;
            else return true;
        }

        public async Task<Poruka> NadjiPoslatuPoruku(string porukaID) 
        {
            var porukePrijavljenog = await mdb.NadjiPoUslovu<Poruka>(poruke, "autorID", funkcije.PrijavljenID());
            var poruka = await mdb.NadjiPoIDu<Poruka>(poruke, porukaID);  
            if (porukePrijavljenog == null || poruka == null || !porukePrijavljenog.Contains(poruka))
                return null;

            return poruka;
        }


        public async Task<ICollection<Poruka>> PrimljenePoruke()
        {
            return await mdb.NadjiPoUslovu<Poruka>(poruke, "primalacID", funkcije.PrijavljenID());
        }

        public async Task<ICollection<Poruka>> PoslatePoruke()
        {
            return await mdb.NadjiPoUslovu<Poruka>(poruke, "autorID", funkcije.PrijavljenID());
        }

        public async Task<ICollection<Poruka>> Razgovor(Korisnik korisnik)
        {
            var poslate = await mdb.NadjiPoUslovima<Poruka>(poruke, 
                new List<string> {"autorID", "primalacID" },
                new List<string> { funkcije.PrijavljenID(), korisnik.Id });

            var primljene = await mdb.NadjiPoUslovima<Poruka>(poruke,
                         new List<string> { "primalacID", "autorID" },
                          new List<string> { funkcije.PrijavljenID(), korisnik.Id });
                        

            primljene.AddRange(poslate);
            return primljene.OrderBy(p => p.Vreme).ToList();
        }

        public async Task<ICollection<bool>> AutoriPoruka(Korisnik korisnik) 
        {
            List<bool>redosled = new List<bool>();
            var razgovor = await Razgovor(korisnik);
            if (razgovor != null)
                foreach (var r in razgovor)
                    if (r.autorID == funkcije.PrijavljenID())
                        redosled.Add(true);
                    else redosled.Add(false);

            return redosled;
        }


        public async Task<ICollection<Korisnik>> Sagovornici() 
        {
            List<Korisnik> sagovornici = new List<Korisnik>();
            var razgovori = await mdb.NadjiPoUslovima<Poruka>(poruke,  new List<string> {"autorID", "primalacID" }, 
                new List<string> {funkcije.PrijavljenID(), funkcije.PrijavljenID() });

            foreach (var r in razgovori) 
            {
                var korisnik = new Korisnik();
                if (r.autorID == funkcije.PrijavljenID())
                    korisnik = await mdb.NadjiPoIDu<Korisnik>("Korisnici", r.primalacID); 
                else korisnik = await mdb.NadjiPoIDu<Korisnik>("Korisnici", r.autorID);

                if (!sagovornici.Contains(korisnik))
                    sagovornici.Add(korisnik);
            }

            return sagovornici;
        }

        public async Task<Poruka> PosaljiPoruku(Korisnik korisnik, string tekst)
        {
            Poruka poruka = new Poruka(funkcije.ParsirajUnos(tekst) , DateTime.Now);
            poruka.autorID = funkcije.PrijavljenID();
            poruka.autor = await mdb.NadjiPoIDu<Korisnik>("Korisnici", poruka.autorID);
            poruka.primalac = korisnik;
            poruka.primalacID = korisnik.Id;

            await mdb.Ubaci<Poruka>(poruke, poruka);
            return poruka;
        }

        public async Task<Poruka> PrepraviPoruku(string porukaID, string tekst) 
        {
            var poruka = await NadjiPoslatuPoruku(porukaID);
            if (poruka == null)
                return null;

            poruka.Tekst = funkcije.ParsirajUnos(tekst);
            return await mdb.Zameni<Poruka>(poruke, poruka.Id, poruka);
        }


        public async Task<string> ObrisiPoruku(string porukaID)
        {
            var poruka = await NadjiPoslatuPoruku(porukaID);
            if (poruka == null)
                return "Tražena poruka nije poslata.";

            await mdb.Obrisi<Poruka>(poruke, porukaID);
            return "Poruka je obrisana.";
        }

        public async Task<string> ObrisiRazgovor(Korisnik korisnik)
        {
            var razgovor = await Razgovor(korisnik);
            if(razgovor.Count == 0)
                return "Nema poruka razmenjenih sa korisnikom " + korisnik.Ime + ".";

            await mdb.ObrisiListu<Poruka>(poruke, razgovor);
  
            return "Razgovor sa korisnikom " + korisnik.Ime + " je obrisan.";
        }

        public async Task<string> ObrisiRazgovore() 
        {
            var sagovornici = await Sagovornici();
            foreach (var s in sagovornici)
                await ObrisiRazgovor(s);

            return "Svi razgovori su obrisani.";
        }
        public async Task<string> ObrisiPoslatePoruke() 
        {
            var poslate = await PoslatePoruke();
            if(poslate != null)
                await mdb.ObrisiListu(poruke, poslate);

            return "Sve poslate poruke su obrisane.";
        }


    }
}
