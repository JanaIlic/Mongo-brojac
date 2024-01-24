namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class DanController : ControllerBase
    {
        public IDan idan;

        public DanController(IDan dan)
        {
            idan = dan;
        }


        [HttpGet("dani"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Dan>>> Dani()
        {
            var dani = await idan.Dani();
            if(dani.Count == 0)
                return NotFound("Nema unetih dana.");

            return Ok(dani);
        }


        [HttpGet("danas"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Dan>> Danas() 
        {
            var danas = await idan.Danas();
            if (danas == null)
                return NotFound("Nije unet današnji dan.");

            return Ok(danas);
        }


        [HttpGet("danPoIDu/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Dan>> NadjiDanPoIDu(string id) 
        {
            var dan = await idan.NadjiDan(id);
            if (dan == null)
                return NotFound("Nije unet traženi dan.");

            return Ok(dan);
        }


        [HttpGet("danPoDatumu/{godina}/{mesec}/{dan}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Dan>> NadjiDanPoDatumu(int godina, int mesec, int dan)
        {
            var provera = idan.ProveriDatum(godina, mesec, dan);
            if (!provera.Equals(string.Empty) )
                return BadRequest(provera);

            var d = await idan.NadjiDanPoDatumu(godina, mesec, dan);
            if (d == null)
                return NotFound("Nije unet dan " + dan + "." + mesec + "." + godina + ".");
            
            return Ok(d);
        }

        [HttpGet("izvestaji"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Izvestaj>>> Izvestaji() 
        {
            var izvestaji = await idan.Izvestaji();
            if (izvestaji.Count == 0 || izvestaji == null)
                return NotFound("Nema upisanih izveštaja.");

            return Ok(izvestaji);

        }


        [HttpGet("prikazIzvestaja"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<string>>> PrikazIzvestaja()
        {
            var izvestaji = await idan.Izvestaji();
            if (izvestaji.Count == 0 || izvestaji == null)
                return NotFound("Nema upisanih izveštaja.");

            var prikaz = await idan.PrikazIzvestaja();
            if(prikaz == null)
                return NotFound("Nema upisanih izveštaja.");

            return Ok(prikaz);

        }



        [HttpGet("nadjiIzvestaj/{danID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Izvestaj>> NadjiIzvestaj(string danID) 
        {
            var izvestaj = await idan.NadjiIzvestaj(danID);
            if (izvestaj == null)
                return NotFound("Ne postoji izveštaj za traženi dan, tada je bilo isključeno generisanje izveštaja.");

            return Ok(izvestaj);
        }

        [HttpGet("danasnjiIzvestaj"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Izvestaj>> DanasnjiIzvestaj() 
        {
            var izvestaj = await idan.DanasnjiIzvestaj();
            if (izvestaj == null)
                return NotFound("Nije generisan danasnji izvestaj.");

            return Ok(izvestaj);
        }


        [HttpGet("jeLiDanas/{dID}"), Authorize(Roles ="Korisnik")]
        public async Task<ActionResult<bool>> JeLiDanas(string dID) 
        {
            return Ok(await idan.JeLiDanas(dID));
        }

        [HttpPost("dodajDan"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> DodajDan()
        {
            string dodavanje = await idan.DodajDan();
            if(dodavanje.Contains("unet"))
                return BadRequest(dodavanje);

            return Ok(dodavanje);
        }


        [HttpPut("rezultat"), Authorize(Roles = "Korisnik") ]
        public async Task<ActionResult<Dan>> UpisiRezultat()
        {
            Dan dan = await idan.UpisiRezultat();
            if (dan == null)
                return NotFound("Dan nije upisan.");

            return Ok(dan);
        } 


       [HttpPut("iskljuciIzvestaje"), Authorize(Roles ="Korisnik")]
        public async Task<ActionResult<Dan>> IskljuciIzvestaje() 
        {
            var iskljucenje = await idan.Iskljuci();
            if (iskljucenje == null)
                return BadRequest("Došlo je do greške.");

            return Ok(iskljucenje);
        }

        [HttpPut("ukljuciIzvestaje"), Authorize(Roles ="Korisnik")]
        public async Task<ActionResult<Dan>> UkljuciIzvestaje()
        {
            var ukljucenje = await idan.Ukljuci();
            if (ukljucenje == null)
                return BadRequest("Došlo je do greške.");

            return Ok(ukljucenje);
        }


        [HttpDelete("obrisiDan/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult> ObrisiDan(string id)
        {
            string brisanje = await idan.ObrisiDan(id);
            if (brisanje.Contains("postoji"))
                return BadRequest(brisanje);

            return Ok(brisanje);
        }





    }
}
