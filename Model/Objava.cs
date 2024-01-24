namespace API.Model
{
    public class Objava
    {
        public Objava() { }
        public Objava(string txt, string s)
        {
            this.Tekst = txt;
            this.Slika = s;
            this.Vreme = DateTime.Now;
        }


        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Tekst { get; set; } = string.Empty;
        public string Slika  { get; set; } = string.Empty;
        public DateTime Vreme { get; set; }

        public Korisnik autor { get; set; } = default!;
        public string autorID { get; set; }

        public Objava? glavna { get; set; }
        public string? glavnaID { get; set; }
        public ICollection<Objava>? komentari { get; set; } = default!;
        public ICollection<Ocena>? ocene { get; set; } = default!;

    }
}
