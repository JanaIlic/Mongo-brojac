namespace API.Servisi.Interfejsi
{
    public interface IStanje
    {
        public Task<ICollection<Stanje>> Stanja();
        public Task<Stanje> StanjePoIDu(string id);
        public Task<Stanje> StanjePoDatumu(DateTime datum);
        public Task<Stanje> PrvoStanje();
        public Task<ICollection<int>> MeseciPrveGodine();
        public int PonudiBrDana(int mesec);
        public Task<Stanje> AktuelnoStanje();
        public Task<string> PrikazBmi(Stanje stanje);
        public Task<Stanje> UpisiStanje(double visina, double tezina, double nt);
        public Task<Stanje> PromeniVisinu(double visina);
        public Task<Stanje> PromeniTezinu(double tezina);
        public Task<string> ZadajCiljnuTezinu(double cilj);
        public Task<string> ZadajVreme(int brojDana);
        public Task<string> ObrisiStanje();
        public string ProveriDatum(int godina, int mesec, int dan);
        public Task<List<string>> PonudiPeriode();

        public int ParsirajPeriod(string period);

        public Task<double> IzracunajBMR(Korisnik korisnik, double visina, double tezina);

    }
}
