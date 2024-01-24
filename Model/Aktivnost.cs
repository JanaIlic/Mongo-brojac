namespace API.Model
{
    public class Aktivnost
    {
        public Aktivnost() { }

        public Aktivnost(string n, double nt) 
        {
            this.Naziv = n;
            this.NivoTezine = nt;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public double NivoTezine { get; set; } 


        public ICollection<KorisnikAktivnost>? korisnici { get; set; } = default!;
        public AdministratorAktivnosti? admin { get; set; } = default!;
        public string? adminID { get; set; }

        public ICollection<TreningAktivnost>? treninzi { get; set; } = default!;
        public virtual ICollection<AktivnostZahtev>? zahtevi  { get; set; } = default!;

    }
}
