namespace API.Model
{
    public class ZahtevNamirnice : Zahtev
    {
        public ZahtevNamirnice() { }

        public ZahtevNamirnice(string n)
        {
            this.Tip = TipZahteva.Namirnica;
            this.NazivNamirnice = n;
        }

        public string NazivNamirnice { get; set; } = string.Empty;
        public AdministratorNamirnica admin { get; set; } = default!;
        public string adminID { get; set; } = "2";

        public ICollection<NamirnicaZahtev> namirnice { get; set; } = default!;

    }
}
