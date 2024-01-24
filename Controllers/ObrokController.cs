namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ObrokController : ControllerBase
    {
        public IObrok iobrok;
        public IJelo ijelo;
        public IDan idan;
        public ObrokController(IObrok obrok, IJelo jelo, IDan dan)
        {
            iobrok = obrok;
            ijelo = jelo;
            idan = dan;
        }

        [HttpGet("obroci"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Obrok>>> Obroci()
        {
            var obroci = await iobrok.Obroci();
            if (obroci.Count == 0)
                return NotFound("Nema unetih obroka.");

            return Ok(obroci);
        }

        [HttpGet("obrokPoIDu/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Obrok>> NadjiObrokPoIDu(string id)
        {
            var obrok = await iobrok.ObrokPoIDu(id);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            return Ok(obrok);
        }

        [HttpGet("obrokPoNazivu/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Obrok>> NadjiObrokPoNazivu(string naziv)
        {
            var obrok = await iobrok.ObrokPoNazivu(naziv);
            if (obrok == null)
                return NotFound("Ne postoji obrok  " + naziv + ".");

            return Ok(obrok);
        }

        [HttpGet("obrociPoNazivu/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Obrok>>> NadjiObrokePoNazivu(string naziv)
        {
            var obroci = await iobrok.ObrociPoNazivu(naziv);
            if (obroci.Count == 0)
                return NotFound("Ne postoji obrok  " + naziv + ".");

            return Ok(obroci);
        }


        [HttpGet("obrociDana/{danID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Obrok>>> NadjiObrokeDana(string danID)
        {
            var dan = await idan.NadjiDan(danID);
            if (dan == null)
                return NotFound("Nema upisanih podataka za uneti dan.");

            var obroci = await iobrok.ObrociDana(dan);
            if (obroci.Count == 0)
                return NotFound("Nije unet nijedan obrok u toku odabranog dana.");

            return Ok(obroci);
        }

        [HttpGet("jeloObroka/{obrokID}/{jeloID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Jelo>>> JeloObroka(string obrokID, string jeloID)
        {
            var obrok = await iobrok.ObrokPoIDu(obrokID);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            var jelo = await ijelo.JeloPoIDu(jeloID);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            return Ok(await iobrok.JeloObroka(obrok, jelo));
        }

        [HttpGet("jelaObroka/{obrokID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Jelo>>> JelaObroka(string obrokID)
        {
            var obrok = await iobrok.ObrokPoIDu(obrokID);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            var jela = await iobrok.JelaObroka(obrok);
            if (jela.Count == 0)
                return NotFound("U obrok još nisu dodata jela.");

            return Ok(jela);
        }

        [HttpGet("maseJela/{obrokID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<double>>> MaseJela(string obrokID)
        {
            var obrok = await iobrok.ObrokPoIDu(obrokID);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            return Ok(await iobrok.MaseJela(obrok));
        }

        [HttpGet("evJela/{obrokID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<double>>> EvJela(string obrokID)
        {
            var obrok = await iobrok.ObrokPoIDu(obrokID);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            return Ok(await iobrok.EvJela(obrok));
        }

        [HttpGet("obrokDodatDanas/{obrokID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<bool>> ObrokDodatDanas(string obrokID)
        {
            var obrok = await iobrok.ObrokPoIDu(obrokID);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            return Ok(await iobrok.ObrokVecDodatDanas(obrok));
        }

        [HttpGet("opisObroka/{obrokID}"), Authorize(Roles ="Korisnik")]
        public async Task<ActionResult<string>> OpisObroka(string obrokID) 
        {
            var obrok = await iobrok.ObrokPoIDu(obrokID);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            return Ok(await iobrok.OpisiObrok(obrok));
        }

        [HttpGet("danasnjiObroci"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Obrok>>> DanasnjiObroci()
        {
            var danasnji = await iobrok.DanasnjiObroci();
            if (danasnji == null)
                return NotFound("Danas nije unet nijedan obrok.");

            return Ok(danasnji);
        }

        [HttpPost("dodajObrok/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Obrok>> DodajObrok(string naziv)
        {
            if (await iobrok.ObrokPoNazivu(naziv) != null)
                return BadRequest("Već postoji obrok pod nazivom " + naziv + ".");

            return Ok(await iobrok.DodajObrok(naziv));
        }

        [HttpPut("dodajObrokDanas/{obrokID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Obrok>> DodajObrokDanas(string obrokID)
        {
            var obrok = await iobrok.ObrokPoIDu(obrokID);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            if (await iobrok.ObrokVecDodatDanas(obrok))
                return BadRequest("Danas je već unet obrok " + obrok.Naziv + ".");

            var dodavanje = await iobrok.DodajObrokDanas(obrok);

            return Ok(dodavanje);
        }

        [HttpPost("objaviObrok/{obrokID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Objava>> ObjaviObrok(string obrokID)
        {
            var obrok = await iobrok.ObrokPoIDu(obrokID);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            return Ok(await iobrok.ObjaviObrok(obrok));
        }

        [HttpPut("promeniNaziv/{obrokID}/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Obrok>> PromeniNaziv(string obrokID, string naziv) 
        {
            var obrok = await iobrok.ObrokPoIDu(obrokID);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            if (await iobrok.ObrokPoNazivu(naziv) != null)
                return Ok("Već postoji obrok pod nazivom " + naziv + ".");

             return Ok(await iobrok.PromeniNaziv(obrok, naziv));
        }

        [HttpPut("promeniMasu/{obrokID}/{masa}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Obrok>> PromeniMasuObroka(string obrokID, double masa)
        {
            var obrok = await iobrok.ObrokPoIDu(obrokID);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            return Ok(await iobrok.PromeniMasuObroka(obrok, masa));
        }


        [HttpPut("dodajJeloObroku/{obrokID}/{jeloID}/{masa}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Obrok>> DodajJeloObroku(string obrokID, string jeloID, double masa) 
        {
            var obrok = await iobrok.ObrokPoIDu(obrokID);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            var jelo = await ijelo.JeloPoIDu(jeloID);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            var dodatak = await iobrok.DodajJeloObroku(obrok, jelo, masa);
            if(dodatak == null)
                return BadRequest("Obrok " + obrok.Naziv + " već sadrži jelo " + jelo.Naziv + ".");

            return Ok(dodatak);
        }

        [HttpPut("promeniMasuJela/{obrokID}/{jeloID}/{masa}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Obrok>> PromeniMasuJela(string obrokID, string jeloID, double masa)
        {
            var obrok = await iobrok.ObrokPoIDu(obrokID);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            var jelo = await ijelo.JeloPoIDu(jeloID);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            var promena = await iobrok.PromeniMasuJela(obrok, jelo, masa);
            if (promena == null)
                return NotFound("Obrok " + obrok.Naziv + " ne sadrži jelo " + jelo.Naziv + ".");

            return Ok(promena);
        }


        [HttpDelete("obrisiJeloIzObroka/{obrokID}/{jeloID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Obrok>> ObrisiJeloIzObroka(string obrokID, string jeloID) 
        {
            var obrok = await iobrok.ObrokPoIDu(obrokID);
            if (obrok == null)
                return NotFound("Ne postoji traženi obrok.");

            var jelo = await ijelo.JeloPoIDu(jeloID);
            if (jelo == null)
                return NotFound("Ne postoji traženo jelo.");

            var brisanje = await iobrok.ObrisiJeloIzObroka(obrok, jelo);
            if (brisanje == null)
                return NotFound("Obrok " + obrok.Naziv + " ne sadrži jelo " + jelo.Naziv + ".");

            return Ok(brisanje);
        }


        [HttpDelete("obrisiObrok/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ObrisiObrok(string id) 
        {
            string brisanje = await iobrok.ObrisiObrok(id);
            if (brisanje.Contains("postoji"))
                return NotFound(brisanje);

            return Ok(brisanje);
        }


        [HttpDelete("obrisiDanasnjiObrok/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ObrisiDanasnjiObrok(string id)
        {
            string brisanje = await iobrok.ObrisiObrokDanas(id);
            if (brisanje.Contains("postoji"))
                return NotFound(brisanje);

            return Ok(brisanje);
        }

    }
}
