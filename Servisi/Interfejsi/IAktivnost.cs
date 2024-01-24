namespace API.Servisi
{
    public interface IAktivnost
    {
        public Task<ICollection<Aktivnost>> Aktivnosti();
        public Task<Aktivnost> AktivnostPoIDu(string id);
        public Task<ICollection<Aktivnost>> AktivnostiPoNazivu(string naziv);
        public Task<string> Potrosnja(Aktivnost aktivnost, int minuti);
        public Task<Aktivnost> DodajAktivnost(string naziv, double nt);
        public Task<Aktivnost> PromeniNazivAktivnosti(Aktivnost aktivnost, string naziv);
        public Task<Aktivnost> PromeniTezinuAktivnosti(Aktivnost aktivnost, double nt);
        public Task<string> ObrisiAktivnost(string id);



    }
}
