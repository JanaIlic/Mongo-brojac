namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class StanjeController : ControllerBase
    {
        public IStanje istanje;
        public StanjeController(IStanje stanje)
        {
            istanje = stanje;
        }

        [HttpGet("stanja"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Stanje>>> Stanja() 
        {
            var stanja = await istanje.Stanja();
            if(stanja.Count == 0)
                return NotFound("Još uvek nije upisano stanje." + Environment.NewLine + 
                    "Za upis izaberi opciju 'novo stanje' u donjem levom delu ekrana. ");

            return Ok(stanja);
        }


        [HttpGet("stanjePoIDu/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Stanje>> NadjiStanjePoIDu(string id) 
        {
            var stanje = await istanje.StanjePoIDu(id);
            if (stanje == null)
                return NotFound("Ne postoji traženo stanje.");

            return Ok(stanje);
        }

        [HttpGet("prvoStanje"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Stanje>> PrvoStanje()
        {
            var stanje = await istanje.PrvoStanje();
            if (stanje == null)
                return NotFound("Još uvek nije upisano stanje." + Environment.NewLine + 
                    "Za upis izaberi opciju 'novo stanje' u donjem levom delu ekrana. ");

            return Ok(stanje);
        }


        [HttpGet("stanjePoDatumu/{godina}/{mesec}/{dan}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Stanje>> NadjiStanjePoDatumu(int godina, int mesec, int dan) 
        {
            var provera =  istanje.ProveriDatum(godina, mesec, dan);
            if (!provera.Equals(string.Empty))
                return BadRequest(provera);

            var stanje = await  istanje.StanjePoDatumu(new DateTime());
            if (stanje == null)
                return NotFound("Ne postoji stanje sa datumom " + dan + "." + mesec + "." + godina + ".");

            return Ok(stanje);
        }


        [HttpGet("aktuelnoStanje"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Stanje>> NadjiAktuelnoStanje() 
        {
            var stanje = await istanje.AktuelnoStanje();
            if (stanje == null)
                return NotFound("Još uvek nije upisano stanje. " + Environment.NewLine + 
                    "Za upis izaberi opciju 'novo stanje' u donjem levom delu eklrana. ");

            return Ok(stanje);
        }

        [HttpGet("prikazBmi/{stanjeID}"), Authorize(Roles ="Korisnik")]
        public async Task<ActionResult<string>> PrikazBmi(string stanjeID) 
        {
            var stanje = await istanje.StanjePoIDu(stanjeID);
            if (stanje == null)
                return NotFound("Ne postoji traženo stanje.");

            return Ok(await istanje.PrikazBmi(stanje));
        }

        [HttpGet("ponudiBrDana/{mesec}")]
        public  ActionResult<int> PonudiBrDana(int mesec) 
        {
            return Ok( istanje.PonudiBrDana(mesec));
        }


        [HttpPost("upisiStanje/{visina}/{tezina}/{nt}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Stanje>> UpisiStanje(double visina, double tezina, double nt) 
        {
            Stanje stanje = await istanje.UpisiStanje(visina, tezina, nt);
            return Ok(stanje);
        }

        [HttpPut("zadajCilj/{cilj}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ZadajCilj(double cilj) 
        {
            return Ok(await istanje.ZadajCiljnuTezinu(cilj));
        }

        [HttpPut("zadajVreme/{vreme}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ZadajVreme(int vreme)
        {
            return Ok(await istanje.ZadajVreme(vreme));
        }

        [HttpPut("promeniVisinu/{visina}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Stanje>> PromeniVisinu(double visina) 
        {
            var stanje = await istanje.StanjePoDatumu(DateTime.Today);
            if (stanje == null)
                return NotFound("Ne postoji stanje, mora se uneti.");

            if (visina != null)
                stanje = await istanje.PromeniVisinu((double)visina);

            return Ok(stanje);
        }

	
        [HttpPut("promeniTezinu/{tezina}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Stanje>> PromeniTezinu(double tezina) 
        {
            var stanje = await istanje.StanjePoDatumu(DateTime.Today);
            if (stanje == null)
                return NotFound("Ne postoji stanje, mora se uneti.");

            if (tezina != null)
                stanje = await istanje.PromeniTezinu((double)tezina);

            return Ok(stanje);
        }

        [HttpDelete("obrisiStanje"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ObrisiStanje() 
        {
            var brisanje = await istanje.ObrisiStanje();
            if (brisanje.Contains("upisano"))
                return NotFound(brisanje);

            return Ok(brisanje);
        }


        [HttpGet("ponudiPeriode"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<List<string>>> Periodi()
        {
            return Ok(await istanje.PonudiPeriode());
        }

        [HttpGet("parsiraj/{period}"), Authorize(Roles = "Korisnik")]
        public ActionResult<int> Parsiraj(string period) 
        {
            return istanje.ParsirajPeriod(period);
        }

    }
}
