namespace API.Servisi.Interfejsi
{
    public interface ITrening
    {
        public Task<ICollection<Trening>> Treninzi();
        public Task<Trening> TreningPoIDu(string id);
        public Task<Trening> TreningPoNazivu(string naziv);
        public Task<ICollection<Trening>> TreninziPoNazivu(string naziv);
        public Task<double> VremeAktivnosti(Trening trening, Aktivnost aktivnost);
        public Task<ICollection<Aktivnost>> AktivnostiTreninga(Trening trening);
        public Task<ICollection<double>> VremenaAktivnosti(Trening trening);
        public Task<ICollection<double>> PotrosnjePriAktivnostima(Trening trening);
        public Task<bool> TreningVecDodatDanas(Trening trening);
        public Task<ICollection<Trening>> DanasnjiTreninzi();
        public Task<ICollection<Trening>> TreninziDana(Dan d);
        public Task<Trening> DodajTrening(string naziv);
        public Task<Objava> ObjaviTrening(Trening trening);
        public Task<Trening> PromeniNaziv(Trening trening, string naziv);
        public Task<Trening> DodajDnevniTrening(Trening trening);
        public Task<Trening> DodajAktivnostTreningu(Trening trening, Aktivnost aktivnost, int vreme);
        public Task<Trening> PromeniVremeAktivnosti(Trening trening, Aktivnost aktivnost, int vreme);
        public Task<string> OpisiTrening(Trening trening);
        public Task<Trening> ObrisiAktivnostIzTreninga(Trening trening, Aktivnost aktivnost);
        public Task<string> ObrisiTrening(string id);
        public Task<string> ObrisiDanasnjiTrening(string id);

    }
}
