namespace API.Servisi.Interfejsi
{
    public interface IObrok
    {
        public Task<ICollection<Obrok>> Obroci();
        public Task<Obrok> ObrokPoIDu(string id);
        public Task<Obrok> ObrokPoNazivu(string naziv);
        public Task<ICollection<Obrok>> ObrociPoNazivu(string naziv);
        public Task<ICollection<Obrok>> ObrociDana(Dan d);
        public Task<bool> ObrokVecDodatDanas(Obrok obrok);

        public Task<ICollection<Obrok>> DanasnjiObroci();
        public Task<string> OpisiObrok(Obrok obrok);

        public Task<Jelo> JeloObroka(Obrok o, Jelo j);
        public Task<ICollection<Jelo>> JelaObroka(Obrok o);
        public Task<ICollection<double>> MaseJela(Obrok o);
        public Task<ICollection<double>> EvJela(Obrok o);
        public Task<Obrok> DodajObrok(string naziv);
        public Task<Objava> ObjaviObrok(Obrok obrok);
        public Task<Obrok> DodajObrokDanas(Obrok obrok);
        public Task<Obrok> PromeniNaziv(Obrok obrok, string naziv);
        public Task<Obrok> PromeniMasuObroka(Obrok obrok, double masa);
        public Task<Obrok> DodajJeloObroku(Obrok obrok, Jelo jelo, double masa);
        public Task<Obrok> PromeniMasuJela(Obrok obrok, Jelo jelo, double masa);
        public Task<Obrok> ObrisiJeloIzObroka(Obrok obrok, Jelo jelo);
        public Task<string> ObrisiObrok(string id);
        public Task<string> ObrisiObrokDanas(string id);


    }
}
