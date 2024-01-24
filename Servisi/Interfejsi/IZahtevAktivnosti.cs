namespace API.Servisi.Interfejsi
{
    public interface IZahtevAktivnosti
    {
        public Task<ICollection<ZahtevAktivnosti>> PrimljeniZahtevi();
        public Task<ICollection<ZahtevAktivnosti>> NoviPrimljeniZahtevi();
        public Task<ICollection<ZahtevAktivnosti>> ZakljuceniPrimljeniZahtevi();

        public Task<ICollection<ZahtevAktivnosti>> PoslatiZahtevi();
        public Task<ZahtevAktivnosti> NadjiPoslatZahtev(string zahtevID);
        public Task<ZahtevAktivnosti> NadjiPrimljenZahtev(string zahtevID);
        public Task<ICollection<ZahtevAktivnosti>> NadjiPoslateZahteve(string naziv);
        public Task<ICollection<ZahtevAktivnosti>> NadjiPrimljeneZahteve(string naziv);
        public Task<bool> PrijavaPoslednjeg();
        public Task<ZahtevAktivnosti> PosaljiZahtev(string naziv, bool prijava, string napomena);
        public Task<string> PovuciZahtev(string zahtevID);
        public Task<ZahtevAktivnosti> PrihvatiZahtev(ZahtevAktivnosti zahtev);
        public Task<ZahtevAktivnosti> IspuniZahtev(ZahtevAktivnosti zahtev, ICollection<Aktivnost>aktivnosti);
        public Task<ZahtevAktivnosti> OdbijZahtev(ZahtevAktivnosti zahtev);


        public Task<ICollection<RezultatZahteva>> RezultatiZahteva();
        public Task<RezultatZahteva> RezultatZahteva(ZahtevAktivnosti zahtev);
        //   public Task<RezultatZahteva> RezultatPoslednjegZahteva();
        public Task<RezultatZahteva> DodajRezultat(string zahtevID);
    }
}
