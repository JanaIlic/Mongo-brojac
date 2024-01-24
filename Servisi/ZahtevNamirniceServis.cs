namespace API.Servisi
{
    public class ZahtevNamirniceServis : IZahtevNamirnice
    {
        private IMongoDB mdb;
        public PomocneFunkcije funkcije;
        public const string zahtevin = "ZahteviNamirnica";
        public ZahtevNamirniceServis(IMongoDB servis, IHttpContextAccessor hca) 
        { 
            mdb = servis;
            funkcije = new PomocneFunkcije(hca);
        }


        public async Task<ICollection<ZahtevNamirnice>> PrimljeniZahtevi()
        {
            return await mdb.UcitajListu<ZahtevNamirnice>(zahtevin);
        }

        public async Task<ICollection<ZahtevNamirnice>> NoviPrimljeniZahtevi()
        {
            return (await PrimljeniZahtevi()).Where(z => z.Stanje == StanjeZahteva.Cekanje || z.Stanje == StanjeZahteva.Obrada).ToList();
        }
        public async Task<ICollection<ZahtevNamirnice>> ZakljuceniPrimljeniZahtevi()
        {
            return (await PrimljeniZahtevi()).Where(z => z.Stanje == StanjeZahteva.Odbijen || z.Stanje == StanjeZahteva.Ispunjen).ToList();
        }

        public async Task<ICollection<ZahtevNamirnice>> PoslatiZahtevi()
        {
            return await mdb.NadjiPoUslovu<ZahtevNamirnice>(zahtevin, "podnosilacID", funkcije.PrijavljenID());
        }

        public async Task<ZahtevNamirnice> NadjiPoslatZahtev(string zahtevID)
        {
            var zahtev = await mdb.NadjiPoIDu<ZahtevNamirnice>(zahtevin, zahtevID);
            if (zahtev == null || !(await PoslatiZahtevi()).Contains(zahtev))
                return null;

            return zahtev;
        }

        public async Task<ZahtevNamirnice> NadjiPrimljenZahtev(string zahtevID)
        {
            var zahtev = await mdb.NadjiPoIDu<ZahtevNamirnice>(zahtevin, zahtevID);
            if (zahtev == null || !(await PrimljeniZahtevi()).Contains(zahtev))
                return null;

            return zahtev;
        }

        public async Task<ICollection<ZahtevNamirnice>> NadjiPoslateZahteve(string naziv)
        {
            var slicni = await mdb.NadjiSlicne<ZahtevNamirnice>(zahtevin, "NazivNamirnice", naziv);
            return slicni.Intersect(await PoslatiZahtevi()).ToList();
        }

        public async Task<ICollection<ZahtevNamirnice>> NadjiPrimljeneZahteve(string naziv)
        {
            var slicni = await mdb.NadjiSlicne<ZahtevNamirnice>(zahtevin, "NazivNamirnice", naziv);
            return slicni.Intersect(await PrimljeniZahtevi()).ToList();
        }

        public async Task<bool> PrijavaPoslednjeg() 
        {
            var zahtev = (await mdb.NadjiPoUslovu<ZahtevNamirnice>(zahtevin, "podnosilacID", funkcije.PrijavljenID()))
                    .OrderBy(z => z.Podnet).LastOrDefault();
            if (zahtev == null)
                return true;

            return zahtev.Prijava;
        }

        public async Task<ZahtevNamirnice> PosaljiZahtev(string naziv, bool prijava, string napomena)
        {
            ZahtevNamirnice zahtev = new ZahtevNamirnice(naziv);
            zahtev.podnosilacID = funkcije.PrijavljenID();

            var podneti = await mdb.NadjiPoUslovima<ZahtevNamirnice>(zahtevin, new List<string> {"podnosilacID", "NazivNamirnice" }, 
                new List<string> {zahtev.podnosilacID, naziv });
            if (podneti.Count > 0)             
                return null;

            zahtev.podnosilac = await mdb.NadjiPoIDu<Korisnik>("Korisnci", zahtev.podnosilacID);
            zahtev.admin = (await mdb.UcitajListu<AdministratorNamirnica>("AdminiNamirnica")).FirstOrDefault();
            zahtev.adminID = zahtev.admin.Id;
            zahtev.Podnet = DateTime.Now;

            if (!napomena.Trim().Equals(string.Empty) || !napomena.Equals("*"))
                zahtev.Poruka = napomena;

            await mdb.Ubaci<ZahtevNamirnice>(zahtevin, zahtev);

            if (prijava)
            {
                zahtev.Prijava = true;
                zahtev.rezultat = await DodajRezultat(zahtev.Id);
                zahtev.rezultatID = zahtev.rezultat.Id;
            }
            else zahtev.Prijava = false;

            return await mdb.Zameni<ZahtevNamirnice>(zahtevin, zahtev.Id, zahtev);
        }



        public async Task<string> PovuciZahtev(string zahtevID)
        {
            var zahtev = await NadjiPoslatZahtev(zahtevID);
            if (zahtev == null)
                return "Ne postoji traženi zahtev.";

            if (zahtev.Stanje != StanjeZahteva.Cekanje)
                return "Admin je prihvatio zahtev i više se ne može povući.";

             await mdb.Obrisi<ZahtevNamirnice>(zahtevin, zahtev.Id);

            return "Zahtev je povučen.";
        }

        public async Task<ZahtevNamirnice> PrihvatiZahtev(ZahtevNamirnice zahtev)
        {
            if (zahtev.Stanje > StanjeZahteva.Cekanje)
                return null;

            zahtev.Prihvacen = DateTime.Now;
            zahtev.Stanje = StanjeZahteva.Obrada;

            if (zahtev.Prijava)
            {
                var rezultat = await RezultatZahteva(zahtev);
                rezultat.Poruka += "Zahtev je prihvaćen i nalazi se u stanju obrade " + funkcije.DatumVremeToString(zahtev.Prihvacen) + " ." + Environment.NewLine;
                await mdb.Zameni<RezultatZahteva>("RezultatiZahteva", rezultat.Id, rezultat);
            }

            return await mdb.Zameni<ZahtevNamirnice>(zahtevin, zahtev.Id, zahtev);
        }

        public async Task<ZahtevNamirnice> IspuniZahtev(ZahtevNamirnice zahtev, ICollection<Namirnica> namirnice)
        {
            if (zahtev.Stanje > StanjeZahteva.Obrada)
                return null;

            foreach (Namirnica n in namirnice)
            {
                NamirnicaZahtev nz = new NamirnicaZahtev();
                nz.namirnica = n;
                nz.namirnicaID = n.Id;
                nz.zahtev = zahtev;
                nz.zahtevID = zahtev.Id;

                await mdb.Ubaci<NamirnicaZahtev>("ZahtevaneNamirnice", nz);
            }

            zahtev.Stanje = StanjeZahteva.Ispunjen;
            zahtev.Zakljucen = DateTime.Now;

            if (zahtev.Prijava)
            {
                var rezultat = await RezultatZahteva(zahtev);
                rezultat.Poruka += "Zahtev je ispunjen " + funkcije.DatumVremeToString(zahtev.Zakljucen) +
                    " . Dodate namirnice: " + Environment.NewLine;
                foreach (var n in namirnice)
                    rezultat.Poruka += n.Naziv + Environment.NewLine;

                await mdb.Zameni<RezultatZahteva>("RezultatiZahteva", rezultat.Id, rezultat);
            }

            return await mdb.Zameni<ZahtevNamirnice>(zahtevin, zahtev.Id, zahtev);
        }

        public async Task<ZahtevNamirnice> OdbijZahtev(ZahtevNamirnice zahtev)
        {
            if (zahtev.Stanje > StanjeZahteva.Obrada)
                return null;

            zahtev.Stanje = StanjeZahteva.Odbijen;
            zahtev.Zakljucen = DateTime.Now;

            if (zahtev.Prijava)
            {
                var rezultat = await RezultatZahteva(zahtev);
                rezultat.Poruka += "Administrator je odbio zahtev " + funkcije.DatumVremeToString(zahtev.Zakljucen) +
                    " jer nije pronašao namirnice slične " + zahtev.NazivNamirnice + Environment.NewLine;

                await mdb.Zameni<RezultatZahteva>("RezultatiZahteva", rezultat.Id, rezultat );
            }

            return await mdb.Zameni<ZahtevNamirnice>(zahtevin, zahtev.Id, zahtev);
        }


        public async Task<ICollection<RezultatZahteva>> RezultatiZahteva()
        {
            var zahtevi = (await PoslatiZahtevi()).OrderByDescending(d => d.Podnet).ToList();
            if (zahtevi.Count == 0)
                return null;

            ICollection<RezultatZahteva> rezultati = new List<RezultatZahteva>();
            foreach (var z in zahtevi)
                rezultati.Add((await mdb.NadjiPoUslovu<RezultatZahteva>("RezultatiZahteva", "zahtevID", z.Id)).FirstOrDefault());

            return rezultati;
        }



        public async Task<RezultatZahteva> RezultatZahteva(ZahtevNamirnice zahtev)
        {
            var rezultat = await mdb.NadjiPoIDu<RezultatZahteva>("RezultatiZahteva", zahtev.rezultatID);
            if (rezultat == null)
                return null;

            return rezultat;
        }


        public async Task<RezultatZahteva> DodajRezultat(string zahtevID)
        {
            RezultatZahteva rezultat = new RezultatZahteva();
            rezultat.zahtevID = zahtevID;
            rezultat.zNamirnice = await NadjiPoslatZahtev(zahtevID);

            var zahtev = await NadjiPoslatZahtev(zahtevID);
            var podnosilac = await mdb.NadjiPoIDu<Korisnik>("Korisnici", zahtev.podnosilacID);
            rezultat.zNamirnice = zahtev;

            string napomena = String.Empty;
            napomena = zahtev.Poruka.Trim();
            if (!napomena.Equals(String.IsNullOrEmpty) && !napomena.Equals("*"))
                napomena = "," + Environment.NewLine + " uz napomenu: " + napomena + Environment.NewLine;
            else napomena = "." + Environment.NewLine;

            rezultat.Poruka = podnosilac.Ime + " podneo je zahtev za " + zahtev.NazivNamirnice + " " + funkcije.DatumVremeToString(zahtev.Podnet)
                    + napomena + "Zahtev je na čekanju." + Environment.NewLine;

            await mdb.Ubaci<RezultatZahteva>("RezultatiZahteva", rezultat);

            return rezultat;
        }


    }
}
