namespace API.Model
{
    public class Ocena
    {
        public Ocena() { }

        public Ocena(int v, DateTime dt) 
        {
            this.Vrednost = v;
            this.Vreme = dt;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int Vrednost { get; set; } = 0;
        public DateTime Vreme { get; set; }
        public Objava objava { get; set; } = default!;
        public string objavaID { get; set; }

        public Korisnik korisnik { get; set; } = default!;
        public string korisnikID { get; set; }


    }
}
