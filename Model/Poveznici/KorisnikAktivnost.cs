namespace API.Model
{
    public class KorisnikAktivnost
    {
        public KorisnikAktivnost() { }

        public Korisnik korisnik { get; set; } = default!;
        public string korisnikID { get; set; }

        public Aktivnost aktivnost { get; set; } = default!;
        public string aktivnostID { get; set; }
    }
}
