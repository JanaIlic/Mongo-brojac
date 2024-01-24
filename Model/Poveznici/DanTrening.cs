namespace API.Model
{
    public class DanTrening
    {
        public DanTrening() { }

        public Dan dan { get; set; } = default!;
        public string danID { get; set; }
        public Trening trening { get; set; } = default!;
        public string treningID { get; set; }
    }
}
