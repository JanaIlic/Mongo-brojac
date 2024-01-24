namespace API.Servisi
{
    public interface INalog
    {
        public string NapraviToken(Nalog nalog);
        
        public Task<string> Prijava(string ime, string sifra);
        public Task<string> PrijavaAdminaAA(string ime, string sifra);
        public Task<string> PrijavaAdminaAN(string ime, string sifra);
        public string PrijavljenID();
        public string Prijavljen();
        public Task<string> PrijavljenUloga();
        public int PonudiBrDana(int mesec);
        public Task<bool> Pracen(Korisnik k);
        public Task<bool> Pratilac(Korisnik k);
  
        public Task<string> Registracija(string ime, string unetaSifra, int godina, int mesec, int dan, PolKorisnika pol, string slika);
        public Task<ICollection<Korisnik>> Korisnici();
        public Task<AdministratorAktivnosti> AdministratorAktivnosti();
        public Task<AdministratorNamirnica> AdministratorNamirnica();
        public Task<Korisnik> KorisnikPoID(string id);
        public Task<Korisnik> KorisnikPoImenu(string ime);
        public Task<ICollection<Korisnik>> KorisniciPoImenu(string ime);

        public Task<ICollection<Korisnik>> Pratioci();
        public Task<ICollection<Korisnik>> Praceni();

        public Task<Korisnik> PromeniIme(string novoIme);
        public Task<Nalog> PromeniImeAdmina(string novoIme);
        public Task<Korisnik> PromeniSifru(string novaSifra);
        public Task<Nalog> PromeniSifruAdmina(string novaSifra);

        public Task<Korisnik> PromeniSliku(string slika);
        public Task<string> PromeniDatumRodjenja(int godina, int mesec, int dan);
        public Task<String> ObrisiKorisnika();

        public Task<ICollection<Aktivnost>> DodateAktivnosti();
        public Task<ICollection<Namirnica>> DodateNamirnice();

    }
}
