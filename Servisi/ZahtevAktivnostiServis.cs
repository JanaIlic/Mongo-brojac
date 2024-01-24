namespace API.Servisi
{
    public class ZahtevAktivnostiServis : IZahtevAktivnosti
    {
        private IMongoDB mdb;
        public PomocneFunkcije funkcije;
        public const string zahtevia = "ZahteviAktivnosti";
        public ZahtevAktivnostiServis(IMongoDB servis, IHttpContextAccessor hca) 
        { 
            mdb = servis;
            funkcije = new PomocneFunkcije(hca);
        }

        public async Task<ICollection<ZahtevAktivnosti>> PrimljeniZahtevi()
        {
            return await mdb.UcitajListu<ZahtevAktivnosti>(zahtevia);
        }

        public async Task<ICollection<ZahtevAktivnosti>> NoviPrimljeniZahtevi() 
        {
            return (await PrimljeniZahtevi()).Where(z => z.Stanje == StanjeZahteva.Cekanje || z.Stanje == StanjeZahteva.Obrada).ToList();
            //context.ZahteviAktivnosti.Where(z => z.Stanje == StanjeZahteva.Cekanje || z.Stanje == StanjeZahteva.Obrada).ToListAsync();   
        }
        public async Task<ICollection<ZahtevAktivnosti>> ZakljuceniPrimljeniZahtevi() 
        {
            return (await PrimljeniZahtevi()).Where(z => z.Stanje == StanjeZahteva.Odbijen || z.Stanje == StanjeZahteva.Ispunjen).ToList();
        }

        public async Task<ICollection<ZahtevAktivnosti>> PoslatiZahtevi()
        {
            return await mdb.NadjiPoUslovu<ZahtevAktivnosti>(zahtevia, "podnosilacID", funkcije.PrijavljenID());
                //context.ZahteviAktivnosti.Where(z => z.podnosilacID == funkcije.PrijavljenID()).ToListAsync();
        }

        public async Task<ZahtevAktivnosti> NadjiPoslatZahtev(string zahtevID)
        {
            var zahtev = await mdb.NadjiPoIDu<ZahtevAktivnosti>(zahtevia, zahtevID);
            if(zahtev == null || !(await PoslatiZahtevi()).Contains(zahtev))
                return null;

            return zahtev;
        }

        public async Task<ZahtevAktivnosti> NadjiPrimljenZahtev(string zahtevID)
        {
            var zahtev = await mdb.NadjiPoIDu<ZahtevAktivnosti>(zahtevia, zahtevID);
            if (zahtev == null || !(await PrimljeniZahtevi()).Contains(zahtev))
                return null;

            return zahtev;
        }

        public async Task<ICollection<ZahtevAktivnosti>> NadjiPoslateZahteve(string naziv)
        {
            var slicni = await mdb.NadjiSlicne<ZahtevAktivnosti>(zahtevia, "NazivAktivnosti", naziv);
            return slicni.Intersect(await PoslatiZahtevi()).ToList();
        }

        public async Task<ICollection<ZahtevAktivnosti>> NadjiPrimljeneZahteve(string naziv)
        {
            var slicni = await mdb.NadjiSlicne<ZahtevAktivnosti>(zahtevia, "NazivAktivnosti", naziv);
            return slicni.Intersect(await PrimljeniZahtevi()).ToList();
        }

        public async Task<bool> PrijavaPoslednjeg()
        {
            var zahtev = (await mdb.NadjiPoUslovu<ZahtevAktivnosti>(zahtevia, "podnosilacID", funkcije.PrijavljenID()))
                    .OrderBy(z => z.Podnet).LastOrDefault();
            if (zahtev == null)
                return true;

            return zahtev.Prijava;
        }

        public async Task<ZahtevAktivnosti> PosaljiZahtev(string naziv, bool prijava, string napomena) 
        {
            ZahtevAktivnosti zahtev = new ZahtevAktivnosti(naziv);
            zahtev.podnosilacID = funkcije.PrijavljenID();

            var podneti = await mdb.NadjiPoUslovima<ZahtevAktivnosti>(zahtevia,new List<string> {"podnosilacID", "NazivAktivnosti"},
                new List<string> {zahtev.podnosilacID, naziv } );
   
            if(podneti.Count > 0)
                return null;

            zahtev.podnosilac = await mdb.NadjiPoIDu<Korisnik>("Korisnci", zahtev.podnosilacID);
            zahtev.admin = (await mdb.UcitajListu<AdministratorAktivnosti>("AdminiAktivnosti")).FirstOrDefault() ;
            zahtev.adminID = zahtev.admin.Id;
            zahtev.Podnet = DateTime.Now;

            if (!napomena.Trim().Equals(string.Empty) || !napomena.Equals("*"))
                zahtev.Poruka = napomena;

            await mdb.Ubaci<ZahtevAktivnosti>(zahtevia, zahtev);

            if (prijava)
            {
                zahtev.Prijava = true;
                zahtev.rezultat = await DodajRezultat(zahtev.Id);
                zahtev.rezultatID = zahtev.rezultat.Id;
            }
            else zahtev.Prijava = false;

            return await mdb.Zameni<ZahtevAktivnosti>(zahtevia, zahtev.Id, zahtev);
        }



        public async Task<string> PovuciZahtev(string zahtevID)
        {
            var zahtev = await NadjiPoslatZahtev(zahtevID);
            if (zahtev == null)
                return "Traženi zahtev ne postoji, ili je uklonjen.";

            if (zahtev.Stanje != StanjeZahteva.Cekanje)
                return "Admin je prihvatio zahtev i više se ne može povući.";

            await mdb.Obrisi<ZahtevAktivnosti>(zahtevia, zahtevID);

            return "Zahtev je povučen.";
        }

        public async Task<ZahtevAktivnosti> PrihvatiZahtev(ZahtevAktivnosti zahtev)
        {
            if (zahtev.Stanje > StanjeZahteva.Cekanje)
                return null;

            zahtev.Prihvacen = DateTime.Now;
            zahtev.Stanje = StanjeZahteva.Obrada;

            if (zahtev.Prijava) 
            {
                var rezultat = await RezultatZahteva(zahtev);
                rezultat.Poruka += " Zahtev je prihvaćen i nalazi se u stanju obrade " + funkcije.DatumVremeToString(zahtev.Prihvacen) + " ." + Environment.NewLine;
            }

            return await mdb.Zameni<ZahtevAktivnosti>(zahtevia, zahtev.Id, zahtev);
        }

        public async Task<ZahtevAktivnosti> IspuniZahtev(ZahtevAktivnosti zahtev, ICollection<Aktivnost>aktivnosti)
        {
            if (zahtev.Stanje > StanjeZahteva.Obrada)
                return null;


            foreach (Aktivnost a in aktivnosti) 
            {
                AktivnostZahtev az = new AktivnostZahtev();
                az.aktivnost = a;
                az.aktivnostID = a.Id;
                az.zahtev = zahtev;
                az.zahtevID = zahtev.Id;
                await mdb.Ubaci<AktivnostZahtev>("ZahtevaneAktivnosti", az);
            }

            zahtev.Stanje = StanjeZahteva.Ispunjen;
            zahtev.Zakljucen = DateTime.Now;

            if (zahtev.Prijava)
            {
                var rezultat = await RezultatZahteva(zahtev);
                rezultat.Poruka += " Zahtev je ispunjen " + funkcije.DatumVremeToString(zahtev.Zakljucen) + 
                    " . Dodate aktivnosti: " + Environment.NewLine;
                foreach (var a in aktivnosti)
                    rezultat.Poruka += a.Naziv + Environment.NewLine;
            }

            return await mdb.Zameni<ZahtevAktivnosti>(zahtevia, zahtev.Id, zahtev);
        }

        public async Task<ZahtevAktivnosti> OdbijZahtev(ZahtevAktivnosti zahtev)
        {
            if (zahtev.Stanje > StanjeZahteva.Obrada )
                return null; 

            zahtev.Stanje = StanjeZahteva.Odbijen;
            zahtev.Zakljucen = DateTime.Now;

            if (zahtev.Prijava)
            {
                var rezultat = await RezultatZahteva(zahtev);
                rezultat.Poruka += " Administrator je odbio zahtev " + funkcije.DatumVremeToString(zahtev.Zakljucen) +
                    " jer nije pronašao aktivnosti slične " + zahtev.NazivAktivnosti + Environment.NewLine;
            }

            return await mdb.Zameni<ZahtevAktivnosti>(zahtevia, zahtev.Id, zahtev);
        }



        public async Task<ICollection<RezultatZahteva>> RezultatiZahteva()
        {
            var zahtevi = (await PoslatiZahtevi()).OrderByDescending(d=>d.Podnet).ToList();
            if (zahtevi.Count == 0)
                return null;

            ICollection<RezultatZahteva> rezultati = new List<RezultatZahteva>();
            foreach (var z in zahtevi)
                rezultati.Add((await mdb.NadjiPoUslovu<RezultatZahteva>("RezultatiZahteva", "zahtevID", z.Id)).FirstOrDefault() );
                    //context.RezultatiZahteva.Where(r => r.zahtevID == z.Id).FirstOrDefaultAsync());

            return rezultati;
        }

        public async Task<RezultatZahteva> RezultatZahteva(ZahtevAktivnosti zahtev) 
        {
            var rezultat = await mdb.NadjiPoIDu<RezultatZahteva>("RezultatiZahteva", zahtev.rezultatID);
                //context.RezultatiZahteva.FindAsync(zahtev.rezultatID);
            if (rezultat == null)
                return null;

            return rezultat;
        }



        public async Task<RezultatZahteva> DodajRezultat(string zahtevID)
        {

            RezultatZahteva rezultat = new RezultatZahteva();
            rezultat.zahtevID = zahtevID;
            var zahtev = await NadjiPoslatZahtev(zahtevID);
            var podnosilac = await mdb.NadjiPoIDu<Korisnik>("Korisnici", zahtev.podnosilacID);
            rezultat.podnosilacID = zahtev.podnosilacID;
            rezultat.zAktivnosti = zahtev;

            string napomena = String.Empty;
             napomena = zahtev.Poruka.Trim();
            if (!napomena.Equals(String.IsNullOrEmpty) && !napomena.Equals("*"))
                napomena = ","+ Environment.NewLine +" uz napomenu: " + napomena + Environment.NewLine;
            else napomena = "." + Environment.NewLine;

                rezultat.Poruka = podnosilac.Ime + " podneo je zahtev za " + zahtev.NazivAktivnosti + " " + funkcije.DatumVremeToString(zahtev.Podnet) 
                    + napomena + Environment.NewLine + " Zahtev je na čekanju." + Environment.NewLine;


            await mdb.Ubaci<RezultatZahteva>("RezultatiZahteva", rezultat);
            return rezultat;
        }

    }
}
