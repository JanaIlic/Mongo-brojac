namespace API.Model.Poveznici
{
    public class AktivnostZahtev
    {
        public AktivnostZahtev(){ }

        public ZahtevAktivnosti zahtev { get; set; } = default!;
        public string zahtevID { get; set; }
        
        public Aktivnost aktivnost { get; set; } = default!;
        public string aktivnostID { get; set; }

    }
}
