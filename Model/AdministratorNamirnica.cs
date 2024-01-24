namespace API.Model
{
    public class AdministratorNamirnica : Nalog
    {
        public AdministratorNamirnica(){}

        public AdministratorNamirnica(string i, byte[] s, byte[] k) : base(i, s, k)
        {
            this.Uloga = UlogaNaloga.AdministratorNamirnica;
        }


        public ICollection<Namirnica>? namirnice { get; set; } = default!;
        public ICollection<ZahtevNamirnice>? zahtevi { get; set; } = default!;
    }
}
