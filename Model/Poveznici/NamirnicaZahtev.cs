namespace API.Model.Poveznici
{
    public class NamirnicaZahtev
    {
        public NamirnicaZahtev(){ }

        public ZahtevNamirnice zahtev { get; set; } = default!;
        public string zahtevID { get; set; }
        public Namirnica namirnica { get; set; } = default!;
        public string namirnicaID { get; set; }
    }
}
