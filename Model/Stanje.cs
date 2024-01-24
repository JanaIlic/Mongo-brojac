namespace API.Model
{
    public class Stanje
    {
        public Stanje() { }

        public Stanje(double v, double t)
        {
            Datum = DateTime.Now;
            Visina = Math.Round(v, 2);
            Tezina = Math.Round(t, 2);
        }


        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime Datum { get; set; }
        public double Visina { get; set; }
        public BMI Bmi { get; set; }
        public double Tezina { get; set; }
        public double BMR { get; set; }
        public double EnergetskePotrebe { get; set; }
        public double Protein { get; set; }
        public double UgljeniHidrati { get; set; }
        public double Mast { get; set; }

        public double CiljnaKilaza { get; set; }
        public double CiljniUnos { get; set; }


        public Korisnik korisnik { get; set; } = default!;
        public string korisnikID { get; set; }


    }

    public enum BMI
    {
        Neuhranjenost,
        NormalnaUhranjenost,
        Predgojaznost,
        Gojaznost,
        PrekomernaGojaznost
    }


}
