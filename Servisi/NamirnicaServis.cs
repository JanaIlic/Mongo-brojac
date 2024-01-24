namespace API.Servisi
{
    public class NamirnicaServis : INamirnica
    {
        private IMongoDB mdb;
        public const string namirnice = "Namirnice";
        public NamirnicaServis(IMongoDB servis )
        {
            mdb = servis;
        }

        public async Task<Namirnica> DodajNamirnicu(Namirnica namirnica)
        {
            namirnica.admin = (await mdb.UcitajListu<AdministratorNamirnica>("AdminiNamirnica")).FirstOrDefault();
            namirnica.adminID = namirnica.admin.Id;
            await mdb.Ubaci<Namirnica>(namirnice, namirnica);

            return namirnica;
        }

        public async Task<ICollection<Namirnica>> NadjiNamirnicePoNazivu(string naziv)
        {
            return (await mdb.UcitajListu<Namirnica>(namirnice)).
                Where(n => ((n.Naziv.Contains(naziv)) || (naziv.Contains(n.Naziv)))).ToList();
        }

        public async Task<Namirnica> NadjiNamirnicuPoNazivu(string naziv)
        {
            return (await mdb.NadjiPoUslovu<Namirnica>(namirnice, "Naziv", naziv)).FirstOrDefault();
        }

        public async Task<Namirnica> NadjiNamirnicu(string id)
        {
            return await mdb.NadjiPoIDu<Namirnica>(namirnice, id);
        }

        public async Task<ICollection<Namirnica>> Namirnice()
        {
            return await mdb.UcitajListu<Namirnica>(namirnice);
        }

        public async Task<string> ObrisiNamirnicu(string id)
        {
            var namirnica = await NadjiNamirnicu(id);
            if (namirnica == null)
                return "Ne postoji tražena namirnica.";

            await mdb.ObrisiVeze<JeloNamirnica>("JelaNamirnice", id, "namirnicaID");
            await mdb.Obrisi<Namirnica>(namirnice, id);

            return namirnica.Naziv + " je obrisana.";
        }

        public async Task<Namirnica> PromeniKoeficijentPromeneMase(Namirnica namirnica, double koeficijent)
        {
            namirnica.PromenaMase = koeficijent;
            return await mdb.Zameni<Namirnica>(namirnice, namirnica.Id, namirnica);
        }

        public async Task<Namirnica> PromeniEnergetskuVrednost(Namirnica namirnica, double energetskaVrednost)
        {
            namirnica.EnergetskaVrednost = energetskaVrednost;
            return await mdb.Zameni<Namirnica>(namirnice, namirnica.Id, namirnica);
        }

        public async Task<Namirnica> PromeniKolicinuBrasna(Namirnica namirnica, KolicinaBrasna brasno)
        {
            namirnica.DodatoBrasno = brasno;
            return await mdb.Zameni<Namirnica>(namirnice, namirnica.Id, namirnica);
        }

        public async Task<Namirnica> PromeniKolicinuMasti(Namirnica namirnica, KolicinaMasti dodataMast)
        {
            namirnica.DodataMast = dodataMast;
            return await mdb.Zameni<Namirnica>(namirnice, namirnica.Id, namirnica);
        }

        public async Task<Namirnica> PromeniMast(Namirnica namirnica, double m)
        {
            namirnica.Mast = m;
            return await mdb.Zameni<Namirnica>(namirnice, namirnica.Id, namirnica);
        }

        public async Task<Namirnica> PromeniNaziv(Namirnica namirnica, string naziv)
        {
            namirnica.Naziv = naziv;
            return await mdb.Zameni<Namirnica>(namirnice, namirnica.Id, namirnica);
        }

        public async Task<Namirnica> PromeniProtein(Namirnica namirnica, double protein)
        {
            namirnica.Protein = protein;
            return await mdb.Zameni<Namirnica>(namirnice, namirnica.Id, namirnica);
        }

        public async Task<Namirnica> PromeniTipObrade(Namirnica namirnica, TipObrade tip)
        {
            namirnica.Tip = tip;
            return await mdb.Zameni<Namirnica>(namirnice, namirnica.Id, namirnica);
        }

        public async Task<Namirnica> PromeniUgljeneHidrate(Namirnica namirnica, double uh)
        {
            namirnica.UgljeniHidrati = uh;
            return await mdb.Zameni<Namirnica>(namirnice, namirnica.Id, namirnica);
        }

        public async Task<Namirnica> PromeniVrstu(Namirnica namirnica, VrstaNamirnice vrsta)
        {
            namirnica.Vrsta = vrsta;
            return await mdb.Zameni<Namirnica>(namirnice, namirnica.Id, namirnica);
        }

        public async Task<Namirnica> PromeniOpis(Namirnica namirnica, string opis)
        {
            namirnica.Opis = opis;
            return await mdb.Zameni<Namirnica>(namirnice, namirnica.Id, namirnica);
        }

        public async Task<Namirnica> SkalirajNamirnicu(Namirnica namirnica, double m) 
        {
            Namirnica n = namirnica;
            double koef = m / 100;

            n.EnergetskaVrednost = Math.Ceiling(namirnica.EnergetskaVrednost * koef);
            n.Protein = Math.Ceiling(namirnica.Protein * koef);
            n.UgljeniHidrati = Math.Ceiling(namirnica.UgljeniHidrati * koef);
            n.Mast = Math.Ceiling(namirnica.Mast * koef);
            return n;
        }

        public async Task<ICollection<Namirnica>> Filtriraj(int vrsta, int tip, int mast, int brasno)
        {
            ICollection<Namirnica> filter = await mdb.UcitajListu<Namirnica>(namirnice);

            if (vrsta != 9)
                filter = filter.Where(n => n.Vrsta == (VrstaNamirnice)vrsta).ToList();

            if (tip != 6)
                filter = filter.Where(n => n.Tip == (TipObrade)tip).ToList();

            if (mast != 3)
                filter = filter.Where(n => n.DodataMast == (KolicinaMasti)mast).ToList();

            if (brasno != 3)
                filter = filter.Where(n => n.DodatoBrasno == (KolicinaBrasna)brasno).ToList();

            return filter;
        }


    }
}
