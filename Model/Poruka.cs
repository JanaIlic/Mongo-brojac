namespace API.Model
{
    public class Poruka
    {
        public Poruka() { }
        public Poruka(string txt, DateTime dt)
        {
            this.Tekst = txt;
            this.Vreme = dt;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Tekst { get; set; } = string.Empty;
        public DateTime Vreme { get; set; } 

        public Korisnik autor { get; set; } = default!;
        public string autorID { get; set; }

        public Korisnik primalac { get; set; } = default!;
        public string primalacID { get; set; }



    }
}
