namespace API.Model
{
    public class ZahtevAktivnosti : Zahtev
    {
        public ZahtevAktivnosti() {}

        public ZahtevAktivnosti(string n)
        {
            this.Tip = TipZahteva.Aktivnost;
            this.NazivAktivnosti = n;
        }

        public string NazivAktivnosti { get; set; } = string.Empty;

        public AdministratorAktivnosti admin { get; set; } = default!;
        public string adminID { get; set; } = "1";

        public virtual ICollection<AktivnostZahtev> aktivnosti { get; set;} = default!;


    }
}
