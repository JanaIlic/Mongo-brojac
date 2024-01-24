namespace API.Servisi.Interfejsi
{
    public interface IObjava
    {
        public Task<ICollection<Objava>> Objave();
        public Task<ICollection<Objava>> VidljiveObjave();
        public Task<Objava> ObjavaPoIDu(string objavaID);
        public Task<Objava> ObjavaPracenog(string objavaID);
        public Task<ICollection<Objava>> SveObjavePracenog(string korisnikID);
        public Task<ICollection<Objava>> Komentari(Objava objava);
        public Task<Objava> KomentarPratiocaNaObjavu(string objavaID);
        public Task<Objava> KomentarPrijavljenog(string objavaID);
        public Task<ICollection<Objava>> ObjavePoTekstu(string tekst);
        public Task<Ocena> OcenaNaObjavu(Objava objava);
        public Task<ICollection<Ocena>> Ocene(Objava objava);
        public Task<double> Prosek(Objava objava);
        public Task<bool> ObjavaPrijavljenogKorisnika(Objava objava);
        public Task<ICollection<Korisnik>> AutoriObjava();
        public Task<ICollection<Korisnik>> AutoriOcena(Objava objava);
        public Task<ICollection<Korisnik>> AutoriKomentara(Objava objava);
        public Task<Objava> Objavi(string tekst);
        public Task<Objava> ObjaviSaSlikom(string tekst, string slika);
        public Task<Objava> Komentarisi(Objava glavna, string tekst);
        public Task<Objava> Oceni(Objava objava, int vrednost);
        public Task<Objava> PromeniOcenu(Objava objava, int vrednost);
        public Task<Objava> PrepraviObjavu(Objava objava, string tekst);
        public Task<Objava> PovuciOcenu(Objava objava);
        public Task<string> ObrisiObjavu(string objavaID);
        public Task<ICollection<Objava>> ObrisiKomentar(string komentarID);
        public Task<string> ObrisiObjave();

    }
}
