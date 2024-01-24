using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AdministratorController : ControllerBase
    {
        public INalog inalog;
        public AdministratorController(INalog nalog)
        {
            inalog = nalog;
        }

        [HttpPost("prijavaAA/{ime}/{sifra}")]
        public async Task<ActionResult> PrijavaAA(string ime, string sifra)
        {
            var prijava = await inalog.PrijavaAdminaAA(ime, sifra);
            if (prijava.Contains("Uneta"))
                return BadRequest(prijava);
            else if (prijava.Contains("postoji"))
                return NotFound(prijava);
            else
                return Ok(prijava);
        }

        [HttpPost("prijavaAN/{ime}/{sifra}")]
        public async Task<ActionResult> PrijavaAN(string ime, string sifra)
        {
            var prijava = await inalog.PrijavaAdminaAN(ime, sifra);
            if (prijava.Contains("Uneta"))
                return BadRequest(prijava);
            else if (prijava.Contains("postoji"))
                return NotFound(prijava);
            else
                return Ok(prijava);
        }

        [HttpGet("adminiNamirnica"), Authorize]
        public async Task<ActionResult<ICollection<AdministratorNamirnica>>> AdminiNamirnica()
        {
            var admini = await inalog.AdministratorNamirnica();
            if (admini == null)
                return NotFound("Nema administratora namirnica.");

            return Ok(admini);
        }

        [HttpGet("adminiAktivnosti"), Authorize]
        public async Task<ActionResult<ICollection<AdministratorAktivnosti>>> AdminiAktivnosti()
        {
            var admini = await inalog.AdministratorAktivnosti();
            if (admini == null)
                return NotFound("Nema administratora aktivnosti.");

            return Ok(admini);
        }



        [HttpGet("prijavljen"), Authorize(Roles = "AdministratorAktivnosti,AdministratorNamirnica")]
        public async Task<ActionResult<Nalog>> Prijavljen()
        {
            string[] prijavljen = inalog.Prijavljen().Split(',');
            if ((prijavljen[1].Trim()).Equals(UlogaNaloga.AdministratorNamirnica.ToString()))
                return Ok(await inalog.AdministratorNamirnica());
            else 
                return Ok(await inalog.AdministratorAktivnosti());
        }

        [HttpGet("uloga")]
        public async Task<ActionResult<string> >Uloga() 
        {
            string uloga = await inalog.PrijavljenUloga();
            if (uloga == null)
                return NotFound("Nijedan administrator nije prijavljen.");

            return Ok(uloga);
        }


        [HttpGet("dodateAktivnosti"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult<ICollection<Aktivnost>>> DodateAktivnosti() 
        {
            var aktivnosti = await inalog.DodateAktivnosti();
            if (aktivnosti == null)
                return NotFound("Nisu dodate nove aktivnosti.");

            return Ok(aktivnosti);
        }

        [HttpGet("dodateNamirnice"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<ICollection<Aktivnost>>> DodateNamirnice()
        {
            var namirnice = await inalog.DodateNamirnice();
            if(namirnice == null)
                return NotFound("Nisu dodate nove namirnice.");

            return Ok(namirnice);
        }

     /*   [HttpPost("registracija/{ime}/{unetaSifra}/{uloga}")]
        public async Task<ActionResult<string>> Registracija(string ime, string unetaSifra, UlogaNaloga uloga)
        {
            string rezutat = await inalog.RegistracijaAdmina(ime, unetaSifra, uloga);

            if (rezutat.Contains("registrovan"))
                return Ok(rezutat);
            else
                return BadRequest(rezutat);
        } */

        [HttpPut("promeniImeAdmina/{novoIme}"), Authorize(Roles = "AdministratorAktivnosti,AdministratorNamirnica")]
        public async Task<ActionResult<Nalog>> PromeniImeAdmina(string novoIme)
        {
            return Ok(await inalog.PromeniImeAdmina(novoIme));
        }

        [HttpPut("promeniSifruAdmina/{novaSifra}"), Authorize(Roles ="AdministratorAktivnosti,AdministratorNamirnica")]
        public async Task<ActionResult<Nalog>> PromeniSifruAdmina(string novaSifra)
        {
            return Ok(await inalog.PromeniSifruAdmina(novaSifra));
        }





    }
}
