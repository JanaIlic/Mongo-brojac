namespace API.Model
{
    public class Namirnica
    {
        public Namirnica() { }

        public Namirnica(string naz, VrstaNamirnice v, TipObrade t, KolicinaBrasna b,
            KolicinaMasti dm, double ev, double p, double uh, double m, double pm, string opis )
        {
            this.Naziv = naz;
            this.EnergetskaVrednost = ev;
            this.Protein = p;
            this.UgljeniHidrati = uh;
            this.Mast = m;
            this.Vrsta = v;
            this.PromenaMase = pm;
            this.Tip = t;
            this.DodatoBrasno = b;
            this.DodataMast = dm;
            this.Opis = opis;
        }


        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public double EnergetskaVrednost { get; set; }
        public double Protein { get; set; }
        public double UgljeniHidrati { get; set; }
        public double Mast { get; set; }
        public VrstaNamirnice Vrsta { get; set; }
        public TipObrade Tip { get; set; } = TipObrade.Sveza;

        public KolicinaMasti DodataMast { get; set; } = KolicinaMasti.Bez;
        public KolicinaBrasna DodatoBrasno { get; set; } = KolicinaBrasna.Bez;
        public double PromenaMase { get; set; } = 1;

        public string Opis { get; set; } = string.Empty;


        public ICollection<KorisnikNamirnica>? korisnici { get; set; } = default!;
        public AdministratorNamirnica? admin { get; set; } = default!;
        public string? adminID { get; set; } 
        public ICollection<JeloNamirnica>? jela { get; set; } = default!;
        public ICollection<NamirnicaZahtev>? zahtevi { get; set; } = default!;

    }

    public enum VrstaNamirnice
    {
        Voce,
        Povrce,
        Meso,
	    MlecniProizvodi,
        Zitarica,
        Testo,
	    SlanoMesanoJelo,
        Poslastica,
        Ostalo
    }

    public enum TipObrade
    { 
        Sveza,
        Kuvana,
        Dinstana,
        Przena,
        Pecena,
        Ostalo
    }

    public enum KolicinaMasti 
    {
        Bez,
        Plitko,
        Duboko
    }

    public enum KolicinaBrasna
    {
        Bez, 
        Sa,
        Pohovano
    }

}
