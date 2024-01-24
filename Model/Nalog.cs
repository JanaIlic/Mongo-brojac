namespace API.Model
{
    public abstract class Nalog
    {
        public Nalog() { }

        public Nalog(string i, byte[] s, byte[] k) 
        {
            this.Ime = i;
            this.Sifra = s;
            this.Kljuc = k;
        }


        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Ime { get; set; } = string.Empty;
        public byte[] Sifra { get; set; }
        public byte[] Kljuc { get; set; }
        public UlogaNaloga Uloga { get; set; } 

    }

    public enum UlogaNaloga
    {
        Korisnik,
        AdministratorAktivnosti,
        AdministratorNamirnica
    }

}
