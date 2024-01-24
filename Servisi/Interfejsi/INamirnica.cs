namespace API.Servisi.Interfejsi
{
    public interface INamirnica
    {

        public Task<ICollection<Namirnica>> Namirnice();
        public Task<ICollection<Namirnica>> NadjiNamirnicePoNazivu(string naziv);
        public Task<ICollection<Namirnica>> Filtriraj(int vrsta, int tip, int mast, int brasno);
        public Task<Namirnica> NadjiNamirnicuPoNazivu(string naziv);
        public Task<Namirnica> NadjiNamirnicu(string id);
        public Task<Namirnica> DodajNamirnicu(Namirnica namirnica);
        public Task<Namirnica> PromeniNaziv(Namirnica namirnica, string naziv);
        public Task<Namirnica> PromeniVrstu(Namirnica namirnica, VrstaNamirnice vrsta);
        public Task<Namirnica> PromeniTipObrade(Namirnica namirnica, TipObrade tip);
        public Task<Namirnica> PromeniKolicinuBrasna(Namirnica namirnica, KolicinaBrasna brasno);
        public Task<Namirnica> PromeniKolicinuMasti(Namirnica namirnica, KolicinaMasti dodataMast);
        public Task<Namirnica> PromeniEnergetskuVrednost(Namirnica namirnica, double energetskaVrednost);
        public Task<Namirnica> PromeniProtein(Namirnica namirnica, double protein);
        public Task<Namirnica> PromeniUgljeneHidrate(Namirnica namirnica, double uh);
        public Task<Namirnica> PromeniMast(Namirnica namirnica, double m);
        public Task<Namirnica> PromeniKoeficijentPromeneMase(Namirnica namirnica, double koeficijent);
        public Task<Namirnica> PromeniOpis(Namirnica namirnica, string opis);

        public Task<Namirnica> SkalirajNamirnicu(Namirnica namirnica, double m);
        public Task<string> ObrisiNamirnicu(string id);



    }
}
