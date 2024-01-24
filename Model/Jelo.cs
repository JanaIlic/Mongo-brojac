namespace API.Model
{
    public class Jelo
    {
        public Jelo() { }

        public Jelo(string n, double mas, double ev, string r, double uh, double m, double pr) 
        {
            this.Naziv = n;
            this.Masa = mas;
            this.EnergetskaVrednost = ev;
            this.Recept = r;
            this.UgljeniHidrati = uh;
            this.Mast = m;
            this.Protein = pr;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public double Masa { get; set; } = 0;
        public double EnergetskaVrednost { get; set; } = 0;
        public string Recept { get; set; } = string.Empty;
        public double UgljeniHidrati { get; set; } = 0;
        public double Mast { get; set; } = 0;
        public double Protein { get; set; } = 0;


        public Korisnik korisnik { get; set; } = default!;
        public string korisnikID { get; set; }
        public ICollection<JeloNamirnica>? namirnice  { get; set; } = default!;
        public ICollection<ObrokJelo>? obroci { get; set; } = default!;



    }
}
