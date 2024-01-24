

using API.Model;
using API.Servisi.Interfejsi;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace API.Data
{
    public class MongoDbServis : IMongoDB
    {

        public PomocneFunkcije funkcije;

        public const string aktivnosti = "Aktivnosti";
        public const string korisnici = "Korisnici";
        public const string namirnice = "Namirnice";


        public MongoDbServis()
        {
            funkcije = new PomocneFunkcije();
            Povezi();
        }

        public MongoClient client = new MongoClient();
        public IMongoDatabase db;
        public void Povezi() 
        {       
            client = new MongoClient("mongodb://localhost:27017");
            db = client.GetDatabase("brojac_db");
        }

        public async Task <IMongoCollection<T>> UcitajKolekciju<T>( string kolekcija)
        {
            return  db.GetCollection<T>(kolekcija);
        }


        public async Task<List<T>> UcitajListu<T>(string kolekcija) 
        {
           var rez = await UcitajKolekciju<T>(kolekcija);
           return await rez.Find( t => true).ToListAsync();
        }
        

        public async Task<T> NadjiPoIDu<T>(string kolekcija, string id)
        {
            var rez = await UcitajKolekciju<T>(kolekcija);
            var filter = Builders<T>.Filter.Eq("Id", id);

            return await rez.Find(filter).FirstOrDefaultAsync();
        }


        public async Task<List<T>> NadjiSlicne<T>(string kolekcija, string polje, string vrednost) 
        {
            var slicni = new List<T>();
            var rez = await UcitajListu<T>(kolekcija);
            foreach (var r in rez) 
            {
                var p = r.GetType().GetProperty(polje);
                var vrednostP = p.GetValue(r).ToString();
                if ( vrednostP.Contains(vrednost) || vrednost.Contains(vrednostP))
                    slicni.Add(r);
            }

            return slicni;
        }


        public async Task<List<T>> NadjiPoUslovima<T>(string kolekcija, List<string> polja, List<string>vrednosti) 
        {
            var rez = await UcitajKolekciju<T>(kolekcija);
            var filter = Builders<T>.Filter.Eq(polja[0], vrednosti[0]);
            for (int i = 1; i < polja.Count; i++) 
            {
                var f = Builders<T>.Filter.Eq(polja[i], vrednosti[i]);
                filter = filter & f;
            }

            return await rez.Find(filter).ToListAsync();
        }

       public async Task<List<T>> NadjiPoUslovuIzListe<T>
            (ICollection<T> lista, string polje, string vrednost) 
        {
            var kolekcija = (IMongoCollection<T>)lista;
            var filter = Builders<T>.Filter.Eq(polje, vrednost);

            return await kolekcija.Find(filter).ToListAsync();
        }

        public async Task<List<T>> NadjiPoUslovu<T>(string kolekcija,
                                      string polje, string vrednost)
        {
            var rez = await UcitajKolekciju<T>(kolekcija);
            var filter = Builders<T>.Filter.Eq(polje, vrednost);

            return await rez.Find(filter).ToListAsync();
        }
        public async Task<T> NadjiPoveznik<T>(string kolekcija, 
            string polje1, string polje2, string id1, string id2)
        {
            var rez = await UcitajKolekciju<T>(kolekcija);
            var filter = Builders<T>.Filter.Eq(polje1, id1);
            var filter2 = Builders<T>.Filter.Eq(polje2, id2);
            return await rez.Find(filter & filter2).FirstOrDefaultAsync();
        }
        public async Task Ubaci<T>(string kolekicja, T element)
        {
            var rez = await UcitajKolekciju<T>(kolekicja);
            await rez.InsertOneAsync(element);
        }
        public async Task<T> Zameni<T>(string kolekcija, string id, T element)
        {
            var rez = await UcitajKolekciju<T>(kolekcija);
            var filter = Builders<T>.Filter.Eq("Id", id);
            if (filter != null)
               await rez.ReplaceOneAsync(filter, element);

            return await NadjiPoIDu<T>(kolekcija, id);
        }

        public async Task<T> PromeniPoveznik<T>(string kolekcija, string polje1, string polje2, string id1, string id2, T novi)
        {
            var rez = await UcitajKolekciju<T>(kolekcija);
            var filter1 = Builders<T>.Filter.Eq(polje1, id1);
            var filter2 = Builders<T>.Filter.Eq(polje2, id2);
            var f = filter1 & filter2;
            await rez.ReplaceOneAsync(f, novi);

            return await NadjiPoveznik<T>(kolekcija, polje1, polje2, id1, id2);
        }

        public async Task Obrisi<T>(string kolekcija, string id)
        {
            var rez = await UcitajKolekciju<T>(kolekcija);
            var filter = Builders<T>.Filter.Eq("Id", id);

            await rez.DeleteOneAsync(filter);
        }

        public async Task ObrisiListu<T>(string kolekcija, ICollection<T> lista) 
        {
            var rez = await UcitajKolekciju<T>(kolekcija);
            var filter = Builders<T>.Filter.Where(k => lista.Contains(k));

            await rez.DeleteManyAsync(filter);
        }
        public async Task ObrisiVeze<T>(string kolekcija, string id, string polje) 
        {
            var rez = await UcitajKolekciju<T>(kolekcija);
            var filter = Builders<T>.Filter.Eq(polje, id);
            await rez.DeleteManyAsync(filter);
        }

        public async Task ObrisiPoveznik<T>(string kolekcija, 
            string id, string polje, string id2, string polje2)
        {
            var rez = await UcitajKolekciju<T>(kolekcija);
            var filter = Builders<T>.Filter.Eq(polje, id);
            var filter2 = Builders<T>.Filter.Eq(id2, polje2);
            await rez.DeleteOneAsync(filter & filter2);
        }







        // funkcije za logger

        public async Task RegistracijaAdmina()
        {
            byte[] kljuc1;
            byte[] sifra1;        
            funkcije.SifraHash("78907890", out sifra1, out kljuc1);
            var aa = new AdministratorAktivnosti("Sandy", sifra1, kljuc1);

            byte[] kljuc2;
            byte[] sifra2;
            funkcije.SifraHash("88888888", out sifra2, out kljuc2);
            var an = new AdministratorNamirnica("KebaKraba", sifra2, kljuc2);

            var adminiA = await UcitajKolekciju<AdministratorAktivnosti>("AdminiAktivnosti");
            var adminiN = await UcitajKolekciju<AdministratorNamirnica>("AdminiNamirnica");

            var pocetak = DateTime.Now;
            await adminiA.InsertOneAsync(aa);
            await adminiN.InsertOneAsync(an);
            var kraj = DateTime.Now;
            var t = (kraj - pocetak).Milliseconds;

            Log.Information(Environment.NewLine + "Administratori su registrovani za " + t + " milisekundi."
                         + Environment.NewLine + "Prosečno vreme upisa jednog administratora iznosi "
                         + Math.Round((double)(t / 2), 2) + " milisekundi." + Environment.NewLine);
        }


        public async Task PromenaAdmina() 
        {
            var adminiA = await UcitajKolekciju<AdministratorAktivnosti>("AdminiAktivnosti");
            var an = (await UcitajListu<AdministratorNamirnica>("AdminiNamirnica")).FirstOrDefault();
  
            var adminiN = await UcitajKolekciju<AdministratorNamirnica>("AdminiNamirnica");
            var aa = (await UcitajListu<AdministratorAktivnosti>("AdminiAktivnosti")).FirstOrDefault();

            byte[] kljuc;
            byte[] sifra;
            funkcije.SifraHash("78907899", out sifra, out kljuc);
            an.Ime = "Keba Kraba";
            aa.Sifra = sifra;
            aa.Kljuc = kljuc;

            var pocetak = DateTime.Now;
            await adminiA.ReplaceOneAsync(Builders<AdministratorAktivnosti>.Filter.Eq("Id", aa.Id), aa);
            await adminiN.ReplaceOneAsync(Builders<AdministratorNamirnica>.Filter.Eq("Id", an.Id), an);
            var kraj = DateTime.Now;
            var t = (kraj - pocetak).Milliseconds;

            Log.Information(Environment.NewLine + "Administratori su promenjeni za " + t + " milisekundi."
                         + Environment.NewLine + "Prosečno vreme promene jednog administratora iznosi "
                         + Math.Round((double)t / 2, 2) + " milisekundi." + Environment.NewLine);
        }

        public async Task<int> Registruj(Korisnik k, string slika, double v, double t, double c, double nt) 
        {
            var rez = await UcitajKolekciju<Korisnik>(korisnici);
            var pocetak = DateTime.Now;
            await rez.InsertOneAsync(k);
            var kraj = DateTime.Now;
            var tt = (kraj - pocetak).Milliseconds;  

            Stanje s = new Stanje(v, t);
            s.korisnik = (await NadjiPoUslovu<Korisnik>(korisnici, "Ime", k.Ime)).FirstOrDefault();
            s.korisnikID = s.korisnik.Id;
            s.Visina = v;
            s.Tezina = t;
            s.Datum = DateTime.Today;
            s.CiljnaKilaza = c;
            s.Bmi = funkcije.Kategorija(v, t);
            var bmr = funkcije.IzracunajBMR(s.korisnik, v, t);
            s.BMR = bmr;
            s.EnergetskePotrebe = Math.Ceiling(bmr * nt);
            s.UgljeniHidrati = Math.Ceiling(s.EnergetskePotrebe * 0.65 / 4.1);
            s.Protein = Math.Ceiling(s.EnergetskePotrebe * 0.125 / 4.1);
            s.Mast = Math.Ceiling(s.EnergetskePotrebe * 0.225 / 9.3);

            double doCiljneTezine = (t - c) * 7700;

            if ((c < t) && ((doCiljneTezine / 30) > (s.EnergetskePotrebe - 1000)))
                s.CiljniUnos = s.EnergetskePotrebe - 500;
            else if (s.Bmi == BMI.NormalnaUhranjenost && (c > t) && (doCiljneTezine / 30 > 500))
                s.CiljniUnos = s.EnergetskePotrebe + 500;

            else if (s.Bmi == BMI.Neuhranjenost && (-doCiljneTezine / 30 > 1000))
                s.CiljniUnos = s.EnergetskePotrebe + 1000;

            else s.CiljniUnos = Math.Floor(s.EnergetskePotrebe - doCiljneTezine / 30);

            k.Slika = slika;

            var rezS = await UcitajKolekciju<Stanje>("Stanja");

            pocetak = DateTime.Now;
            await rez.ReplaceOneAsync(Builders<Korisnik>.Filter.Eq("Id", k.Id), k);
            await rezS.InsertOneAsync(s);
            kraj = DateTime.Now;
            tt = tt + (kraj - pocetak).Milliseconds;

            return tt;
        }



        public async Task RegistracijaPrvihKorisnika()
        {
            byte[] kljuc;
            byte[] sifra;

            funkcije.SifraHash("12341234", out sifra, out kljuc);
            Korisnik k = new Korisnik("Patrick", sifra, kljuc, new DateTime(1995, 3, 29), 0, "*");
          
            var vreme =  (await Registruj(k, "slika1.png", 175, 70, 65, 1.6));

            funkcije.SifraHash("56785678", out sifra, out kljuc);
            k = new Korisnik("Lignjoslav", sifra, kljuc, new DateTime(1990, 9, 1), 0, "*");
            vreme = vreme + (await Registruj(k, "slika2.png", 195,90, 93, 1.9));

            funkcije.SifraHash("77777799", out sifra, out kljuc);
            k = new Korisnik("Mardž", sifra, kljuc, new DateTime(1963, 7, 28), 0, "*");
            vreme = vreme + (await Registruj(k, "slika3.png", 162, 60, 55, 1.75));

            funkcije.SifraHash("99999977", out sifra, out kljuc);
            k = new Korisnik("Bart", sifra, kljuc, new DateTime(1991, 11, 19), 0, "*");
            vreme = vreme + (await Registruj(k, "slika4.png", 180, 65, 70, 2));

            Log.Information(Environment.NewLine + "Korisnici su registrovani za " + vreme + " milisekundi."
                         + Environment.NewLine + "Prosečno vreme upisa jednog korisnika iznosi "
                         + Math.Round((double)vreme / 4, 2) + " milisekundi." + Environment.NewLine);
        }


        public async Task PromenaPrvihKorisnika()
        {
            byte[] kljuc;
            byte[] sifra;
            funkcije.SifraHash("99997777", out sifra, out kljuc);

            var rez = await UcitajKolekciju<Korisnik>(korisnici);

            Korisnik k1 = (await NadjiPoUslovu<Korisnik>(korisnici, "Ime", "Patrick")).FirstOrDefault();  
            k1.Ime = "Patrik";

            Korisnik k2= (await NadjiPoUslovu<Korisnik>(korisnici, "Ime", "Lignjoslav")).FirstOrDefault();
            k2.Slika = "slika4.png";

            Korisnik k3 = (await NadjiPoUslovu<Korisnik>(korisnici, "Ime", "Mardž")).FirstOrDefault();        
            k3.Sifra = sifra;
            k3.Kljuc = kljuc; 

            Korisnik k4 = (await NadjiPoUslovu<Korisnik>(korisnici, "Ime", "Bart")).FirstOrDefault();
            k4.DatumRodjenja = new DateTime(1991, 9, 8);
 

            var pocetak = DateTime.Now;
            await rez.ReplaceOneAsync(Builders<Korisnik>.Filter.Eq("Id", k1.Id), k1);
            await rez.ReplaceOneAsync(Builders<Korisnik>.Filter.Eq("Id", k2.Id), k2);
            await rez.ReplaceOneAsync(Builders<Korisnik>.Filter.Eq("Id", k3.Id), k3);
            await rez.ReplaceOneAsync(Builders<Korisnik>.Filter.Eq("Id", k4.Id), k4);
            var kraj = DateTime.Now;

            var t = (kraj - pocetak).Milliseconds;

            Log.Information(Environment.NewLine + "Korisnici su promenjeni za " + t + " milisekundi."
                         + Environment.NewLine + "Prosečno vreme promene jednog korisnika iznosi "
                         + Math.Round((double)t / 4, 2) + " milisekundi." + Environment.NewLine);
        }

        public async Task BrisanjePrvihKorisnika()
        {
            var korisniciZaBrisanje = await UcitajKolekciju<Korisnik>(korisnici);
            var stanja = await UcitajKolekciju<Stanje>("Stanja");

            var pocetak = DateTime.Now;
            await db.DropCollectionAsync("Stanja");
            await db.DropCollectionAsync(korisnici);
          //  await stanja.DeleteManyAsync<Stanje>(st);
            //await korisniciZaBrisanje.DeleteManyAsync<Korisnik>(filterK);
  
            var kraj = DateTime.Now;
            var t = (kraj - pocetak).Milliseconds;

            Log.Information(Environment.NewLine + "Korisnici su obrisani za " + t + " milisekundi."
                         + Environment.NewLine + "Prosečno vreme brisanja jednog korisnika i jednog stanja iznosi "
                         + Math.Round((double)t / 4, 2) + " milisekundi." + Environment.NewLine);
        }

        public async Task UpisPrvihNamirnica() 
        {
            var namirniceZaUpis = new List<Namirnica>();

            namirniceZaUpis.AddRange(new Namirnica[] { new Namirnica("paprika", VrstaNamirnice.Povrce, TipObrade.Sveza,
                                            KolicinaBrasna.Bez, KolicinaMasti.Bez, 30, 2, 10, 0, 1, "somborka"),
                                    new Namirnica("jabuka", VrstaNamirnice.Voce, TipObrade.Sveza,
                                            KolicinaBrasna.Bez, KolicinaMasti.Bez, 52.1, 0.3, 14, 0.2, 1, "petrovača"),
                                    new Namirnica("pržena piletina", VrstaNamirnice.Meso, TipObrade.Przena,
                                                KolicinaBrasna.Pohovano, KolicinaMasti.Duboko, 262, 26.75, 3.18, 14.98, 0.67, "pileći batak pržen u tiganju"),
                                    new Namirnica("karfiol", VrstaNamirnice.Povrce, TipObrade.Kuvana,
                                                KolicinaBrasna.Bez, KolicinaMasti.Bez, 40, 1.9, 5, 0.3, 0.92, "kuvan karfiol"),
                                    new Namirnica("pečeno jaje", VrstaNamirnice.Meso, TipObrade.Pecena,
                                                KolicinaBrasna.Bez, KolicinaMasti.Bez, 143, 12.6, 0.9, 8.7, 0.91, "jaje na oko pečeno bez masti"),
                                    new Namirnica("banana", VrstaNamirnice.Voce, TipObrade.Sveza,
                                                KolicinaBrasna.Bez, KolicinaMasti.Bez, 88.7, 0.2, 17, 0.5, 1, "sveža banana"),
                                    new Namirnica("pljeskavica", VrstaNamirnice.Meso, TipObrade.Pecena,
                                                KolicinaBrasna.Bez, KolicinaMasti.Plitko, 350, 20, 10, 30, 0.78, "pljeskavica od mlevenog mesa pečena na malo ulja"),
                                    new Namirnica("kukuruz", VrstaNamirnice.Zitarica, TipObrade.Kuvana, KolicinaBrasna.Bez,
                                                KolicinaMasti.Bez, 112, 3.3, 22.6, 1.4, 1, "kuvani kukuruz šećerac"),
                                    new Namirnica("mandarina", VrstaNamirnice.Voce, TipObrade.Sveza, KolicinaBrasna.Bez,
                                                KolicinaMasti.Bez, 47, 0.7, 12, 0, 1, "sveža mandarina"),
                                    new Namirnica("kupus", VrstaNamirnice.Povrce, TipObrade.Sveza, KolicinaBrasna.Bez,
                                                KolicinaMasti.Bez, 24, 1.3, 6, 0.1, 1, "svež kupus, salata"),
                                    new Namirnica("kuvana šargarepa", VrstaNamirnice.Povrce, TipObrade.Kuvana, KolicinaBrasna.Bez,
                                                 KolicinaMasti.Bez, 17.6, 0.4, 4.1, 0.1, 0.89, "šagrarepa, obarena u slanoj vodi"),
                                    new Namirnica("pržen crni luk", VrstaNamirnice.Povrce, TipObrade.Przena, KolicinaBrasna.Bez,
                                                  KolicinaMasti.Plitko, 251, 4.5, 27.5, 13.5, 0.65, "crni luk, pržen u ulju"),
                                    new Namirnica("kuvan brokoli", VrstaNamirnice.Povrce, TipObrade.Kuvana, KolicinaBrasna.Bez,
                                                  KolicinaMasti.Bez, 21, 2.9, 2, 0.2, 1.11, "brokoli, skuvan u slanoj vodi"),
                                    new Namirnica("kuvana boranija", VrstaNamirnice.Povrce, TipObrade.Kuvana, KolicinaBrasna.Bez,
                                                    KolicinaMasti.Bez, 65.9, 0.8, 4.7, 5, 0.93, "boranija, skuvana u slanoj vodi"),
                                    new Namirnica("dinstane pečurke", VrstaNamirnice.Ostalo, TipObrade.Dinstana, KolicinaBrasna.Bez,
                                                    KolicinaMasti.Plitko, 121.2, 5.8, 8.7, 7.1, 0.81, "pečurke izdinstane na ulju"),
                                    new Namirnica("pomfrit", VrstaNamirnice.Povrce, TipObrade.Przena, KolicinaBrasna.Bez,
                                                KolicinaMasti.Duboko, 298.3, 3.3, 39, 14.9, 0.56, "pomfrit, krompir pržen u dubukom ulju ili na masti"),
                                    new Namirnica("pržena pastrmka", VrstaNamirnice.Meso, TipObrade.Przena, KolicinaBrasna.Sa,
                                                KolicinaMasti.Plitko, 195, 19.3, 0.5, 13, 0.58, "pastrmka, riba uvaljana u brašno, pa pržena na ulju"),
                                    new Namirnica("kuvan goveđi jezik", VrstaNamirnice.Meso, TipObrade.Kuvana, KolicinaBrasna.Bez,
                                                KolicinaMasti.Bez, 90, 16, 2, 12, 0.8, "goveđi jezik, kuvan u slanoj vodi, bez masti"),
                                    new Namirnica("pohovana piletina", VrstaNamirnice.Meso, TipObrade.Przena, KolicinaBrasna.Pohovano,
                                                KolicinaMasti.Duboko, 253, 16.6, 17.3, 12.6, 0.82, "pileće belo meso, pohovano u brašnu i jajima, pa prženo"),
                                    new Namirnica("kuvani krompir", VrstaNamirnice.Povrce, TipObrade.Kuvana, KolicinaBrasna.Bez, KolicinaMasti.Bez, 89, 2, 23, 0, 0.95,
                                                "krompir skuvan u slanoj vodi")
                                     });

            foreach (var n in namirniceZaUpis)
            {
                n.admin = null;
                n.adminID = null;
            }

            var prva = (await NadjiPoUslovu<Namirnica>(namirnice, "Naziv", "prva namirnica")).FirstOrDefault().Id;
            await Obrisi<Namirnica>(namirnice, prva);

            var kolekcijaN = await UcitajKolekciju<Namirnica>("Namirnice");

            var pocetak = DateTime.Now;
            await kolekcijaN.InsertManyAsync(namirniceZaUpis);
          //  foreach (var n in namirniceZaUpis)
            //    await Ubaci<Namirnica>(namirnice, n);

            var kraj = DateTime.Now;
            var t = (kraj - pocetak).Milliseconds;

            Log.Information(Environment.NewLine + "Namirnice su upisane za " + t + " milisekundi."
                         + Environment.NewLine + "Prosečno vreme upisa jedne namirnice iznosi "
                         + Math.Round((double)t / 20, 2) + " milisekundi." + Environment.NewLine);
        }


        public async Task PromenaPrvihNamirnica()
        {
            var sve = await UcitajListu<Namirnica>(namirnice);
            var zaPromenuOpisa = (sve).Take(10);
            var t = 0;
            var kolekcijaN = await UcitajKolekciju<Namirnica>(namirnice);

            foreach (var n in sve) 
            {
                if(zaPromenuOpisa.Contains(n))
                    n.Opis += " - promenjen opis.";
                else n.PromenaMase = Math.Round(n.PromenaMase * 1.5, 2);

                var pocetak = DateTime.Now;
                await kolekcijaN.ReplaceOneAsync(Builders<Namirnica>.Filter.Eq("Id", n.Id), n);
             //   await Zameni<Namirnica>(namirnice, n.Id, n);
                var kraj = DateTime.Now;
                t = t + (kraj - pocetak).Milliseconds; 
            }
            
            Log.Information(Environment.NewLine + "Namirnice su promenjene za " + t + " milisekundi."
                         + Environment.NewLine + "Prosečno vreme promene jedne namirnice iznosi "
                         + Math.Round((double)t / 20, 2) + " milisekundi." + Environment.NewLine);
        }


        public async Task BrisanjePrvihNamirnica()
        {
           // var namirniceZaBrisanje = await UcitajListu<Namirnica>(namirnice);

            var pocetak = DateTime.Now;
            await db.DropCollectionAsync(namirnice);
            //await ObrisiListu<Namirnica>(namirnice, namirniceZaBrisanje);
            var kraj = DateTime.Now;

            var t = (kraj - pocetak).Milliseconds;

            Log.Information(Environment.NewLine + "Namirnice su obrisane za " + t + " milisekundi."
                         + Environment.NewLine + "Prosečno vreme brisanja jedne namirnice iznosi "
                         + Math.Round((double)t / 20, 2) + " milisekundi." + Environment.NewLine);
        }


        public async Task<Aktivnost> UpisPrveAktivnosti()
        {
            var a = new Aktivnost("prva aktivnost", 1.5);
            await Ubaci<Aktivnost>(aktivnosti,a);
            return a;
        }

        public async Task<Namirnica> UpisPrveNamirnice() 
        {
            var n = new Namirnica("prva namirnica", VrstaNamirnice.Meso, TipObrade.Pecena,
                   KolicinaBrasna.Bez, KolicinaMasti.Duboko, 350, 75, 4, 20, 0.75, "prva namirnica, za probu");

            await Ubaci<Namirnica>(namirnice, n);
            return n;
        }
        public async Task UpisPrvihAktivnosti()
        {
            var prva = (await NadjiPoUslovu<Aktivnost>(aktivnosti, "Naziv", "prva aktivnost")).FirstOrDefault().Id;
            await Obrisi<Aktivnost>(aktivnosti, prva);
            List<Aktivnost> aktivnostiZaUpis = new List<Aktivnost>();

            aktivnostiZaUpis.AddRange(new Aktivnost[] { new Aktivnost("spavanje", 1),
                new Aktivnost("ležanje", 1.2),
                new Aktivnost("mirno sedenje", 1.2),
                new Aktivnost("hodanje sa teretom", 3.75),
                new Aktivnost("mirno stajanje", 1.4),
                new Aktivnost("hodanje, lagana šetnja", 2.45),
                new Aktivnost("brzo hodanje uzbrdo", 6.6),
                new Aktivnost("hodanje uzbrdo sa teretom", 6),
                new Aktivnost("čišćenje", 3.2),
                new Aktivnost("pranje suđa", 1.7),
                new Aktivnost("peglanje", 1.4),
                new Aktivnost("kuvanje", 1.8),
                new Aktivnost("kancelarijski rad", 1.6),
                new Aktivnost("rad na računaru", 2),
                new Aktivnost("stolarstvo", 3.5),
                new Aktivnost("cepanje drva", 4.1),
                new Aktivnost("ples", 4.8),
                new Aktivnost("lagana vožnja bicikla", 4.8),
                new Aktivnost("plivanje", 5.5),
                new Aktivnost("boks", 5.4)
                });

            foreach (var a in aktivnostiZaUpis)
            {
                a.adminID = null;
                a.admin = null;
            }

            var kolekcijaA = await UcitajKolekciju<Aktivnost>(aktivnosti);
            var t = 0;

            var pocetak = DateTime.Now;
            await kolekcijaA.InsertManyAsync(aktivnostiZaUpis);
            var kraj = DateTime.Now;
            t = t + (kraj - pocetak).Milliseconds;

            Log.Information(Environment.NewLine +  "Aktivnosti su upisane za " + t + " milisekundi." 
                         + Environment.NewLine + "Prosečno vreme upisa jedne aktivnosti iznosi "
                         + Math.Round((double)t / 20, 2) + " milisekundi." + Environment.NewLine);
        }


        public async Task PromenaPrvihAktivnosti()
        {
            var sve = await UcitajListu<Aktivnost>(aktivnosti);
            var zaPromenuNaziva = sve.Take(10);
            var t = 0;
            var kolekcijaA = await UcitajKolekciju<Aktivnost>(aktivnosti);

            foreach (var a in sve) 
            {
                if(zaPromenuNaziva.Contains(a))
                     a.Naziv += "- promenjen naziv";
                else a.NivoTezine = Math.Round((a.NivoTezine * 1.2), 2);

                var pocetak = DateTime.Now;
                await kolekcijaA.ReplaceOneAsync(Builders<Aktivnost>.Filter.Eq("Id", a.Id), a);
                var kraj = DateTime.Now;
                t = t + (kraj - pocetak).Milliseconds;
            }

            Log.Information(Environment.NewLine + "Aktivnosti su promenjene za " + t + " milisekundi."
                         + Environment.NewLine + "Prosečno vreme promene jedne aktivnosti iznosi "
                         + Math.Round((double)t / 20, 2) + " milisekundi." + Environment.NewLine);
        }

        public async Task BrisanjePrvihAktivnosti()
        {
            var aktivnostiZaBrisanje = await UcitajListu<Aktivnost>(aktivnosti);
           // var kolekcijaA = await UcitajKolekciju<Aktivnost>(aktivnosti);
           // var filter = Builders<Aktivnost>.Filter.Where(a => aktivnostiZaBrisanje.Contains(a));
            
            var pocetak = DateTime.Now;
            await db.DropCollectionAsync(aktivnosti);
          //  await ObrisiListu<Aktivnost>(aktivnosti, aktivnostiZaBrisanje);
            var kraj = DateTime.Now;
            var t = (kraj - pocetak).Milliseconds;

            Log.Information(Environment.NewLine + "Aktivnosti su obrisane za " + t + " milisekundi."
                         + Environment.NewLine + "Prosečno vreme brisanja jedne aktivnosti iznosi "
                         + Math.Round((double)t / 20, 2) + " milisekundi." + Environment.NewLine);
        }




        public async Task Popuni()
        {
            var db = (new MongoClient("mongodb://localhost:27017")).GetDatabase("brojac_db");
            var kolekcije = db.ListCollectionNames().ToList();

            await RegistracijaAdmina();
            await RegistracijaPrvihKorisnika();
            await UpisPrvihAktivnosti();
            await UpisPrvihNamirnica();
        }


        public async Task Promeni() 
        {
            var db = (new MongoClient("mongodb://localhost:27017")).GetDatabase("brojac_db");
            var kolekcije = db.ListCollectionNames().ToList();

            if (kolekcije.Contains("AdminiAktivnosti") && kolekcije.Contains("AdminiNamirnica"))
                await PromenaAdmina();

            if (kolekcije.Contains(korisnici))
                await PromenaPrvihKorisnika();

            if (kolekcije.Contains(aktivnosti))
                await PromenaPrvihAktivnosti();

            if (kolekcije.Contains(namirnice))
                await PromenaPrvihNamirnica();
        }

        public async Task Obrisi() 
        {
            var db = (new MongoClient("mongodb://localhost:27017")).GetDatabase("brojac_db");
            var kolekcije = db.ListCollectionNames().ToList();

            if (kolekcije.Contains(korisnici))
                await BrisanjePrvihKorisnika();

            if (kolekcije.Contains(aktivnosti))
                await BrisanjePrvihAktivnosti();

            if (kolekcije.Contains(namirnice))
                await BrisanjePrvihNamirnica();
        }


        public async void Testiraj() 
        {
            await Popuni();
            await Promeni();
            await Obrisi();
        }


    }

}



 