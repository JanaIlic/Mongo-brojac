namespace API.Model
{
    public class AdministratorAktivnosti : Nalog
    {
        public AdministratorAktivnosti(){}

        public AdministratorAktivnosti(string i, byte[] s, byte[] k) : base(i, s, k)
        {
            this.Uloga = UlogaNaloga.AdministratorAktivnosti;
        }


        public ICollection<Aktivnost>? aktivnosti { get; set; } = default!;
        public ICollection<ZahtevAktivnosti>? zahtevi { get; set; } = default!;

       

    }
}
