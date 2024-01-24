namespace API.Model
{
    public class ObrokJelo
    {
        public ObrokJelo() { }

        public Obrok obrok { get; set; } = default!;
        public string obrokID { get; set; }

        public Jelo jelo { get; set; } = default!;
        public string jeloID { get; set; }

        public double masa { get; set; }

    }
}
