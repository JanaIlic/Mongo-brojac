namespace API.Servisi.Interfejsi
{
    public interface IPoruka
    {
        public Task<bool> Pratilac(string pratilacID);
        public Task<bool> Pracen(string pracenID);
        public Task<Poruka> NadjiPoslatuPoruku(string porukaID);
        public Task<ICollection<Poruka>> PoslatePoruke();
        public Task<ICollection<Poruka>> PrimljenePoruke();
        public  Task<ICollection<Poruka>> Razgovor(Korisnik korisnik);
        public Task<ICollection<Korisnik>> Sagovornici();
        public Task<ICollection<bool>> AutoriPoruka(Korisnik korisnik);
        public Task<Poruka> PosaljiPoruku(Korisnik korisnik, string tekst);
        public Task<Poruka> PrepraviPoruku(string porukaID, string tekst);
        public Task<string> ObrisiPoruku(string porukaID);
        public Task<string> ObrisiRazgovor(Korisnik korisnik);
        public Task<string> ObrisiRazgovore();
        public Task<string> ObrisiPoslatePoruke();

    }
}
