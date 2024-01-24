namespace API.Model
{
    public class Obrok
    {
        public Obrok() { }
        public Obrok(string n, double ev, double mas, double uh, double m, double pr) 
        {
            this.Naziv = n;
            this.EnergetskaVrednost = ev;
            this.Masa = mas;
            this.UgljeniHidrati = uh;
            this.Protein = pr;
            this.Mast = m;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Naziv { get; set; } = String.Empty;
        public double EnergetskaVrednost { get; set; } = 0;
        public double Masa { get; set; } = 0;
        public double UgljeniHidrati { get; set; } = 0;
        public double Mast { get; set; } = 0;
        public double Protein { get; set; } = 0;


        public ICollection<ObrokJelo>? jela { get; set; } = default!;
        public Korisnik korisnik  { get; set; } = default!;
        public string korisnikID { get; set; }
        public ICollection<DanObrok>? dani { get; set; } = default!;

    }
}
