namespace API.Model
{
    public class TreningAktivnost
    {
        public TreningAktivnost() { }

        public Trening trening { get; set; } = default!;
        public string treningID { get; set; }
        public Aktivnost aktivnost { get; set; } = default!;
        public string aktivnostID { get; set; }

        public int vreme { get; set; }

    }
}
