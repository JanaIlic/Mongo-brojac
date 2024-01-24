namespace API.Servisi.Interfejsi
{
    public interface IDan
    {
        public Task<ICollection<Dan>> Dani();
        public Task<Dan> Danas();

        public Task<bool> JeLiDanas(string danID);
        public Task<Dan> NadjiDan(string id);
        public Task<Dan> NadjiDanPoDatumu(int godina, int mesec, int dan);
        public Task<string> DodajDan();
        public Task<Dan> UpisiRezultat();
        public Task<string> ObrisiDan(string id);
        public Task<Dan> Iskljuci();
        public Task<Dan> Ukljuci();
        public string ProveriDatum(int godina, int mesec, int dan);
        public Task<ICollection<Izvestaj>> Izvestaji();
        public Task<ICollection<string>> PrikazIzvestaja();
        public Task<Izvestaj> NadjiIzvestaj(string danID);
        public Task<Izvestaj> DanasnjiIzvestaj();
        public Task<Izvestaj> DodajIzvestaj();
    
    }
}
