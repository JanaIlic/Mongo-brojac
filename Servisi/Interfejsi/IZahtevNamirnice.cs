namespace API.Servisi.Interfejsi
{
    public interface IZahtevNamirnice
    {
        public Task<ICollection<ZahtevNamirnice>> PrimljeniZahtevi();
        public Task<ICollection<ZahtevNamirnice>> NoviPrimljeniZahtevi();

        public Task<ICollection<ZahtevNamirnice>> ZakljuceniPrimljeniZahtevi();
        public Task<ICollection<ZahtevNamirnice>> PoslatiZahtevi();
        public Task<ZahtevNamirnice> NadjiPoslatZahtev(string zahtevID);
        public Task<ZahtevNamirnice> NadjiPrimljenZahtev(string zahtevID);
        public Task<ICollection<ZahtevNamirnice>> NadjiPoslateZahteve(string naziv);
        public Task<ICollection<ZahtevNamirnice>> NadjiPrimljeneZahteve(string naziv);
        public Task<bool> PrijavaPoslednjeg();
        public Task<ZahtevNamirnice> PosaljiZahtev(string naziv, bool prijava, string napomena);
        public Task<string> PovuciZahtev(string zahtevID);
        public Task<ZahtevNamirnice> PrihvatiZahtev(ZahtevNamirnice zahtev);
        public Task<ZahtevNamirnice> IspuniZahtev(ZahtevNamirnice zahtev, ICollection<Namirnica> namirnice);
        public Task<ZahtevNamirnice> OdbijZahtev(ZahtevNamirnice zahtev);

        public Task<ICollection<RezultatZahteva>> RezultatiZahteva();

        public Task<RezultatZahteva> RezultatZahteva(ZahtevNamirnice zahtev);

        // public Task<RezultatZahteva> RezultatPoslednjegZahteva();
        public Task<RezultatZahteva> DodajRezultat(string zahtevID);
    }
}
