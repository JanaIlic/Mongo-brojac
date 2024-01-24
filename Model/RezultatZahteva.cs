namespace API.Model
{
    public class RezultatZahteva : Obavestenje
    {
        public RezultatZahteva()
        {
            this.Tip = TipObavestenja.RezultatZahteva;
        }
        public RezultatZahteva(string p) : base(p)
        {
            this.Tip = TipObavestenja.RezultatZahteva;
        }

        public ZahtevAktivnosti? zAktivnosti { get; set; } = default!;
        public ZahtevNamirnice? zNamirnice { get; set; } = default!;
        public ZahtevZaPracenje? zPracenja { get; set; } = default!;
        public string podnosilacID { get; set; }
        public string zahtevID { get; set; }

    }
}
