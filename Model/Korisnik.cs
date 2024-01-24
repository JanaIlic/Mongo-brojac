namespace API.Model
{
    public class Korisnik : Nalog
    {
        public Korisnik(){}

        public Korisnik(string i, byte[] s, byte[] kljuc,  DateTime dr, PolKorisnika pk, string sl) 
        { 
            this.Ime = i;
            this.Sifra = s;
            this.Kljuc = kljuc;
            this.Uloga = UlogaNaloga.Korisnik;
            this.DatumRodjenja = dr;
            this.Pol = pk;
            this.Slika = sl;
        }

        public DateTime DatumRodjenja { get; set; } 
        public PolKorisnika Pol { get; set; }
        public string Slika { get; set; } = string.Empty;
        

        public ICollection<Stanje>? stanja { get; set; } = default!;
        public ICollection<KorisnikNamirnica>? namirnice { get; set; } = default!;
        public ICollection<Jelo>? jela { get; set; } = default!;
        public ICollection<Obrok>? obroci { get; set; } = default!;
        public ICollection<KorisnikAktivnost>? aktivnosti  { get; set; } = default!;
        public ICollection<Trening>? treninzi { get; set; } = default!;
        public ICollection<ZahtevZaPracenje>? poslatiZahteviZaPracenje { get; set; } = default!;
        public ICollection<ZahtevZaPracenje>? dobijeniZahteviZaPracenje { get; set; } = default!;
        public ICollection<ZahtevAktivnosti>? zahteviAktivnosti { get; set; } = default!;
        public ICollection<ZahtevNamirnice>? zahteviNamirnica { get; set; } = default!;
        public ICollection<Dan>? dani { get; set; } = default!;

        public ICollection<DvaKorisnika>? prati { get; set; } = default!;
        public ICollection<DvaKorisnika>? pratioci { get; set; } = default!;
        public ICollection<Poruka>? poslatePoruke { get; set; } = default!;
        public ICollection<Poruka>? primljenePoruke { get; set; } = default!;
        public ICollection<Objava>? objave { get; set; } = default!;
        public ICollection<Ocena>? ocene { get; set; } = default!;
    }

    public enum PolKorisnika 
    {
        Muski, 
        Zenski
    }


}
