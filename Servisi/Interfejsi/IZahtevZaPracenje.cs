namespace API.Servisi.Interfejsi
{
    public interface IZahtevZaPracenje
    {
        public Task<ICollection<ZahtevZaPracenje>> PrimljeniZahtevi();
        public Task<ICollection<ZahtevZaPracenje>> PoslatiZahtevi();
        public Task<ZahtevZaPracenje> NadjiPoslatZahtev(string zahtevID);
        public Task<ZahtevZaPracenje> NadjiPrimljenZahtev(string zahtevID);
        public Task<ICollection<ZahtevZaPracenje>> NadjiPoslateZahteve(string ime);
        public Task<ICollection<ZahtevZaPracenje>> NadjiPrimljeneZahteve(string ime);
        public Task<ICollection<string>> Primaoci();
        public Task<ICollection<string>> Podnosioci();
        public Task<bool> PrijavaPoslednjeg();
        public Task<bool> ZahtevPoslatKorisniku(Korisnik primalac);
        public Task<ZahtevZaPracenje> PosaljiZahtev(Korisnik korisnik, bool prijava, string pozdrav);
        public Task<string> PovuciZahtev(string zahtevID);
        public Task<string> Otrprati(string pracenID);
        public Task<bool> Pratilac(string pratilacID);
        public Task<bool> Pracen(string pracenID);
        public Task<string> ObrisiPratioca(string pratilacID);
        public Task<ZahtevZaPracenje> PrihvatiZahtev(ZahtevZaPracenje zahtev);
        public Task<ZahtevZaPracenje> OdbijZahtev(ZahtevZaPracenje zahtev);
        public Task<ICollection<RezultatZahteva>> RezultatiZahteva();
        public Task<RezultatZahteva> RezultatPoslatogZahteva(ZahtevZaPracenje zahtev);
      //  public Task<RezultatZahteva> RezultatPrimljenogZahteva(ZahtevZaPracenje zahtev);
        public Task<RezultatZahteva> DodajRezultat(string zahtevID);

    }
}
