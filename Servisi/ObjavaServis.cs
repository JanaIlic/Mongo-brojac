namespace API.Servisi
{
    public class ObjavaServis : IObjava
    {
        private IMongoDB mdb;
        public PomocneFunkcije funkcije;
        public const string objave = "Objave";
        public ObjavaServis(IMongoDB servis, IHttpContextAccessor hca)
        {
            mdb = servis;
            funkcije = new PomocneFunkcije(hca);
        }


        public async Task<ICollection<Objava>> Objave()
        {
            // var o1 = await mdb.NadjiPoUslovu<Objava>(objave, "autorID", funkcije.PrijavljenID());
            // var o2 = await mdb.NadjiPoUslovuIzListe<Objava>(o1, "glavnaID", null);
            var o2 = await mdb.NadjiPoUslovima<Objava>(objave, new List<string> {"autorID", "glavnaID" },
                new List<string> {funkcije.PrijavljenID(), null });
            return  o2.OrderByDescending(o => o.Vreme).ToList();
        }

        public async Task<ICollection<Objava>> VidljiveObjave()
        {
            var praceni = new List<string>();
            foreach (var pracen in 
            (await mdb.NadjiPoUslovu<DvaKorisnika>("ParoviKorisnika", "pratilac", funkcije.PrijavljenID())).ToList() )
                praceni.Add(pracen.pracenID);

            var objavePracenih = (await mdb.UcitajListu<Objava>(objave)).Where(o => praceni.Contains(o.autorID));
            objavePracenih.ToList().AddRange(await Objave());

            return objavePracenih.OrderByDescending(o => o.Vreme).ToList();
        }

        public async Task<Objava> ObjavaPoIDu(string objavaID)
        {
            var objava = (await Objave()).Where(o=>  o.Id == objavaID).FirstOrDefault();
            if (objava == null)
                return null;

            return objava;
        }

        public async Task<Objava> ObjavaPracenog(string objavaID)
        {
            var vidljive = (await VidljiveObjave()).ToList();
            var prijavljenog = (await Objave()).ToList();

            var o = vidljive.DistinctBy(ob => prijavljenog.Contains(ob));
            var objava = await mdb.NadjiPoIDu<Objava>(objave, objavaID);
            if (o == null || objava == null || !o.Contains(objava) )
                return null;

            return objava;
        }

        public async Task<ICollection<Objava>> SveObjavePracenog(string korisnikID)
        {
            return await mdb.NadjiPoUslovuIzListe((await VidljiveObjave()), "autorID", korisnikID);
        }

        public async Task<ICollection<Objava>> ObjavePoTekstu(string tekst) 
        {
            return (await VidljiveObjave()).Where(o => (o.Tekst.Contains(tekst) || tekst.Contains(o.Tekst))).ToList();
        }


        public async Task<ICollection<Objava>> Komentari(Objava objava) 
        {
            return (await mdb.NadjiPoUslovu<Objava>(objave, "glavnaID", objava.Id )).OrderBy(k => k.Vreme).ToList();
        }


        public async Task<Objava> KomentarPratiocaNaObjavu(string objavaID) 
        {
            var komentar = await mdb.NadjiPoIDu<Objava>(objave, objavaID);
            if (komentar == null)
                return null;

            var glavna = await mdb.NadjiPoIDu<Objava>(objave, komentar.glavnaID);
            if (glavna == null)
                return null;

            if (glavna.autorID == funkcije.PrijavljenID())
                return komentar;

            else return null;
        }

        public async Task<Objava> KomentarPrijavljenog(string objavaID) 
        {
            var komentar = await mdb.NadjiPoIDu<Objava>(objave, objavaID);
            if (komentar == null )
                return null;

            if (komentar.autorID != funkcije.PrijavljenID())
                return null;

            return komentar;
        }


        public async Task<Ocena> OcenaNaObjavu(Objava objava)
        {

            var ocena = (await mdb.NadjiPoUslovima<Ocena>("Ocene", new List<string> {"objavaID", "autorID" }, 
                new List<string> { objava.Id, funkcije.PrijavljenID()} )).FirstOrDefault();
            if (ocena == null)
                return null;

            return ocena;
        }

        public async Task<ICollection<Ocena>> Ocene(Objava objava) 
        {
            return await mdb.NadjiPoUslovu<Ocena>("Ocene", "objavaID", objava.Id);
        }

        public async Task<double> Prosek(Objava objava) 
        {
            double suma = 0;
            var ocene = await Ocene(objava);
            foreach (var o in ocene)
                suma += o.Vrednost;

            if(suma == 0)
                return 0;

            return Math.Round(suma/ocene.Count, 2);
        }

        public async Task<bool> ObjavaPrijavljenogKorisnika(Objava objava) 
        {
            var jeste = false;
            if(objava.autorID == funkcije.PrijavljenID())
                jeste = true;

            return jeste;
        }

        public async Task<ICollection<Korisnik>> AutoriObjava() 
        {
            var objave = await VidljiveObjave();
            List<Korisnik> autori = new List<Korisnik>();
            foreach (var objava in objave)
                autori.Add(await mdb.NadjiPoIDu<Korisnik>("Korisnici", objava.autorID) );

            return autori;
        }


        public async Task<ICollection<Korisnik>> AutoriKomentara(Objava objava) 
        {
            List<Korisnik> autori = new List<Korisnik>();
            foreach (var komentar in await Komentari(objava))
                autori.Add(await mdb.NadjiPoIDu<Korisnik>("Korisnici", komentar.autorID));

            return autori;
        }

        public async Task<ICollection<Korisnik>> AutoriOcena(Objava objava) 
        {
            List<Korisnik> autori = new List<Korisnik>();
            foreach (var ocena in await Ocene(objava))
                autori.Add(await mdb.NadjiPoIDu<Korisnik>("Korisnici", ocena.korisnikID ));

            return autori;
        }

        public async Task<Objava> Objavi(string tekst) 
        {
            Objava objava = new Objava();
            objava.Tekst = funkcije.ParsirajUnos(tekst);
            objava.Vreme = DateTime.Now;
            objava.autorID = funkcije.PrijavljenID();
            objava.autor = await mdb.NadjiPoIDu<Korisnik>("Korisnici", objava.autorID);
            objava.glavnaID = null;

            await mdb.Ubaci<Objava>(objave, objava);
            return objava;
        }


        public async Task<Objava> ObjaviSaSlikom(string tekst, string slika) 
        {
            Objava objava = new Objava();
            objava.Tekst = funkcije.ParsirajUnos(tekst);
            objava.Vreme = DateTime.Now;
            objava.autorID = funkcije.PrijavljenID();
            objava.autor = await mdb.NadjiPoIDu<Korisnik>("Korisnici", objava.autorID);
            objava.glavnaID = null;

            await mdb.Ubaci<Objava>(objave, objava);

            if (!slika.Equals(String.IsNullOrEmpty))
                this.funkcije.UbaciSlikuObjave(slika, objava);
            objava.Slika = objava.Id + Path.GetExtension(slika);

            return await mdb.Zameni<Objava>(objave, objava.Id, objava);
        }

        public async Task<Objava> Komentarisi(Objava glavna, string tekst) 
        {
            Objava komentar = await Objavi(tekst);
            komentar.glavna = glavna;
            komentar.glavnaID = glavna.Id;

            return await mdb.Zameni<Objava>(objave, glavna.Id, glavna);
        }


        public async Task<Objava> Oceni(Objava objava, int vrednost) 
        {
            if (await OcenaNaObjavu(objava) != null) 
                await PromeniOcenu(objava, vrednost);
            
            else{
                Ocena ocena = new Ocena(vrednost, DateTime.Now);
                ocena.objava = objava;
                ocena.objavaID = objava.Id;
                ocena.korisnik = await mdb.NadjiPoIDu<Korisnik>("Korisnici", funkcije.PrijavljenID());
                ocena.korisnikID = ocena.korisnik.Id;
                await mdb.Ubaci<Ocena>("Ocene", ocena);
            }


            return await mdb.Zameni<Objava>(objave, objava.Id, objava);
        }

        public async Task<Objava> PrepraviObjavu(Objava objava, string tekst) 
        {
            objava.Tekst = funkcije.ParsirajUnos(tekst);
            return await mdb.Zameni<Objava>(objave, objava.Id, objava);
        }


        public async Task<Objava> PromeniOcenu(Objava objava, int vrednost)
        {
            var ocena = await OcenaNaObjavu(objava);
            if(ocena == null)
                return null;
            
            ocena.Vrednost = vrednost;
            await mdb.Zameni<Ocena>("Ocene", ocena.Id, ocena);
            return await mdb.Zameni<Objava>(objave, objava.Id, objava);
        }


        public async Task<Objava> PovuciOcenu(Objava objava) 
        {
            var ocena = await OcenaNaObjavu(objava);
            if (ocena == null)
                return null;

            await mdb.Obrisi<Ocena>("Ocene", ocena.Id);
            return await mdb.Zameni<Objava>(objave, objava.Id, objava);
        }

        public async Task<string> ObrisiObjavu(string objavaID) 
        {
            var objava = await ObjavaPoIDu(objavaID);
            if (objava == null)
                return string.Empty;


            if(objava.Slika != String.Empty)
                this.funkcije.ObrisiSLiku(@"..\..\Brojač Kalorija\Brojac\src\assets\slike-objava\" + objavaID);

            var komentari = await Komentari(objava);
            if (komentari.Count > 0)
                foreach (var k in komentari)
                    await mdb.Obrisi<Objava>(objave, k.Id);

            var ocene = await Ocene(objava);
            foreach (var o in ocene)
                await mdb.Obrisi<Ocena>("Ocene", o.Id);

            await mdb.Obrisi<Objava>(objave, objavaID);

            return "Objava je obrisana.";
        }

        public async Task<ICollection<Objava>> ObrisiKomentar(string komentarID) 
        {
            var komentar = await KomentarPratiocaNaObjavu(komentarID);
            if (komentar == null) 
                komentar = await KomentarPrijavljenog(komentarID);

            var objava = await mdb.NadjiPoIDu<Objava>(objave, komentar.glavnaID);
            await mdb.Obrisi<Objava>(objave, komentarID);
            await mdb.Zameni<Objava>(objave, objava.Id, objava);

            return await Komentari(objava);
        }

        public async Task<string> ObrisiObjave() 
        {
            var objave = await Objave();
            if (objave.Count == 0)
                return "Ništa nije objavljeno.";

            foreach (var objava in objave)
                await ObrisiObjavu(objava.Id);


            return "Objave su obrisane.";
        }






    }
}
