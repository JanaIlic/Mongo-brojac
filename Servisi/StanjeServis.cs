namespace API.Servisi
{
    public class StanjeServis : IStanje
    {
        private IMongoDB mdb;
        public PomocneFunkcije funkcije;
        public const string stanja = "Stanja";
        public StanjeServis(IMongoDB servis, IHttpContextAccessor hca) 
        { 
            mdb = servis;
            funkcije = new PomocneFunkcije(hca);
        }

        public async Task<ICollection<Stanje>> Stanja()
        {
            var s = await mdb.NadjiPoUslovu<Stanje>(stanja, "korisnikID", funkcije.PrijavljenID());
            return  s.OrderBy(st => st.Datum).ToList();
        }

        public async Task<Stanje> StanjePoIDu(string id)
        {
            //var stanje = (await mdb.NadjiPoUslovuIzListe<Stanje>((await Stanja()), "Id", id)).FirstOrDefault();
            var stanje = await mdb.NadjiPoUslovima<Stanje>(stanja, new List<string> { "korisnikID", "Id" },
                new List<string> { funkcije.PrijavljenID(), id });
            if (stanje == null)
                return null;

            return  stanje.FirstOrDefault();
        }

        public async Task<Stanje> PrvoStanje()
        {
            var st = await Stanja();
                //context.Stanja.Where(s => s.korisnikID == funkcije.PrijavljenID()).OrderBy(s => s.Datum).FirstOrDefaultAsync();
            if (st == null)
                return null;

            return st.FirstOrDefault();
        }

        public async Task<ICollection<int>> MeseciPrveGodine() 
        {
            List<int> meseci = new List<int>();

            for (int i = (await this.PrvoStanje()).Datum.Month; i < 13; i++)
                meseci.Add(i);

            return meseci;
        }


        public int PonudiBrDana(int mesec) 
        {
            return this.funkcije.PonudiBrDana(mesec);
        }


        public async Task<Stanje> AktuelnoStanje() 
        {
            var stanja = await Stanja();
            if (stanja == null ||  stanja.Count == 0)
                return null;
            else
            {
                var stanje =  stanja.Last();

                stanje.korisnikID = funkcije.PrijavljenID();
                stanje.korisnik = await mdb.NadjiPoIDu<Korisnik>("Korisnici", stanje.korisnikID);
                stanje.korisnik.DatumRodjenja = (await mdb.NadjiPoIDu<Korisnik>("Korisnici", stanje.korisnikID)).DatumRodjenja;

                return stanje;
            }
        }

        public async Task<string> PrikazBmi(Stanje stanje) 
        {
            var visina = stanje.Visina / 100;
            double vrednost = Math.Round(stanje.Tezina / (visina * visina), 2);
            var kategorija = funkcije.Kategorija(stanje.Visina, stanje.Tezina).ToString();
            if (kategorija == "NormalnaUhranjenost")
                kategorija = "normalna uhranjenost";
            else if (kategorija == "PrekomernaGojaznost")
                kategorija = "prekomerna gojaznost";
            else kategorija = kategorija.ToLower();

            return vrednost.ToString() + ", " + kategorija;
        }

        public async Task<Stanje> StanjePoDatumu(DateTime datum)
        {
            Stanje stanje = (await Stanja()).Where(s => s.Datum.Equals(datum)).FirstOrDefault();

              if (stanje == null)
              {
                 stanje = (await Stanja()).FirstOrDefault();
                  TimeSpan min =  datum - (await Stanja()).First().Datum;
                  if (min < TimeSpan.Zero)
                      min = -min;
                  foreach (var s in (await Stanja())) 
                  {
                      TimeSpan razlika = datum - s.Datum;
                      if (razlika < TimeSpan.Zero)
                          razlika = -razlika;

                      if (razlika < min) 
                      {
                          min = razlika;
                          stanje = s;
                      }
                  }

                  if (stanje == null)
                      return null;
              } 


            stanje.korisnikID = funkcije.PrijavljenID();
            stanje.korisnik = await mdb.NadjiPoIDu<Korisnik>("Korisnici", stanje.korisnikID);       
            stanje.korisnik.DatumRodjenja = (await mdb.NadjiPoIDu<Korisnik>("Korisnici", stanje.korisnikID)).DatumRodjenja;

            return stanje;
        }


        public async Task<Stanje> UpisiStanje(double visina, double tezina, double nt)
        {
            Stanje stanje = new Stanje(visina, tezina);
            stanje.korisnikID = funkcije.PrijavljenID();
            stanje.korisnik = await mdb.NadjiPoIDu<Korisnik>("Korisnici", stanje.korisnikID);
            stanje.korisnik.Pol = (await mdb.NadjiPoIDu<Korisnik>("Korisnici", stanje.korisnikID)).Pol;
            stanje.korisnik.DatumRodjenja = (await mdb.NadjiPoIDu<Korisnik>("Korisnici", stanje.korisnikID)).DatumRodjenja;
            stanje.Bmi = funkcije.Kategorija(visina, tezina);
            var bmr =  await IzracunajBMR(stanje.korisnik, visina, tezina);
            stanje.BMR = bmr;
            stanje.EnergetskePotrebe = Math.Ceiling(bmr * nt);
            stanje.UgljeniHidrati = Math.Ceiling(stanje.EnergetskePotrebe * 0.65 / 4.1);
            stanje.Protein = Math.Ceiling(stanje.EnergetskePotrebe * 0.125 / 4.1);
            stanje.Mast = Math.Ceiling(stanje.EnergetskePotrebe * 0.225 / 9.3);


            if (await PrvoStanje() == null)
            {
                var dani = await mdb.NadjiPoUslovu<Dan>("Dani", "korisnikID", funkcije.PrijavljenID());
                var danas = dani.Where(d => d.Datum.Date.Equals(DateTime.Today)).FirstOrDefault();
                // await context.Dani.Where(dan => dan.korisnikID == stanje.korisnikID 
                //       && dan.Datum.Date.Equals(DateTime.Today)).FirstOrDefaultAsync();
                if (danas != null)
                    danas.Rezultat = -stanje.EnergetskePotrebe;

                await mdb.Zameni<Dan>("Dani", danas.Id, danas);
            }

            var prethodno = await AktuelnoStanje();
            if (prethodno != null)
                await PromeniDnevniRezultat(prethodno.EnergetskePotrebe - stanje.EnergetskePotrebe);

            await mdb.Ubaci<Stanje>(stanja, stanje);
            return  stanje;
        }


        public async Task<string> ZadajCiljnuTezinu(double cilj)
        {
            var stanje = await AktuelnoStanje();
            double donjaGranica = Math.Floor(18.5 * (stanje.Visina * stanje.Visina) / 10000);
            double gornjaGranica = Math.Ceiling(25 * (stanje.Visina * stanje.Visina) / 10000);

            double promena = Math.Abs(stanje.Tezina - cilj) ;

            if (promena * 7700/365  > stanje.EnergetskePotrebe - 1000) 
            {
                double preporuka = Math.Ceiling((stanje.EnergetskePotrebe - 1000) * 365 / 7700) -1 ;
                return promena + " kg je prevelika promena za godinu dana. Za to vreme, preporučljivo bi bilo da izgubiš do " + preporuka + 
                    " kg. To bi značilo da postaviš za cilj težinu od " + (stanje.Tezina - preporuka) + " kg.";
            }

            double idealna = funkcije.IdealnaKilaza(stanje.korisnik.Pol, stanje.Visina);

            if ((cilj < stanje.Tezina) &&  stanje.Bmi == BMI.Neuhranjenost)
                    return "Treba da dobiješ na težini, ne smeš da gubiš. Ciljna težina treba da te bude " + idealna + " kg.";

            else if ((cilj < stanje.Tezina) && (funkcije.Kategorija(stanje.Visina, cilj) == BMI.Neuhranjenost))
                        return "Sa " + cilj + " kg bi se ubrajao/la u neuhranjene, izaberi ciljnu težinu od " + donjaGranica + " ili više kg.";
                                   
            else if ((cilj > stanje.Tezina) && stanje.Bmi >= BMI.Predgojaznost )    
                    return "Treba da izgubiš na težini, ne smeš da dobiješ. Ciljna težina treba da ti bude " + idealna + " kg."; 

            else if ((cilj > stanje.Tezina) && funkcije.Kategorija(stanje.Visina, cilj) >= BMI.Predgojaznost)
                        return "Sa " + cilj + " kg bi se ubrajao/la u predgojazne, ili gojazne, izaberi ciljnu težinu od " + gornjaGranica + " ili manje kg.";

            else if (stanje.Tezina == cilj)
            {
                stanje.CiljnaKilaza = stanje.Tezina;
                stanje.CiljniUnos = stanje.EnergetskePotrebe;
                await mdb.Zameni<Stanje>(stanja, stanje.Id, stanje);

                return "Upisana je ciljna težina od " + stanje.CiljnaKilaza + " kg, što znači da održavaš svoju trenutnu težinu, unosom od "
                   + stanje.CiljniUnos + " kcal dnevno u narednom periodu.";
            }
            else
            {
                 stanje.CiljnaKilaza = cilj;
                 await mdb.Zameni<Stanje>(stanja, stanje.Id, stanje);

                return "Upisana je ciljna težina od " + stanje.CiljnaKilaza + " kg.";
            }
        }


        public async Task<string> ZadajVreme(int brojDana) 
        {
            var stanje = await AktuelnoStanje();
            double doCiljneTezine = (stanje.Tezina - stanje.CiljnaKilaza) * 7700;

            if ((stanje.CiljnaKilaza < stanje.Tezina) && ((doCiljneTezine /brojDana ) > (stanje.EnergetskePotrebe - 1000)))          
                return "Izaberi duži vremenski period, " + NedeljeMeseci(brojDana) + " je prekratko vreme za gubljenje " + (stanje.Tezina - stanje.CiljnaKilaza) + 
                    " kg. Predlog je da unosiš " + (stanje.EnergetskePotrebe - 500) + " kcal dnevno u periodu od " + NedeljeMeseci((int)doCiljneTezine/500 + 1) + 
                    ". Za brži rezultat možeš unositi 1000 kcal dnevno u periodu od " + NedeljeMeseci((int)(doCiljneTezine/(stanje.EnergetskePotrebe - 1000) + 1) ) +".";

            else if (stanje.Bmi == BMI.NormalnaUhranjenost && (stanje.CiljnaKilaza > stanje.Tezina) && (doCiljneTezine / brojDana > 500))
                return "Izaberi duži vremenski period, " + NedeljeMeseci(brojDana) + " je prekratko vreme za dobijanje " + (stanje.CiljnaKilaza - stanje.Tezina) + 
                    " kg. Predlog je da unosiš " + (stanje.EnergetskePotrebe + 500) + " kcal dnevno u periodu od " + NedeljeMeseci((int)doCiljneTezine / 500 + 1) + ".";

            else if (stanje.Bmi == BMI.Neuhranjenost && (-doCiljneTezine / brojDana > 1000))                 
                return "Izaberi duži vremneski period, " + NedeljeMeseci(brojDana) + " je prekratko vreme za dobijanje " + ( stanje.CiljnaKilaza - stanje.Tezina) + 
                    " kg. Predlog je da unosiš od " +  (stanje.EnergetskePotrebe + 1000) + " kcal dnevno u periodu od " + NedeljeMeseci((int)doCiljneTezine / 1000 + 1) + ".";
            else
            {
                stanje.CiljniUnos = Math.Floor(stanje.EnergetskePotrebe - doCiljneTezine / brojDana);
                await mdb.Zameni<Stanje>(stanja, stanje.Id, stanje);

                return "Izabrano vreme je upisano. Predlog je da unosiš " + stanje.CiljniUnos + " kcal dnevno u periodu od " + NedeljeMeseci((int)brojDana) + ".";
            }                                                        
        }


        public async Task<Stanje> PromeniVisinu(double visina)
        {
            var stanje = await AktuelnoStanje();
            stanje.Visina = Math.Round(visina, 2);
            var stareEP = stanje.EnergetskePotrebe;

            stanje.Bmi = funkcije.Kategorija(visina, stanje.Tezina);
            double koef = stanje.BMR / (await IzracunajBMR(stanje.korisnik, visina, stanje.Tezina));
            stanje.BMR = await IzracunajBMR(stanje.korisnik, visina, stanje.Tezina);
            stanje.EnergetskePotrebe = Math.Ceiling(koef * stanje.EnergetskePotrebe);
            stanje.UgljeniHidrati = Math.Ceiling(stanje.EnergetskePotrebe * 0.65 / 4.1);
            stanje.Protein = Math.Ceiling(stanje.EnergetskePotrebe * 0.125 / 4.1);
            stanje.Mast = Math.Ceiling(stanje.EnergetskePotrebe * 0.225 / 9.3);

            await PromeniDnevniRezultat(stareEP - stanje.EnergetskePotrebe);

            await mdb.Zameni<Stanje>(stanja, stanje.Id, stanje);
            return stanje;
        }

        public async Task<Stanje> PromeniTezinu(double tezina) 
        {
            var stanje = await AktuelnoStanje();
            stanje.Tezina = Math.Round(tezina, 2);
            var stareEP = stanje.EnergetskePotrebe;

            stanje.Bmi = funkcije.Kategorija(stanje.Visina, tezina);
            double koef =  (await IzracunajBMR(stanje.korisnik, stanje.Visina, tezina)) / stanje.BMR;
            stanje.BMR = await IzracunajBMR(stanje.korisnik, stanje.Visina, tezina);
     
            stanje.EnergetskePotrebe = Math.Ceiling(koef * stanje.EnergetskePotrebe);
            stanje.UgljeniHidrati = Math.Ceiling(stanje.EnergetskePotrebe * 0.65 / 4.1);
            stanje.Protein = Math.Ceiling(stanje.EnergetskePotrebe * 0.125 / 4.1);
            stanje.Mast = Math.Ceiling(stanje.EnergetskePotrebe * 0.225 / 9.3);

            await PromeniDnevniRezultat(stareEP - stanje.EnergetskePotrebe);

            await mdb.Zameni<Stanje>(stanja, stanje.Id, stanje);
            return stanje;
        }


        public async Task<Dan> PromeniDnevniRezultat(double promena) 
        {
            var danas = (await mdb.NadjiPoUslovu<Dan>("Dani", "korisnikID", funkcije.PrijavljenID())).
                Where(dan => dan.Datum.Date.Equals(DateTime.Today)).FirstOrDefault();
           
            if (danas != null) 
                danas.Rezultat += promena;

            var stanje = await AktuelnoStanje();

            var izvestaj = (await mdb.NadjiPoUslovu<Izvestaj>("Izvestaji", "danID", danas.Id)).FirstOrDefault();
                //context.Izvestaji.Where(i => i.danID == danas.Id).FirstOrDefaultAsync();
            if (izvestaj != null) 
            {
                string pre = "Izveštaj za " + funkcije.DatumToString(DateTime.Today) + Environment.NewLine + "Energetske potrebe: ";
                string stareEP = izvestaj.Poruka.Substring(pre.Length, stanje.EnergetskePotrebe.ToString().Length);
                izvestaj.Poruka = izvestaj.Poruka.Replace(stareEP, stanje.EnergetskePotrebe.ToString());
                await mdb.Zameni<Izvestaj>("Izvestaji", izvestaj.Id, izvestaj);
            }


            await mdb.Zameni<Dan>("Dani", danas.Id, danas);
            return danas; 
        } 



        public async Task<string> ObrisiStanje()
        {
            var stanje = await AktuelnoStanje();
            if (stanje == null)
                return "Nije upisano nijedno stanje.";

            var ep = stanje.EnergetskePotrebe;
            await mdb.Obrisi<Stanje>(stanja, stanje.Id);

            var prethodno = await AktuelnoStanje();
            if(prethodno != null)
                 await PromeniDnevniRezultat(ep -  prethodno.EnergetskePotrebe);


            return "Stanje je obrisano.";
        }


        public string ProveriDatum(int godina, int mesec, int dan)
        {
            return funkcije.ProveriDatum(godina, mesec, dan);
        }


 /*       public BMI Kategorija(double visina, double tezina)
        {
            visina /= 100;
            double bmi = tezina / (visina * visina);

            BMI k = BMI.NormalnaUhranjenost;

            switch (bmi)
            {
                case double b when b < 18.5:
                    k = BMI.Neuhranjenost;
                    break;

                case double b when (b >= 18.5) && (b < 0.25):
                    k = BMI.NormalnaUhranjenost;
                    break;

                case double b when (b >= 25) && (b < 30):
                    k = BMI.Predgojaznost;
                    break;

                case double b when (b >= 30) && (b < 35):
                    k = BMI.Gojaznost;
                    break;

                case double b when b >= 35:
                    k = BMI.PrekomernaGojaznost;
                    break;
            }
            return k;
        }
 */





        public async Task<double> IzracunajBMR(Korisnik korisnik, double visina, double tezina)
        {
            return funkcije.IzracunajBMR(korisnik, visina, tezina);
           // var korisnik = await mdb.NadjiPoIDu<Korisnik>("Korisnici", funkcije.PrijavljenID());
         /*   int godine =  DateTime.Today.Year  -  korisnik.DatumRodjenja.Year;
            double bmr = 0;

            var k = funkcije.Kategorija(visina, tezina);


            if (!k.Equals(BMI.NormalnaUhranjenost))
                 tezina = IdealnaKilaza(korisnik.Pol, visina);


            if (korisnik.Pol == PolKorisnika.Muski)
            {
                switch (godine)
                {
                     case int g when (g <= 2):
                        bmr = 60.9 * tezina - 54;
                        break; 
                    case int g when ((g >= 3) && (g <= 9)):
                        bmr = 22.7 * tezina + 495;
                        break;
                    case int g when ((g >= 10) && (g <= 17)):
                        bmr = 17.5 * tezina + 651;
                        break;
                    case int g when ((g >= 18) && (g <= 29)):
                        bmr = 15.3 * tezina + 679;
                        break;
                    case int g when ((g >= 30) && (g <= 59)):
                        bmr = 11.6 * tezina + 879;
                        break;
                    case int g when (g >= 60):
                        bmr = 13.5 * tezina + 487;
                        break;
                }
            }
            else
            {
                switch (godine)
                {
                    case int g when (g <= 2):
                        bmr = 61 * tezina - 51;
                        break;
                    case int g when ((g >= 3) && (g <= 9)):
                        bmr = 22.5 * tezina + 499;
                        break;
                    case int g when ((g >= 10) && (g <= 17)):
                        bmr = 12.2 * tezina + 746;
                        break;
                    case int g when ((g >= 18) && (g <= 29)):
                        bmr = 14.7 * tezina + 496;
                        break;
                    case int g when ((g >= 30) && (g <= 59)):
                        bmr = 8.7 * tezina + 829;
                        break;
                    case int g when (g >= 60):
                        bmr = 10.5 * tezina + 596;
                        break;
                }
            }


            return Math.Ceiling(bmr); */
        }



        public async Task<List<string>> PonudiPeriode() 
        {
            var stanje = await AktuelnoStanje();
            List<string> periodi = new List<string>();
            double doCiljneTezine = (stanje.Tezina - stanje.CiljnaKilaza) * 7700;
            
            int minPeriod = 0;
            int maxPeriod = 0;

            if (doCiljneTezine > 0)  
            {
                minPeriod = (int)(doCiljneTezine / (stanje.EnergetskePotrebe - 1000));
                maxPeriod = (int)(doCiljneTezine /  500);
            }
            else 
            {
                doCiljneTezine = -doCiljneTezine;
                minPeriod = (int)(doCiljneTezine / 1000);
                maxPeriod = (int)(doCiljneTezine /  500);
            }

            var ned = minPeriod / 7;
            if (minPeriod % 7 > 3)
                minPeriod = ned* 7 + 1;
            else minPeriod = ned * 7;

            ned = maxPeriod / 7;
            if (maxPeriod % 7 > 3)
                maxPeriod = ned * 7 + 1;
            else maxPeriod = ned * 7;

            for (int i = minPeriod; i <= maxPeriod; i += 7)
                periodi.Add(NedeljeMeseci(i));

            return  periodi;
        }

        public int ParsirajPeriod(string period) 
        {
            int brDana = 0;
            if (period.Contains("godina"))
                brDana = 365;

            if (period.Contains("mesec"))
            {
                if (period.Split(' ')[0].Equals("mesec"))
                    brDana = 30;
                else if(period.Split(' ')[1].Equals("meseca") || period.Split(' ')[1].Equals("meseci"))
                    brDana = 30 * Convert.ToInt32((period.Split(' ')[0]));
            }
            else if (period.Contains("i") && period.Contains("nedelj"))
                brDana += 7 * Convert.ToInt32(period.Split(' ')[9]);
            else if (!period.Contains("i") &&  period.Contains("nedelj"))
                brDana = 7 * Convert.ToInt32(period.Split(' ')[0]);  
            else
                brDana += Convert.ToInt32(period.Split(' ')[0]);

            return brDana;
        }
        
        public string NedeljeMeseci(int brojDana)
        {
            if (brojDana < 0)
                brojDana = -brojDana;

            string poruka = String.Empty;

            if (brojDana < 31)
                poruka = brojDana + " dana";
            else
            {
                int ostatak = 0;
                int brojNedelja = Math.DivRem(brojDana, 7, out ostatak);

                if (ostatak > 3)
                    brojNedelja += 1;

                if (brojNedelja < 4)
                    poruka += brojNedelja + " nedelje";
                else
                {
                    int brojMeseci = Math.DivRem(brojNedelja, 4, out ostatak);

                    if (brojMeseci == 1)
                        poruka += "mesec";
                    else if ((brojMeseci > 1) && (brojMeseci < 5))
                        poruka += brojMeseci + " meseca";
                    else
                        poruka += brojMeseci + " meseci";

                    if (ostatak == 0 && brojMeseci == 1)
                        poruka += " dana";
                    else if (ostatak == 1)
                        poruka += " i nedelju dana";
                    else if (ostatak > 1)
                        poruka += " i " + ostatak + " nedelje";
                }
            }
            return poruka;
        }



    }
}
