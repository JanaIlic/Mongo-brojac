namespace API.Servisi
{
    public class DanServis : IDan
    {
        private IMongoDB mdb;
        public PomocneFunkcije funkcije;
        public const string dani = "Dani";

        public DanServis(IMongoDB servis, IHttpContextAccessor hca) 
        {
              mdb = servis;
              funkcije = new PomocneFunkcije(hca);
        }


        public async Task<ICollection<Dan>> Dani()
        {
            return await mdb.NadjiPoUslovu<Dan>(dani, "korisnikID", funkcije.PrijavljenID());
        }

        public async Task<Dan> Danas() 
        {
            var dani = (await Dani() ).Where(d => d.Datum.Date.Equals(DateTime.Today));
            if (dani == null)
                return null;

            var danas = dani.FirstOrDefault();
            if (danas == null)
                return null;

            return danas;
        }

        public async Task<bool> JeLiDanas(string dID) 
        {
            bool jeste = false;
            var dan = await NadjiDan(dID);
            if(dan.Datum.Date == DateTime.Today)
                jeste = true;

            return jeste;
        }

        public async Task<Dan> NadjiDan(string id)
        {
            return await mdb.NadjiPoIDu<Dan>(dani, id);           
        }

        public async Task<Dan> NadjiDanPoDatumu(int godina, int mesec, int dan)
        {
            var d = await Dani();
            if (d == null)
                    return null;

            return  d.Where(dd => dd.Datum.Day.Equals(new DateTime(godina, mesec, dan).Day)).FirstOrDefault();
        }



        public async Task<string> DodajDan()
        {
            if (await Danas() != null)           
                  return "Već je unet dan sa današnjim datumom.";

            Dan d = new Dan();

            var korisnik = await mdb.NadjiPoUslovu<Korisnik>("Korisnici", "Id", funkcije.PrijavljenID());
            d.korisnik =  korisnik.FirstOrDefault();
            d.korisnikID = d.korisnik.Id;

            var prijava = false ;
            var daniKorisnika = await Dani();
            var juce =  daniKorisnika.OrderBy(d => d.Datum).LastOrDefault();

            if (juce != null)
                prijava = juce.Prijava;

            d.Prijava = prijava;

            var stanja = await mdb.NadjiPoUslovu<Stanje>("Stanja", "korisnikID", funkcije.PrijavljenID());
            var stanje = stanja.OrderBy(s => s.Datum).LastOrDefault();

            if (stanja == null || stanja.ToList().Count == 0 || stanje == null)
                d.Rezultat = 0;
            else
                d.Rezultat = -stanje.EnergetskePotrebe;


            await mdb.Ubaci(dani, d);
              
            if (d.Prijava)
               await DodajIzvestaj();

            return "Dan je dodat.";            
        }


        

        public async Task<Dan> UpisiRezultat()
        {
            var danas = await Danas();
            var stanja = await mdb.NadjiPoUslovu<Stanje>("Stanja", "korisnikID", funkcije.PrijavljenID());
            var stanje = stanja.OrderBy(s => s.Datum).LastOrDefault();

            if (stanje == null)
                return null;

            if (danas.Rezultat == 0)
            {
                danas.Rezultat = -stanje.EnergetskePotrebe;
                await mdb.Zameni(dani, danas.Id, danas );
            }
    
            return danas;
        } 

        public async Task<string> ObrisiDan(string id)
        {
            var dan = await NadjiDan(id);
            if (dan == null)
                return "Izabrani dan ne postoji.";

            await mdb.ObrisiVeze<DanObrok>("DaniObroci", id, "danID");
            await mdb.ObrisiVeze<DanTrening>("DaniTreninzi", id, "danID");  

            var izvestaj = await NadjiIzvestaj(id);
            if (izvestaj != null)
            {
                await mdb.Obrisi<Izvestaj>("Izvestaji", izvestaj.Id);
                await DodajIzvestaj();
            }

            await mdb.Obrisi<Dan>(dani, dan.Id);
            await DodajDan();
            await UpisiRezultat();

            return "Dan je resetovan.";
        }

        public string ProveriDatum(int godina, int mesec, int dan)
        {
            return funkcije.ProveriDatum(godina, mesec, dan);
        }

        public async Task<Dan> Iskljuci() 
        {
            var dan = await Danas();
            dan.Prijava = false;
            await mdb.Zameni(dani, dan.Id, dan);

            var izvestaj = (await mdb.NadjiPoUslovu<Izvestaj>("Izvestaji", "danID", dan.Id)).FirstOrDefault();
            if (izvestaj != null)
                await mdb.Obrisi<Izvestaj>("Izvestaji", izvestaj.Id);
    
            return dan;
        }

        public async Task<Dan> Ukljuci() 
        {
            var dan = await Danas();
            dan.Prijava = true;
            await mdb.Zameni(dani, dan.Id, dan);
  

            var izvestaj = await DodajIzvestaj();
            return dan;
        }

        public async Task<Izvestaj> DodajIzvestaj()
        {
            var danas = await Danas();
            var stanja = await mdb.NadjiPoUslovu<Stanje>("Stanja", "korisnikID", funkcije.PrijavljenID());
            var stanje = stanja.OrderBy(s => s.Datum).LastOrDefault();
            if (stanje == null)
                return null;

            Izvestaj i = await NadjiIzvestaj(danas.Id);
            if (i == null)
            {
                Izvestaj izvestaj = new Izvestaj("Izveštaj za " + funkcije.DatumToString(DateTime.Today) + Environment.NewLine + "Energetske potrebe: " + stanje.EnergetskePotrebe + " kcal " + Environment.NewLine);
                izvestaj.danID = danas.Id;
                izvestaj.dan = danas;

                await mdb.Ubaci("Izvestaji", izvestaj);
                danas.izvestajID = (await mdb.NadjiPoUslovu<Izvestaj>("Izvestaji", "danID", danas.Id)).FirstOrDefault().Id;
                await mdb.Zameni(dani, danas.Id, danas);   

                return izvestaj;
            }
            else
                return i;
        }

        public async Task<ICollection<Izvestaj>> Izvestaji()
        {
            var dani = (await Dani()).OrderByDescending(d => d.Datum).ToList();
            if (dani == null)
                return null;

            ICollection<Izvestaj> izvestaji = new List<Izvestaj>();
            foreach (var d in dani)
                izvestaji.Add((await mdb.NadjiPoUslovu<Izvestaj>("Izvestaji", "danID", d.Id)).FirstOrDefault() );

            if (izvestaji == null || izvestaji.Count == 0)
                return null;

            return izvestaji;
        }

        public async Task<ICollection<string>> PrikazIzvestaja() 
        {
            List<string> prikaz = new List<string>();
            var izvestaji = await Izvestaji();
            if (izvestaji == null || izvestaji.Count == 0)
                return null;

            foreach (var i in izvestaji)
            {
                if (i != null)
                  prikaz.Add(i.Poruka);
            }

            return prikaz;
        }


        public async Task<Izvestaj> NadjiIzvestaj(string danID)
        {
            var dan = await NadjiDan(danID);
            if (dan == null)
                return null;

            var izvestaj = (await mdb.NadjiPoUslovu<Izvestaj>("Izvestaji", "danID", dan.Id)).FirstOrDefault();
            if (izvestaj == null)
                return null; 

            return izvestaj;
        }

        public async Task<Izvestaj> DanasnjiIzvestaj() 
        {
            var danas = await Danas();
            var danasnji = (await mdb.NadjiPoUslovu<Izvestaj>("Izvestaji", "danID", danas.Id)).FirstOrDefault();
            if (danasnji == null)
                return null;

            return (danasnji);
        }


        


    }
}
