namespace API.Model
{
    public abstract class Obavestenje
    {
        public Obavestenje() { }
        public Obavestenje(string p)
        {
            this.Poruka = p;
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public TipObavestenja Tip { get; set; }
        public string Poruka { get; set; } = string.Empty;
    }

    public enum TipObavestenja
    {
        Izvestaj,
        RezultatZahteva
    }

}
