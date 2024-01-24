namespace API.Model
{
    public class Izvestaj : Obavestenje
    {
        public Izvestaj()
        {
            this.Tip = TipObavestenja.Izvestaj;
        }
        public Izvestaj(string p) : base(p)
        {
            this.Tip = TipObavestenja.Izvestaj;
        }

        public Dan dan { get; set; } = default!;
        public string danID { get; set; }

    }
}
