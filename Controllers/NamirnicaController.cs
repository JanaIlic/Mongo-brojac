namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class NamirnicaController : ControllerBase
    {
        INamirnica inamirnica;
        public NamirnicaController(INamirnica namirnica) 
        {
            inamirnica = namirnica;
        }

        [HttpGet("namirnice"), Authorize(Roles = "AdministratorNamirnica, Korisnik")]
        public async Task<ActionResult<ICollection<Namirnica>>> Namirnice() 
        {
            var namirnice = await inamirnica.Namirnice();
            if(namirnice.Count == 0)
                return NotFound("Nema namirnica.");

            return Ok(namirnice);
        }

        [HttpGet("filter/{vrsta}/{tip}/{mast}/{brasno}"), Authorize(Roles = "AdministratorNamirnica, Korisnik")]
        public async Task<ActionResult<ICollection<Namirnica>>> Filter(int vrsta, int tip, int mast,int brasno)
        {
            var filter = await inamirnica.Filtriraj(vrsta, tip, mast, brasno);
            if (filter.Count == 0)
               return NotFound("Nema namirnica iz izabranih kategorija.");

            return Ok(filter);
        }



        [HttpGet("nadjiPoNazivu/{naziv}"), Authorize(Roles = "AdministratorNamirnica, Korisnik")]
        public async Task<ActionResult<ICollection<Namirnica>>> NadjiNamirnicePoNazivu(string naziv) 
        {
            var namirnice = await inamirnica.NadjiNamirnicePoNazivu(naziv);
            if (namirnice.Count == 0)
                return NotFound("Ne postoji namirnica " + naziv + ".");

            return Ok(namirnice);
        }

        [HttpGet("nadjiPoIDu/{id}"), Authorize(Roles = "AdministratorNamirnica, Korisnik")]
        public async Task<ActionResult<Namirnica>> NadjiNamirnicu(string id) 
        {
            var namirnica = await inamirnica.NadjiNamirnicu(id);
            if(namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            return Ok(namirnica);
        }

        [HttpGet("skalirajNamirnicu/{id}/{masa}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Namirnica>> SkalirajNamirnicu(string id, double masa) 
        {
            System.Diagnostics.Trace.WriteLine("ID: " + id);
            System.Diagnostics.Trace.WriteLine("MASA: " + masa);

            var namirnica = await inamirnica.NadjiNamirnicu(id);
            if (namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            return Ok(await inamirnica.SkalirajNamirnicu(namirnica, masa));
        }


        [HttpPost("dodajNamirnicu/{naziv}/{vrsta}/{tip}/{brasno}/{dm}/{ev}/{p}/{uh}/{m}/{pm}/{opis}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<Namirnica>> DodajNamirnicu(string naziv, VrstaNamirnice vrsta, TipObrade tip, KolicinaBrasna brasno,
            KolicinaMasti dm, double ev, double p, double uh, double m, double pm, string opis) 
        {
            if (await inamirnica.NadjiNamirnicuPoNazivu(naziv) != null)
                return BadRequest("Već postoji namirnica " + naziv + ".");

            if (p + uh + m > 100)
                return BadRequest("Pogrešno je uneta masa makronutrijenata."); 

            Namirnica namirnica = new Namirnica(naziv, vrsta, tip, brasno, dm, ev, p, uh, m, pm, opis);
            await inamirnica.DodajNamirnicu(namirnica);

            return Ok(namirnica);
        }

        [HttpPut("promeniNaziv/{id}/{naziv}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<Namirnica>> PromeniNaziv(string id, string naziv) 
        {
            var namirnica = await inamirnica.NadjiNamirnicu(id);
            if (namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            await inamirnica.PromeniNaziv(namirnica, naziv);
            return Ok(namirnica);
        }


        [HttpPut("promeniVrstu/{id}/{vrsta}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<Namirnica>> PromeniVrstu(string id, VrstaNamirnice vrsta)
        {
            var namirnica = await inamirnica.NadjiNamirnicu(id);
            if (namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            await inamirnica.PromeniVrstu(namirnica, vrsta);
            return Ok(namirnica);
        }

        [HttpPut("promeniTipObrade/{id}/{tip}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<Namirnica>> PromeniTipObrade(string id, TipObrade tip)
        {
            var namirnica = await inamirnica.NadjiNamirnicu(id);
            if (namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            await inamirnica.PromeniTipObrade(namirnica, tip);
            return Ok(namirnica);
        }


        [HttpPut("promeniBrasno/{id}/{brasno}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<Namirnica>> PromeniKolicinuDodatogBrasna(string id, KolicinaBrasna brasno)
        {
            var namirnica = await inamirnica.NadjiNamirnicu(id);
            if (namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            await inamirnica.PromeniKolicinuBrasna(namirnica, brasno);
            return Ok(namirnica);
        }


        [HttpPut("promeniKolicinuDodateMasti/{id}/{mast}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<Namirnica>> PromeniKolicnuDodateMasti(string id, KolicinaMasti mast)
        {
            var namirnica = await inamirnica.NadjiNamirnicu(id);
            if (namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            await inamirnica.PromeniKolicinuMasti(namirnica, mast);
            return Ok(namirnica);
        }

        [HttpPut("promeniEnergetskuVrednost/{id}/{energetskaVrednost}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<Namirnica>> PromeniEnergetskuVrednost(string id, double energetskaVrednost)
        {
            var namirnica = await inamirnica.NadjiNamirnicu(id);
            if (namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            await inamirnica.PromeniEnergetskuVrednost(namirnica, energetskaVrednost);
            return Ok(namirnica);
        }

        [HttpPut("promeniProtein/{id}/{protein}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<Namirnica>> PromeniProtein(string id, double protein)
        {
            var namirnica = await inamirnica.NadjiNamirnicu(id);
            if (namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            await inamirnica.PromeniProtein(namirnica, protein);
            return Ok(namirnica);
        }

        [HttpPut("promeniUH/{id}/{uh}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<Namirnica>> PromeniUH(string id, double uh)
        {
            var namirnica = await inamirnica.NadjiNamirnicu(id);
            if (namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            await inamirnica.PromeniUgljeneHidrate(namirnica, uh);
            return Ok(namirnica);
        }


        [HttpPut("promeniMast/{id}/{mast}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<Namirnica>> PromeniMast(string id, double mast)
        {
            var namirnica = await inamirnica.NadjiNamirnicu(id);
            if (namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            await inamirnica.PromeniMast(namirnica, mast);
            return Ok(namirnica);
        }

        [HttpPut("promeniKoeficijent/{id}/{promena}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<Namirnica>> PromeniKoeficijent(string id, double promena)
        {
            var namirnica = await inamirnica.NadjiNamirnicu(id);
            if (namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            await inamirnica.PromeniKoeficijentPromeneMase(namirnica,promena);
            return Ok(namirnica);
        }

        [HttpPut("promeniOpis/{id}/{promena}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<Namirnica>> PromeniOpis(string id, string promena)
        {
            var namirnica = await inamirnica.NadjiNamirnicu(id);
            if (namirnica == null)
                return NotFound("Ne postoji tražena namirnica.");

            await inamirnica.PromeniOpis(namirnica, promena);
            return Ok(namirnica);
        }


        [HttpDelete("obrisi/{id}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult> ObrisiNamirnicu(string id) 
        {
            var brisanje = await inamirnica.ObrisiNamirnicu(id);
            if (brisanje.Contains("postoji"))
                return NotFound(brisanje);

           return Ok(brisanje); 
        }


    }
}
