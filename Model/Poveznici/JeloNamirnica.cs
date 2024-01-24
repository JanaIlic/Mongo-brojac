namespace API.Model
{
    public class JeloNamirnica
    {
        public JeloNamirnica() { }

        public Jelo jelo { get; set; } = default!;
        public string jeloID { get; set; } 

        public Namirnica namirnica { get; set; } = default!;
        public string namirnicaID { get; set; }

        public double masa { get; set; }

    }
}
