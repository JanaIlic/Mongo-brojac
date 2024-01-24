namespace API.Model
{
    public class DvaKorisnika
    {
        public DvaKorisnika() { }

        public Korisnik? pratilac { get; set; } = default!;
        public string? pratilacID { get; set; }

        public Korisnik? pracen { get; set; } = default!;
        public string? pracenID { get; set; }

       // public ICollection<Poruka>? primljenePoruke { get; set; } = default!;

    }
}
