namespace API.Model
{
    public class ZahtevZaPracenje : Zahtev
    {
        public ZahtevZaPracenje(){ }

        public ZahtevZaPracenje(string p) : base(p)
        {
            this.Tip = TipZahteva.Pracenje;
        }

        public Korisnik pracen { get; set; } = default!;
        public string pracenID { get; set; }

    }
}
