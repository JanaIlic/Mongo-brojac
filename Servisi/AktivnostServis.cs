namespace API.Servisi
{
    public class AktivnostServis : IAktivnost
    {
        private IMongoDB mdb;
        public PomocneFunkcije funkcije;
        public const string aktivnosti = "Aktivnosti";
        public AktivnostServis(IMongoDB servis, IHttpContextAccessor hca)
        {
            mdb = servis;
            funkcije = new PomocneFunkcije(hca);
        }

        public async Task<ICollection<Aktivnost>> Aktivnosti()
        {
            return await mdb.UcitajListu<Aktivnost>(aktivnosti);
        }

        public async Task<Aktivnost> AktivnostPoIDu(string id)
        {
            return await mdb.NadjiPoIDu<Aktivnost>(aktivnosti, id);
        }

        public async Task<ICollection<Aktivnost>> AktivnostiPoNazivu(string naziv)
        {
            return await mdb.NadjiSlicne<Aktivnost>(aktivnosti, "Naziv", naziv);
        }

        public async Task<string> Potrosnja(Aktivnost aktivnost, int minuti) 
        {
            var stanja = await mdb.NadjiPoUslovu<Stanje>("Stanja", "korisnikID", funkcije.PrijavljenID());
            var stanje = stanja.OrderBy(s => s.Datum).LastOrDefault();   
            var mirovanje = stanje.BMR  * minuti / 1440 ;
            double proizvod = Math.Floor(mirovanje * aktivnost.NivoTezine);
       
            return "Za " + minuti + " minuta " + aktivnost.Naziv + " potrošićeš " + proizvod + " kcal, što je " + 
                Math.Floor(proizvod - mirovanje) + " više nego što bi za isto vreme u stanju mirovanja." ;
        }

        public async Task<Aktivnost> DodajAktivnost(string naziv, double nt)
        {
            Aktivnost aktivnost = new Aktivnost(naziv, nt);
            var a = await mdb.UcitajListu<AdministratorAktivnosti>("AdminiAktivnosti");
 
            aktivnost.admin =  a.FirstOrDefault();
            aktivnost.adminID = aktivnost.admin.Id;

            await mdb.Ubaci<Aktivnost>(aktivnosti, aktivnost);

            return aktivnost;
        }

        public async Task<Aktivnost> PromeniNazivAktivnosti(Aktivnost aktivnost, string naziv)
        {
            aktivnost.Naziv = naziv;
            return await mdb.Zameni(aktivnosti, aktivnost.Id, aktivnost);
        }

        public async Task<Aktivnost> PromeniTezinuAktivnosti(Aktivnost aktivnost, double nt)
        {
            aktivnost.NivoTezine = nt;
            return await mdb.Zameni(aktivnosti, aktivnost.Id, aktivnost);
        }

        public async Task<string> ObrisiAktivnost(string id)
        {
            var aktivnost = await AktivnostPoIDu(id);
            if (aktivnost == null)
                return "Ne postoji tražena aktivnost.";

            await mdb.ObrisiVeze<TreningAktivnost>("TreniziAktivnosti", aktivnost.Id, "aktivnostID");
            await mdb.Obrisi<Aktivnost>(aktivnosti, id);
      
            return "Aktivnost " + aktivnost.Naziv + " je obrisana.";
        }







    }
}
