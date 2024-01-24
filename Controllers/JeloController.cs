namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class JeloController : ControllerBase
    {
        public IJelo ijelo;
        public INamirnica inamirnica;

        public JeloController(IJelo jelo, INamirnica namirnica)
        {
            ijelo = jelo;
            inamirnica = namirnica;
        }


        [HttpGet("jela"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Jelo>>> Jela()
        {
            var jela = await ijelo.Jela();
            if (jela.Count == 0)
                return NotFound("Nema unetih jela.");

            return Ok(jela);
        }


        [HttpGet("jeloPoIDu/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Jelo>> NadjiJeloPoIDu(string id)
        {
            var jelo = await ijelo.JeloPoIDu(id);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            return Ok(jelo);
        }


        [HttpGet("jeloPoNazivu/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Jelo>> NadjiJeloPoNazivu(string naziv)
        {
            var jelo = await ijelo.JeloPoNazivu(naziv);
            if (jelo == null)
                return NotFound("Ne postoji jelo " + naziv + ".");

            return Ok(jelo);
        }


        [HttpGet("jelaPoNazivu/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Jelo>>> NadjiJelaSlicnogNaziva(string naziv)
        {
            var jela = await ijelo.JelaPoNazivu(naziv);
            if (jela.Count == 0)
                return NotFound("Ne postoji jelo pod nazivom " + naziv + ", ili sličnim.");

            return Ok(jela);
        }

        [HttpGet("skalirajJelo/{jeloID}/{masa}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Jelo>> SkalirajJelo(string jeloID, double masa) 
        {
            var jelo = await ijelo.JeloPoIDu(jeloID);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            return Ok(await ijelo.SkalirajJelo(jelo, masa));
        }

        [HttpGet("namirniceJela/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Namirnica>>> NamirniceJela(string id) 
        {
            var jelo = await ijelo.JeloPoIDu(id);
            if(jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            return Ok(await ijelo.NamirniceJela(jelo));
        }

        [HttpGet("masaNamirnice/{jeloID}/{namirnicaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<double>> MasaNamirnice(string jeloID, string namirnicaID) 
        {
            var jelo = await ijelo.JeloPoIDu(jeloID);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            var namirnica = await inamirnica.NadjiNamirnicu(namirnicaID);
            if(namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            return Ok(await ijelo.MasaNamirniceUJelu(jelo, namirnica));
        }

        [HttpGet("maseNamirnica/{jeloID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<double>> MaseNamirnica(string jeloID)
        {
            var jelo = await ijelo.JeloPoIDu(jeloID);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

 

            return Ok(await ijelo.MaseNamirnicaUJelu(jelo));
        }


        [HttpPost("dodajNovoJelo/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Jelo>> DodajNovoJelo(string naziv) 
        {
            if (await ijelo.JeloPoNazivu(naziv) != null)
                return BadRequest("Već postoji jelo pod nazivom " + naziv + ".");

            return Ok(await ijelo.DodajNovoJelo(naziv));
        }


        [HttpPost("objaviJelo/{jeloID}"), Authorize(Roles ="Korisnik")]
        public async Task<ActionResult<Objava>> ObjaviJelo(string jeloID) 
        {
            var jelo = await ijelo.JeloPoIDu(jeloID);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            return Ok(await ijelo.ObjaviJelo(jelo));            
        }


        [HttpPut("promeniNazivJela/{id}/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Jelo>> PromeniNazivJela(string id, string naziv) 
        {
            var jelo = await ijelo.JeloPoIDu(id);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            if (await ijelo.JeloPoNazivu(naziv) != null)
                return BadRequest("Već postoji jelo pod nazivom " + naziv + ".");

            return Ok(await ijelo.PromeniNazivJela(jelo, naziv));
        }


        [HttpPut("dodajNamirnicuJelu/{jeloID}/{namirnicaID}/{masa}/{pre}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Jelo>> DodajNamirnicuJelu(string jeloID, string namirnicaID, double masa, bool pre) 
        {
            var jelo = await ijelo.JeloPoIDu(jeloID);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            var namirnica = await inamirnica.NadjiNamirnicu(namirnicaID);
            if (namirnica == null)
                return NotFound("Ne postoji tražena naminrica.");

            return Ok(await ijelo.DodajJeluNamirnicu(jelo, namirnica, masa, pre));
        }


        [HttpPut("promeniMasuNamirnice/{jeloID}/{namirnicaID}/{masa}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Jelo>> PromeniMasuNamirnice(string jeloID, string namirnicaID, double masa) 
        {
            var jelo = await ijelo.JeloPoIDu(jeloID);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            var namirnica = await inamirnica.NadjiNamirnicu(namirnicaID);
            if (namirnica == null)
                return NotFound("Ne postoji tražena naminrica.");

            var promena = await ijelo.PromeniMasuNamirnice(jelo, namirnica, masa);
            if (promena == null)
                return NotFound("Jelo " + jelo.Naziv + " ne sadrži namirnicu " + namirnica.Naziv + ".");

            return Ok(jelo);
        }


        [HttpPut("ispisiRecept/{jeloID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> IspisiRecept(string jeloID) 
        {
            var jelo = await ijelo.JeloPoIDu(jeloID);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            await ijelo.NapisiRecept(jelo);

            return Ok(jelo.Recept);
        }

        [HttpDelete("obrisiNamirnicuIzJela/{jeloID}/{namirnicaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Jelo>> ObrisiNamirnicuIzJela(string jeloID, string namirnicaID)
        {
            var jelo = await ijelo.JeloPoIDu(jeloID);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            var namirnica = await inamirnica.NadjiNamirnicu(namirnicaID);
            if (namirnica == null)
                return NotFound("Ne postoji tražena naminrica.");

            var brisanje = await ijelo.ObrisiNamirnicuIzJela(jelo, namirnica);
            if (brisanje == null)
                return NotFound("Jelo " + jelo.Naziv + " ne sadrži namirnicu " + namirnica.Naziv + ".");

            return Ok(jelo);
        }


        [HttpDelete("obrisiJelo/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ObrisiJelo(string id)
        {
            string brisanje = await ijelo.ObrisiJelo(id);
            if (brisanje.Contains("postoji"))
                return NotFound(brisanje);

            return Ok(brisanje);
        }




    }
}
