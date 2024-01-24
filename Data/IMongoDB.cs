namespace API.Data
{
    public interface IMongoDB
    {

        public Task<IMongoCollection<T>> UcitajKolekciju<T>( string kolekcija);
        public Task<List<T>> UcitajListu<T>(string kolekcija);
        public Task<T> NadjiPoIDu<T>(string kolekcija, string id);
        public Task<List<T>> NadjiSlicne<T>(string kolekicja, string polje, string vrednost);
        public Task<List<T>> NadjiPoUslovu<T>(string kolekicja, string polje, string vrednost);
        public Task<List<T>> NadjiPoUslovima<T>(string kolekcija, List<string>polja, List<string>vrednosti);

        public Task<T> NadjiPoveznik<T>(string kolekcija, string polje1, string polje2, string id1, string id2);
        public Task<List<T>> NadjiPoUslovuIzListe<T>(ICollection<T> lista, string polje, string vrednost);
        public Task Ubaci<T>(string kolekicja, T element);
        public Task<T> Zameni<T>(string kolekcija, string id, T element);
        public Task<T> PromeniPoveznik<T>(string kolekcija, string polje1, string polje2, string id1, string id2, T novi);
        public Task ObrisiVeze<T> (string kolekcija, string id, string polje);
        public Task ObrisiPoveznik<T>(string kolekcija, string polje1, string polje2, string id1, string id2);
        public Task Obrisi<T>(string kolekcija, string id);
        public Task ObrisiListu<T>(string kolekcija, ICollection<T> lista);
        public Task RegistracijaAdmina();
        public Task RegistracijaPrvihKorisnika();
        public Task<Namirnica> UpisPrveNamirnice();
        public Task UpisPrvihNamirnica();
        public Task<Aktivnost> UpisPrveAktivnosti();
        public Task UpisPrvihAktivnosti();

        public Task Popuni();
        public Task Promeni();
        public Task Obrisi();

    }
}