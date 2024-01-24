namespace API.Model
{
    public class DanObrok
    {
        public DanObrok() { }
        
        public Dan dan { get; set; } = default!;
        public string danID { get; set; }
        public Obrok obrok { get; set; } = default!;
        public string obrokID { get; set; }
    }
}
