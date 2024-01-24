namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AktivnostController : ControllerBase
    {
        private IAktivnost iaktivnost;
        public AktivnostController(IAktivnost aktivnost)
        {
            iaktivnost = aktivnost;        
        }

        [HttpGet("aktivnosti"), Authorize(Roles ="Korisnik, AdministratorAktivnosti")]
        public async Task<ActionResult<ICollection<Aktivnost>>> Aktivnosti()
        {
            var aktivnosti = await iaktivnost.Aktivnosti();
            if (aktivnosti.Count == 0)
                return NotFound("Nema aktivnosti.");

            return Ok(aktivnosti);
        }

        [HttpGet("PoIDu/{id}"), Authorize(Roles = "Korisnik, AdministratorAktivnosti")]
        public async Task<ActionResult<Aktivnost>> AktivnostPoIDu(string id) 
        {
            var aktivnost = await iaktivnost.AktivnostPoIDu(id);
            if (aktivnost == null)
                return NotFound("Ne postoji tražena aktivnost.");

            return Ok(aktivnost);
        }

        [HttpGet("PoNazivu/{naziv}"), Authorize(Roles = "Korisnik, AdministratorAktivnosti")]
        public async Task<ActionResult<ICollection<Aktivnost>>> AktivnostPoNazivu(string naziv) 
        {
            ICollection<Aktivnost> aktivnosti = await iaktivnost.AktivnostiPoNazivu(naziv);
            if (aktivnosti.Count == 0)
                return NotFound("Ne postoji aktivnost " + naziv + ".");

            return Ok(aktivnosti);
        }

        [HttpGet("potrosnja/{aktivnostID}/{minuti}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> Potrosnja(string aktivnostID, int minuti) 
        {
            var aktivnost = await iaktivnost.AktivnostPoIDu(aktivnostID);
            if (aktivnost == null)
                return NotFound("Ne postoji tražena aktivnost.");

            return Ok(await iaktivnost.Potrosnja(aktivnost, minuti));
        }
        

        [HttpPost("dodaj/{naziv}/{nt}"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult<Aktivnost>> DodajAktivnost(string naziv, double nt) 
        {
            var aktivnosti = await iaktivnost.AktivnostiPoNazivu(naziv);
            if (aktivnosti.Count > 0)
                return BadRequest("Već postoji aktivnost pod nazivom " + naziv + ".");

            return Ok(await iaktivnost.DodajAktivnost(naziv, nt));
        }

        [HttpPut("promeniNaziv/{id}/{naziv}"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult<Aktivnost>> PromeniNazivAktivnosti(string id, string naziv)
        {
            var aktivnost = await iaktivnost.AktivnostPoIDu(id);
            if (aktivnost == null)
                return NotFound("Ne postoji tražena aktivnost.");

            return Ok(await iaktivnost.PromeniNazivAktivnosti(aktivnost, naziv));
        }

        [HttpPut("promeniNT/{id}/{nt}"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult<Aktivnost>> PromeniTezinuAktivnosti(string id, double nt)
        {
            var aktivnost = await iaktivnost.AktivnostPoIDu(id);
            if (aktivnost == null)
                return NotFound("Ne postoji tražena aktivnost.");

            return Ok(await iaktivnost.PromeniTezinuAktivnosti(aktivnost, nt));
        }

        [HttpDelete("obrisi/{id}"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult> ObrisiAktivnost(string id) 
        {
            string brisanje = await iaktivnost.ObrisiAktivnost(id);
            if (brisanje.Contains("postoji"))
                return NotFound(brisanje);

            return Ok(brisanje);
        }







    }
}
