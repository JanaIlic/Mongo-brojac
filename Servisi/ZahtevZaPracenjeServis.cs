namespace API.Servisi
{
    public class ZahtevZaPracenjeServis : IZahtevZaPracenje
    {
        private IMongoDB mdb;
        public PomocneFunkcije funkcije;
        public const string zahtevip = "ZahteviZaPracenje";
        public const string korisnici = "Korisnici";
        public const string parovi = "ParoviKorisnika";
        public ZahtevZaPracenjeServis(IMongoDB servis, IHttpContextAccessor hca)
        {
            mdb = servis;
            funkcije = new PomocneFunkcije(hca);
        }

        public async Task<ICollection<ZahtevZaPracenje>> PrimljeniZahtevi()
        {
            return (await mdb.NadjiPoUslovu<ZahtevZaPracenje>(zahtevip, "pracenID", funkcije.PrijavljenID())).
                OrderByDescending(z => z.Podnet).ToList();
        }

        public async Task<ICollection<ZahtevZaPracenje>> PoslatiZahtevi()
        {
            return (await mdb.NadjiPoUslovu<ZahtevZaPracenje>(zahtevip, "podnosilacID", funkcije.PrijavljenID())).
              OrderByDescending(z =>  z.Podnet).ToList();
        }

        public async Task<ZahtevZaPracenje> NadjiPoslatZahtev(string zahtevID)
        {
            var zahtev = await mdb.NadjiPoIDu<ZahtevZaPracenje>(zahtevip, zahtevID);
            if (zahtev == null || !(await PoslatiZahtevi()).Contains(zahtev))
                return null;

            return zahtev;
        }

        public async Task<ZahtevZaPracenje> NadjiPrimljenZahtev(string zahtevID)
        {
            var zahtev = await mdb.NadjiPoIDu<ZahtevZaPracenje>(zahtevip, zahtevID);
            if (zahtev == null || !(await PrimljeniZahtevi()).Contains(zahtev))
                return null;

            return zahtev;
        }

        public async Task<ICollection<ZahtevZaPracenje>> NadjiPoslateZahteve(string ime)
        {
            var slicni = await mdb.NadjiSlicne<ZahtevZaPracenje>(zahtevip, "Ime", ime);
            return slicni.Intersect(await PoslatiZahtevi()).ToList();
        }

        public async Task<ICollection<ZahtevZaPracenje>> NadjiPrimljeneZahteve(string ime)
        {
            var slicni = await mdb.NadjiSlicne<ZahtevZaPracenje>(zahtevip, "Ime", ime);
            return slicni.Intersect(await PrimljeniZahtevi()).ToList();
        }

        public async Task<ICollection<string>> Primaoci() 
        {
            List<string> primaoci = new List<string>();
            var poslatiZahtevi = await PoslatiZahtevi();
            foreach (var z in poslatiZahtevi)
                primaoci.Add((await mdb.NadjiPoIDu<Korisnik>(korisnici, z.pracenID)).Ime) ;

            return primaoci;
        }
        public async Task<ICollection<string>> Podnosioci() 
        { 
            List<string> podnosioci = new List<string>();
            var primljeniZahtevi = await PrimljeniZahtevi();
            foreach (var z in primljeniZahtevi)
                podnosioci.Add((await mdb.NadjiPoIDu<Korisnik>(korisnici, z.podnosilacID)).Ime);

            return podnosioci;
        }

        public async Task<bool> Pratilac(string pratilacID)
        {
            var pratilac = await mdb.NadjiPoUslovima<DvaKorisnika>(parovi, new List<string> {"pratilacID", "pracenID" }, 
                new List<string> {pratilacID, funkcije.PrijavljenID() });
            if ( pratilac.Count == 0 )
                return false;
            else return true;
        }

        public async Task<bool> Pracen(string pracenID) 
        {
            var pracen = await mdb.NadjiPoUslovima<DvaKorisnika>(parovi, new List<string> { "pratilacID", "pracenID" },
                    new List<string> { funkcije.PrijavljenID(), pracenID });
            if (pracen == null)
                return false;
            else return true;           
        }

        public async Task<bool> PrijavaPoslednjeg()
        {
            var zahtev = (await mdb.NadjiPoUslovu<ZahtevAktivnosti>(zahtevip, "podnosilacID", funkcije.PrijavljenID()))
                    .OrderBy(z => z.Podnet).LastOrDefault();
        
            if (zahtev == null)
                return true;

            return zahtev.Prijava;
        }

        public async Task<bool> ZahtevPoslatKorisniku(Korisnik primalac) 
        {
            var poslat = false;
            var poslati = await PoslatiZahtevi();
            foreach(var z in poslati)
                if(z.pracenID == primalac.Id && z.Stanje.Equals(StanjeZahteva.Cekanje))
                    poslat = true;

            return poslat;
        }

        public async Task<ZahtevZaPracenje> PosaljiZahtev(Korisnik korisnik, bool prijava, string pozdrav)
        {
            ZahtevZaPracenje zahtev = new ZahtevZaPracenje();
            zahtev.podnosilac = await mdb.NadjiPoIDu<Korisnik>(korisnici, funkcije.PrijavljenID());
            zahtev.podnosilacID = funkcije.PrijavljenID();
            zahtev.pracen = korisnik;
            zahtev.pracenID = korisnik.Id;

            var poslati = await mdb.NadjiPoUslovu<ZahtevZaPracenje>(zahtevip, "podnosilacID", zahtev.podnosilacID);
            var primljeni = await mdb.NadjiPoUslovu<ZahtevZaPracenje>(zahtevip, "pracenID", zahtev.pracenID);
            
            if (poslati.Intersect(primljeni).Where(z => z.Stanje == StanjeZahteva.Cekanje ||
                                                        z.Stanje == StanjeZahteva.Obrada) != null )
                return null;

            zahtev.Stanje = StanjeZahteva.Cekanje;
            zahtev.Podnet = DateTime.Now;

            if (!pozdrav.Trim().Equals(string.Empty) || !pozdrav.Equals("*"))
                zahtev.Poruka = pozdrav;

            await mdb.Ubaci<ZahtevZaPracenje>(zahtevip, zahtev);

            
            if (prijava)
            {
                zahtev.Prijava = true;
                zahtev.rezultat = await DodajRezultat(zahtev.Id);
                zahtev.rezultatID = zahtev.rezultatID;
            }
            else zahtev.Prijava = false;

            return await mdb.Zameni<ZahtevZaPracenje>(zahtevip, zahtev.Id, zahtev);
        }


        public async Task<string> PovuciZahtev(string zahtevID)
        {
            var zahtev = await NadjiPoslatZahtev(zahtevID);
            if (zahtev == null)
                return "Ne postoji zahtev sa ID-em " + zahtevID + ".";

            if (zahtev.Stanje != StanjeZahteva.Cekanje)
                return "Korisnik je već prihvatio zahtev i više se ne može povući. Možeš otpratiti korisnika. ";

            await mdb.Obrisi<ZahtevZaPracenje>(zahtevip, zahtevID);    
            return "Zahtev je povučen.";
        }

        public async Task<ZahtevZaPracenje> PrihvatiZahtev(ZahtevZaPracenje zahtev)
        {
            if (zahtev.Stanje != StanjeZahteva.Cekanje)
                return null;

            zahtev.Stanje = StanjeZahteva.Ispunjen;
            zahtev.Zakljucen = DateTime.Now;

            DvaKorisnika par = new DvaKorisnika();
            par.pratilac = await mdb.NadjiPoIDu<Korisnik>(korisnici, zahtev.podnosilacID);
            par.pratilacID = zahtev.podnosilacID;
            par.pracen = await mdb.NadjiPoIDu<Korisnik>(korisnici, zahtev.pracenID);
            par.pracenID = zahtev.pracenID;
            await mdb.Ubaci<DvaKorisnika>(parovi, par);

            if (zahtev.Prijava) 
            {
                var rezultat = await RezultatPoslatogZahteva(zahtev);
                rezultat.Poruka += "Zahtev za praćenje je prihvaćen "+ funkcije.DatumVremeToString(zahtev.Zakljucen) + 
                    ". Korisnik " + par.pratilac.Ime + " sada prati korisnika " + par.pracen.Ime + "." ;
                await mdb.Zameni<RezultatZahteva>("RezultatiZahteva", rezultat.Id, rezultat);
            }

            return await mdb.Zameni<ZahtevZaPracenje>(zahtevip, zahtev.Id, zahtev);
        }

        public async Task<ZahtevZaPracenje> OdbijZahtev(ZahtevZaPracenje zahtev)
        {
            if (zahtev.Stanje != StanjeZahteva.Cekanje)
                return null;

            zahtev.Stanje = StanjeZahteva.Odbijen;
            zahtev.Zakljucen = DateTime.Now;

            if (zahtev.Prijava)
            {
                var rezultat = await RezultatPoslatogZahteva(zahtev);
                var pracen = await mdb.NadjiPoIDu<Korisnik>(korisnici, zahtev.pracenID);
                var podnosilac = await mdb.NadjiPoIDu<Korisnik>(korisnici, zahtev.podnosilacID);
                rezultat.Poruka +=  "Korisnik " + pracen.Ime + " je " + funkcije.DatumVremeToString(zahtev.Zakljucen)
                    + " odbio zahtev za praćenje koji je poslao korisnik " + podnosilac.Ime + Environment.NewLine;

                await mdb.Zameni<RezultatZahteva>("RezultatiZahteva", rezultat.Id, rezultat);
            }

            return await mdb.Zameni<ZahtevZaPracenje>(zahtevip, zahtev.Id, zahtev);
        }

        public async Task<string> Otrprati(string pracenID) 
        {       
            if (! await Pracen(pracenID))
                return "Ne pratiš ovog korisnika.";

            await mdb.ObrisiVeze<DvaKorisnika>(parovi, pracenID, "pracenID"  );

            var pracen = await mdb.NadjiPoIDu<Korisnik>(korisnici, pracenID); 
            return "Više ne pratiš korisnika " + pracen.Ime + ".";
        }

        public async Task<string> ObrisiPratioca(string pratilacID) 
        {
            if (! await Pracen(pratilacID))
                return "Korisnik te ne prati.";

            await mdb.ObrisiVeze<DvaKorisnika>(parovi, pratilacID, "pratilacID");

            var pratilac = await mdb.NadjiPoIDu<Korisnik>(korisnici, pratilacID);
            return "Korisnik "+ pratilac.Ime + " te više ne prati.";
        }



        public async Task<ICollection<RezultatZahteva>> RezultatiZahteva()
        {
            var zahtevi = (await PoslatiZahtevi()).OrderByDescending(d => d.Podnet).ToList();
            if (zahtevi.Count == 0)
                return null;

            ICollection<RezultatZahteva> rezultati = new List<RezultatZahteva>();
            foreach (var z in zahtevi)
                rezultati.Add((await mdb.NadjiPoUslovu<RezultatZahteva>("RezultatiZahteva", "zahtevID", z.Id)).FirstOrDefault());
            //context.RezultatiZahteva.Where(r => r.zahtevID == z.Id).FirstOrDefaultAsync());

            return rezultati;
        }

        public async Task<RezultatZahteva> RezultatPoslatogZahteva(ZahtevZaPracenje zahtev)
        {
            var rezultat = await mdb.NadjiPoIDu<RezultatZahteva>("RezulatiZahteva", zahtev.rezultatID);
            if (rezultat == null)
                return null;

            return rezultat;
        }

     /*   public async Task<RezultatZahteva> RezultatPrimljenogZahteva(ZahtevZaPracenje zahtev)
        {
            var rezultat = await context.RezultatiZahteva.FindAsync(zahtev.rezultatID);
          
            if (rezultat == null)
                return null;

            return rezultat;
        }
     */

        public async Task<RezultatZahteva> DodajRezultat(string zahtevID)
        {
            RezultatZahteva rezultat = new RezultatZahteva();
            rezultat.zahtevID = zahtevID;
            var zahtev = await NadjiPoslatZahtev(zahtevID);
            rezultat.zPracenja = zahtev;

            var podnosilac = await mdb.NadjiPoIDu<Korisnik>(korisnici, zahtev.podnosilacID);
            var pracen = await mdb.NadjiPoIDu<Korisnik>(korisnici, zahtev.pracenID);

            string pozdrav = "";
            pozdrav = zahtev.Poruka.Trim();
            if (!pozdrav.Trim().Equals(String.IsNullOrEmpty) && !pozdrav.Equals("*"))
                pozdrav = "," + Environment.NewLine + " uz poruku: " + pozdrav + Environment.NewLine;
            else pozdrav = "." + Environment.NewLine;

            rezultat.Poruka = podnosilac.Ime + " podneo je zahtev za praćenje " + pracen.Ime + " " + funkcije.DatumVremeToString(zahtev.Podnet) 
                     + pozdrav + "Zahtev je na čekanju." + Environment.NewLine;

            await mdb.Ubaci<RezultatZahteva>("RezultatiZahteva", rezultat);
            return rezultat;
        }

    }
}
