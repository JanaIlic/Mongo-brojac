namespace API.Model
{
    public class Dan
    {
        public Dan() { }
        public Dan(DateTime d, double r)
        {
            this.Datum = d;
            this.Rezultat = r;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime Datum { get; set; } = DateTime.Now;
        public double Rezultat { get; set; } = 0;
        public bool Prijava { get; set; } 

        public Korisnik korisnik { get; set; } = default!;
        public string korisnikID { get; set; }

        public Izvestaj? izvestaj { get; set; }
        public string? izvestajID { get; set; }
        public ICollection<DanObrok>? obroci { get; set; } = default!;
        public ICollection<DanTrening>? treninzi { get; set; } = default!;

    }
}
