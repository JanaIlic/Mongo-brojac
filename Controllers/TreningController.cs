namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class TreningController : ControllerBase
    {
        public ITrening itrening;
        public IAktivnost iaktivnost;
        public IStanje istanje;
        public IDan idan;
        public TreningController(ITrening trening, IAktivnost aktivnost, IStanje stanje, IDan dan)
        {
            itrening = trening;
            iaktivnost = aktivnost;
            istanje = stanje;
            idan = dan;
        }


        [HttpGet("treninzi"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Trening>>> Treninzi()
        {
            var treninzi = await itrening.Treninzi();
            if (treninzi == null)
                return NotFound("Nema treninga.");

            return Ok(treninzi);
        }


        [HttpGet("treningPoIDu/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Trening>> NadjiTreningPoIDu(string id)
        {
            var trening = await itrening.TreningPoIDu(id);
            if (trening == null)
                return NotFound("Ne postoji traženi trening.");

            return Ok(trening);
        }


        [HttpGet("treningPoNazivu/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Trening>> NadjiTreningPoNazivu(string naziv)
        {
            var trening = await itrening.TreningPoNazivu(naziv);
            if (trening == null)
                return NotFound("Ne postoji trening pod nazivom " + naziv + ".");

            return Ok(trening);
        }


        [HttpGet("treninziPoNazivu/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Trening>>> TreninziPoNazivu(string naziv)
        {
            var treninzi = await itrening.TreninziPoNazivu(naziv);
            if (treninzi.Count == 0)
                return NotFound("Ne postoji trening pod nazivom sličnim " + naziv + ".");

            return Ok(treninzi);
        }

        [HttpGet("vremeAktivnosti/{treningID}/{aktivnostID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<double>> VremeAktivnosti(string treningID, string aktivnostID)
        {
            var trening = await itrening.TreningPoIDu(treningID);
            if (trening == null)
                return NotFound("Ne postoji traženi trening.");

            var aktivnost = await iaktivnost.AktivnostPoIDu(aktivnostID);
            if (aktivnost == null)
                return NotFound("Ne postoji tražena aktivnost.");

            return Ok(await itrening.VremeAktivnosti(trening, aktivnost));
        }


        [HttpGet("opisiTrening/{treningID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> Opisi(string treningID)
        {
            var trening = await itrening.TreningPoIDu(treningID);
            if (trening == null)
                return NotFound("Ne postoji traženi trening.");

            return Ok(await itrening.OpisiTrening(trening));
        }

        [HttpGet("aktivnostiTreninga/{treningID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Aktivnost[]>> AktivnostiTreninga(string treningID)
        {
            var trening = await itrening.TreningPoIDu(treningID);
            if (trening == null)
                return NotFound("Ne postoji traženi trening.");

            return Ok(await itrening.AktivnostiTreninga(trening));
        }

        [HttpGet("vremenaAktivnosti/{treningID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Aktivnost[]>> VremenaAktivnosti(string treningID)
        {
            var trening = await itrening.TreningPoIDu(treningID);
            if (trening == null)
                return NotFound("Ne postoji traženi trening.");

            return Ok(await itrening.VremenaAktivnosti(trening));
        }

        [HttpGet("potrosnjePriAktivnostima/{treningID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<double[]>> PotrosnjePriAktivnostima(string treningID)
        {
            var trening = await itrening.TreningPoIDu(treningID);
            if (trening == null)
                return NotFound("Ne postoji traženi trening.");

            return Ok(await itrening.PotrosnjePriAktivnostima(trening));
        }

        [HttpGet("danasnjiTreninzi"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Trening>>> DanasnjiTreninzi() 
        {
            var danasnji = await itrening.DanasnjiTreninzi();
            if (danasnji == null)
                return NotFound("Danas nije unet nijedan trening.");

            return Ok(danasnji);
        }

        [HttpGet("treninziDana/{danID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Trening>>> TreninziDana(string danID) 
        {
            var dan = await idan.NadjiDan(danID);
            if (dan == null)
                return NotFound("Nema upisanih podataka za uneti dan.");

            var treninzi = await itrening.TreninziDana(dan);
            if (treninzi == null)
                return NotFound("Nije unet nijedan trening u toku odabranog dana.");

            return Ok(treninzi);
        }


        [HttpPost("dodajTrening/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Trening>> DodajTrening(string naziv)
        {
            if (await itrening.TreningPoNazivu(naziv) != null)
                return NotFound("Već postoji trening pod nazivom " + naziv + ".");

            return Ok(await itrening.DodajTrening(naziv));
        }


        [HttpPost("objaviTrening/{treningID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Objava>> ObjaviTrening(string treningID)
        {
            var trening = await itrening.TreningPoIDu(treningID);
            if (trening == null)
                return NotFound("Ne postoji traženi trening.");

            return Ok(await itrening.ObjaviTrening(trening));
        }


        [HttpPut("promeniNaziv/{treningID}/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Trening>> PromeniNaziv(string treningID, string naziv) 
        {
            var trening = await itrening.TreningPoIDu(treningID);
            if (trening == null)
                return NotFound("Ne postoji traženi trening.");

            if (naziv.Equals(String.IsNullOrEmpty))
                return NotFound("Nije unet novi naziv.");

            return await itrening.PromeniNaziv(trening, naziv);
        }


        [HttpPut("dodajAktivnostTreningu/{treningID}/{aktivnostID}/{vreme}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Trening>> DodajAktivnostTreningu(string treningID, string aktivnostID, int vreme)
        {
            var trening = await itrening.TreningPoIDu(treningID);
            if (trening == null)
                return NotFound("Ne postoji traženi trening.");

            var aktivnost = await iaktivnost.AktivnostPoIDu(aktivnostID);
            if (aktivnost == null)
                return NotFound("Ne postoji tražena aktivnost.");

            var dodavanje = await itrening.DodajAktivnostTreningu(trening, aktivnost, vreme);
            if (dodavanje == null)
                return BadRequest("Trening " + trening.Naziv + " već sadrži " + aktivnost.Naziv + ".");

            return Ok(dodavanje);
        }


        [HttpPut("promeniVremeAktivnosti/{treningID}/{aktivnostID}/{vreme}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Trening>> PromeniVremeAktivnosti(string treningID, string aktivnostID, int vreme)
        {
            var trening = await itrening.TreningPoIDu(treningID);
            if (trening == null)
                return NotFound("Ne postoji traženi trening.");

            var aktivnost = await iaktivnost.AktivnostPoIDu(aktivnostID);
            if (aktivnost == null)
                return NotFound("Ne postoji tražena aktivnost.");

            var promena = await itrening.PromeniVremeAktivnosti(trening, aktivnost, vreme);
            if (promena == null)
                return BadRequest("Trening " + trening.Naziv + " ne sadrži " + aktivnost.Naziv + ".");

            return Ok(promena);
        }


        [HttpPut("dodajDnevniTrening/{treningID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Trening>> DodajDnevniTrening(string treningID) 
        {
            var trening = await itrening.TreningPoIDu(treningID);
            if (trening == null)
                return NotFound("Ne postoji traženi trening.");

            if (await itrening.TreningVecDodatDanas(trening))
                return BadRequest("Danas je već dodat trening " + trening.Naziv + ".");

           var dodavanje =  await itrening.DodajDnevniTrening(trening);
           if (dodavanje == null)
                return NotFound("Dan nije upisan.");

            return Ok(dodavanje);
        }


        [HttpPut("obrisiAktivnostIzTreninga/{treningID}/{aktivnostID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Trening>> ObrisiAktivnostIzTreninga(string treningID, string aktivnostID)
        {
            var trening = await itrening.TreningPoIDu(treningID);
            if (trening == null)
                return NotFound("Ne postoji traženi trening.");

            var aktivnost = await iaktivnost.AktivnostPoIDu(aktivnostID);
            if (aktivnost == null)
                return NotFound("Ne postoji tražena aktivnost.");

            var brisanje = await itrening.ObrisiAktivnostIzTreninga(trening, aktivnost);
            if (brisanje == null)
                return BadRequest("Trening " + trening.Naziv + " ne sadrži " + aktivnost.Naziv + "."); 

            return Ok(brisanje);
        }


        [HttpDelete("obrisiTrening/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ObrisiTrening(string id)
        {
            var brisanje = await itrening.ObrisiTrening(id);
            if (brisanje.Contains("postoji"))
                NotFound(brisanje);

            return Ok(brisanje);
        }

        [HttpDelete("obrisiDanasnjiTrening/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ObrisiDanasnjiTrening(string id)
        {
            var brisanje = await itrening.ObrisiDanasnjiTrening(id);
            if (brisanje.Contains("postoji"))
                NotFound(brisanje);

            return Ok(brisanje);
        }



    }
}
