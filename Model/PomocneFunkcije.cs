using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace API.Model
{
    public class PomocneFunkcije
    {
        public readonly IHttpContextAccessor accessor;

        public PomocneFunkcije() { }
        public PomocneFunkcije(IHttpContextAccessor hca)
        {
            accessor = hca;
        }

        public string PrijavljenID()
        {
            string id = String.Empty;
            if (accessor.HttpContext != null)
                id = accessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            return id;
        }

        public string PrijavljenUloga()
        {
            string uloga = string.Empty;
            if (accessor.HttpContext != null)
                uloga = accessor.HttpContext.User?.FindFirstValue(ClaimTypes.Role);

            return uloga;
        }

        public string Prijavljen()
        {
            string rezultat = string.Empty;
            if (accessor.HttpContext != null)
            {
                rezultat += "Prijavljen je " + accessor.HttpContext.User?.FindFirstValue(ClaimTypes.Name) + ", " +
                    accessor.HttpContext.User?.FindFirstValue(ClaimTypes.Role) + ", " +
                    "sa ID-em " + accessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            return rezultat;
        }

        public string ProveriDatum(int godina, int mesec, int dan)
        {
            string rezultat = string.Empty;
            List<int> mesec31 = new List<int> { 1, 3, 5, 8, 10, 12 };

            if ((godina < 2022) || (godina > DateTime.Now.Year))
                rezultat = "Pogrešno uneta godina.";
            else if (mesec > 12)
                rezultat = "Nemoguće. Godina ima 12 meseci.";
            else if ((mesec == 2) && (dan > 28))
                rezultat = "Nemoguće. Februar ima 28 dana.";
            else if ((mesec31.Contains(mesec)) && (dan > 31))
                rezultat = "Nemoguće. Ne može biti više od 31 dan u unetom mesecu.";
            else if (!(mesec31.Contains(mesec)) && (dan > 30))
                rezultat = "Nemoguće. Ne može biti više od 30 dana u unetom mesecu.";

            if (rezultat != String.Empty)
                rezultat += " Unesi ponovo.";

            return rezultat;
        }

        public string ProveriDatumRodjenja(int godina, int mesec, int dan)
        {
            string rezultat = string.Empty;
            List<int> mesec31 = new List<int> { 1, 3, 5, 8, 10, 12 };

            if ((godina < 1900) || (godina > DateTime.Now.Year))
                rezultat = "Nemoguć broj godina.";
            else if (mesec > 12)
                rezultat = "Nemoguće. Godina ima 12 meseci.";
            else if ((mesec == 2) && (dan > 28))
                rezultat = "Nemoguće. Februar ima 28 dana.";
            else if ((mesec31.Contains(mesec)) && (dan > 31))
                rezultat = "Nemoguće. Ne može biti više od 31 dan u unetom mesecu.";
            else if (!(mesec31.Contains(mesec)) && (dan > 30))
                rezultat = "Nemoguće. Ne može biti više od 30 dana u unetom mesecu.";

            if (rezultat != String.Empty)
                rezultat += " Unesi ponovo.";

            return rezultat;
        }


        public int PonudiBrDana(int mesec)
        {
            int br = 0;
            int[] br31 = { 1, 3, 5, 8, 10, 12 };
            int[] br30 = { 4, 6, 7, 9, 11 };

            if (br31.Contains(mesec))
                br = 31;
            else if (br30.Contains(mesec))
                br = 30;
            else
                br = 28;

            return br;
        }

        public void SifraHash(string sifra, out byte[] hash, out byte[] kljuc)
        {
            using (var hmac = new HMACSHA512())
            {
                kljuc = hmac.Key;
                hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(sifra));
            }
        }

        public bool ProveriSifru(string sifra, byte[] hash, byte[] kljuc)
        {
            using (var hmac = new HMACSHA512(kljuc))
            {
                var izracunajHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(sifra));
                return izracunajHash.SequenceEqual(hash);
            }
        }


        public string DatumVremeToString(DateTime dt)
        {
            string ispis = dt.Day.ToString() + ".";
            ispis += dt.Month.ToString() + ".";
            ispis += dt.Year.ToString() + ". u ";
            ispis += dt.Hour.ToString() + ":";
            ispis += dt.Minute.ToString();

            return ispis;
        }

        public string DatumToString(DateTime dt)
        {
            string ispis = dt.Day.ToString() + ".";
            ispis += dt.Month.ToString() + ".";
            ispis += dt.Year.ToString() + ".";

            return ispis;
        }


        public async void UbaciProfilnuSliku(string slika, Korisnik korisnik)
        {
            FileInfo[] files = new FileInfo[20];
            int i = 0;

            foreach (var drive in DriveInfo.GetDrives())
            {
                DirectoryInfo dir = drive.RootDirectory;
                System.Diagnostics.Trace.WriteLine("DRIVE: " + dir);
                foreach (var file in dir.GetFiles(slika, new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true }))
                {
                    files[i] = file;
                    i++;
                }
            }


            if (files[0] != null)
            {
                string putanja = @"..\..\Brojač Kalorija\Brojac\src\assets\";
                korisnik.Slika = korisnik.Ime + Path.GetExtension(slika);


                if (File.Exists(putanja + korisnik.Slika))
                {
                    File.Delete(putanja + korisnik.Slika);
                    File.Copy(files[0].FullName, putanja + korisnik.Slika);
                }
                else
                    File.Copy(files[0].FullName, putanja + korisnik.Slika);

            }
        }


        public void ObrisiSLiku(string putanja)
        {

            if (File.Exists(putanja + ".jpg"))
                File.Delete(putanja + ".jpg");

            else if (File.Exists(putanja + ".jpeg"))
                File.Delete(putanja + ".jpeg");

            else if (File.Exists(putanja + ".png"))
                File.Delete(putanja + ".png");
        }




        public async void UbaciSlikuObjave(string slika, Objava objava)
        {
            FileInfo[] files = new FileInfo[20];
            int i = 0;

            foreach (var drive in DriveInfo.GetDrives())
            {
                DirectoryInfo dir = drive.RootDirectory;
                foreach (var file in dir.GetFiles(slika, new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true }))
                {
                    files[i] = file;
                    i++;
                }
            }


            if (files[0] != null)
            {
                string putanja = @"..\..\Brojač Kalorija\Brojac\src\assets\slike-objava\";
                objava.Slika = objava.Id + Path.GetExtension(slika);

                if (File.Exists(putanja + objava.Slika) && Path.GetExtension(slika) == Path.GetExtension(objava.Slika))
                {
                    File.Copy(files[0].FullName, putanja + objava.Slika);
                }
                else if (File.Exists(putanja + objava.Slika) && Path.GetExtension(slika) != Path.GetExtension(objava.Slika))
                {
                    File.Copy(files[0].FullName, putanja + objava.Slika);
                    File.Delete(putanja + objava.Slika);
                }
                else
                    File.Copy(files[0].FullName, putanja + objava.Slika);


            }


        }


        public string ParsirajUnos(string unos)
        {
            var upis = unos.Trim();

            if (upis.Equals("**"))
                upis = String.Empty;
            else if (upis.Contains("***"))
            {
                var redovi = upis.Split("***");
                if (redovi.Length == 0)
                    foreach (var r in redovi)
                        if (r.Length > 60)
                            r.Insert(60, "***");

                upis = upis.Replace("***", Environment.NewLine);
            }
            else if (upis.Contains("upitnik"))
                upis = upis.Replace("upitnik", "?");

            return upis;
        }




        public BMI Kategorija(double visina, double tezina)
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



        public double IdealnaKilaza(PolKorisnika pol, double visina)
        {
            double ik = 0;

            if (pol == PolKorisnika.Muski)
                ik = (visina - 100) - (visina - 150) / 4;
            else
                ik = (visina - 100) - (visina - 150) / 2.5;

            return Math.Round(ik, 1);
        }

        public double IzracunajBMR(Korisnik korisnik, double visina, double tezina)
        {
            // var korisnik = await mdb.NadjiPoIDu<Korisnik>("Korisnici", funkcije.PrijavljenID());
            int godine = DateTime.Today.Year - korisnik.DatumRodjenja.Year;
            double bmr = 0;

            var k = Kategorija(visina, tezina);


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


            return Math.Ceiling(bmr);
        }



    }
}
