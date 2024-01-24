namespace API.Servisi.Interfejsi
{
    public interface IJelo
    {
        public Task<ICollection<Jelo>> Jela();
        public Task<Jelo> JeloPoNazivu(string naziv);
        public Task<ICollection<Jelo>> JelaPoNazivu(string naziv);
        public Task<Jelo> JeloPoIDu(string id);
        public Task<ICollection<Namirnica>> NamirniceJela(Jelo jelo);

        public Task<Jelo> SkalirajJelo(Jelo j, double masa);

        public Task<double> MasaNamirniceUJelu(Jelo jelo, Namirnica namirnica);
        public Task<ICollection<double>> MaseNamirnicaUJelu(Jelo jelo);
        public Task<Jelo> DodajNovoJelo(string naziv);
        public Task<Objava> ObjaviJelo(Jelo jelo);
        public Task<Jelo> PromeniNazivJela(Jelo jelo, string noviNaziv);
        public Task<Jelo> DodajJeluNamirnicu(Jelo jelo, Namirnica namirnica, double masa, bool pre);
        public Task<Jelo> PromeniMasuNamirnice(Jelo jelo, Namirnica namirnica, double masa);
        public Task<Jelo> NapisiRecept(Jelo jelo);
        public Task<Jelo> ObrisiNamirnicuIzJela(Jelo jelo, Namirnica namirnica);
        public Task<string> ObrisiJelo(string id);

    }
}
