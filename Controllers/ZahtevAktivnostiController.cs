namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ZahtevAktivnostiController : ControllerBase
    {
        public IZahtevAktivnosti izahtev;
        public IAktivnost iaktivnost;
        public ZahtevAktivnostiController(IZahtevAktivnosti zahtev, IAktivnost aktivnost)
        {
            izahtev = zahtev;
            iaktivnost = aktivnost;
        }


        [HttpGet("poslatiZahtevi"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<ZahtevAktivnosti>>> PoslatiZahtevi()
        {
            var zahtevi = await izahtev.PoslatiZahtevi();
            if (zahtevi.Count == 0)
                return NotFound("Nema poslatih zahteva.");

            return Ok(zahtevi);
        }


        [HttpGet("nadjiPoslatZahtev/{zahtevID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevAktivnosti>> NadjiPoslatZahtev(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPoslatZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            return Ok(zahtev);
        }


        [HttpGet("nadjiPoslateZahteve/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevAktivnosti>> NadjiPoslateZahteve(string naziv)
        {
            var zahtevi = await izahtev.NadjiPoslateZahteve(naziv);
            if (zahtevi.Count == 0)
                return NotFound("Ne postoji poslat zahtev za " + naziv + ".");

            return Ok(zahtevi);
        }


        [HttpGet("primljeniZahtevi"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult<ICollection<ZahtevAktivnosti>>> PrimljeniZahtevi()
        {
            var zahtevi = await izahtev.PrimljeniZahtevi();
            if (zahtevi.Count == 0)
                return NotFound("Nema primljenih zahteva.");

            return Ok(zahtevi);
        }


        [HttpGet("noviPrimljeniZahtevi"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult<ICollection<ZahtevAktivnosti>>> NoviPrimljeniZahtevi() 
        {
            var zahtevi = await izahtev.NoviPrimljeniZahtevi();
            if (zahtevi.Count == 0)
                return NotFound("Nema novih primljenih zahteva.");

            return Ok(zahtevi);
        }


        [HttpGet("stariPrimljeniZahtevi"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult<ICollection<ZahtevAktivnosti>>> ZakljuceniPrimljeniZahtevi() 
        {
            var zahtevi = await izahtev.ZakljuceniPrimljeniZahtevi();
            if (zahtevi.Count == 0)
                return NotFound("Nema obrađenih primljenih zahteva.");

            return Ok(zahtevi);
        }

        [HttpGet("nadjiPrimljenZahtev/{zahtevID}"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult<ZahtevAktivnosti>> NadjiPrimljenZahtev(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            return Ok(zahtev);
        }


        [HttpGet("nadjiPrimljeneZahteve/{naziv}"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult<ZahtevAktivnosti>> NadjiPrimljeneZahteve(string naziv)
        {
            var zahtevi = await izahtev.NadjiPrimljeneZahteve(naziv);
            if (zahtevi.Count == 0)
                return NotFound("Ne postoji primljen zahtev za " + naziv + ".");

            return Ok(zahtevi);
        }


        [HttpGet("rezultatPoslatogZahteva/{zahtevID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<RezultatZahteva>> RezultatPoslatogZahteva(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPoslatZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");


            var rezultat = await izahtev.RezultatZahteva(zahtev);
            if (rezultat == null)
                return NotFound("Ne postoji rezultat za traženi zahtev.");

            return Ok(rezultat);
        }


        [HttpGet("rezultatPrimljenogZahteva/{zahtevID}"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult<RezultatZahteva>> RezultatPrimljenogZahteva(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

  
            var rezultat = await izahtev.RezultatZahteva(zahtev);
            if (rezultat == null)
                return NotFound("Ne postoji rezultat za traženi zahtev sa ID-em.");

            return Ok(rezultat);
        }


        [HttpGet("rezultatiZahteva"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<RezultatZahteva>>> RezultatiPoslatihZahteva()
        {
            if ((await izahtev.PoslatiZahtevi()).Count == 0)
                return NotFound("Nema poslatih zahteva.");

            var rezultati = await izahtev.RezultatiZahteva();
            if (rezultati.Count == 0)
                return NotFound("Nema rezultata poslatih zahteva.");

            return Ok(rezultati);
        }

        [HttpGet("prijavaPoslednjeg"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<bool>> PrijavaPoslednjeg()
        {
            return Ok(await izahtev.PrijavaPoslednjeg());
        }

        [HttpPost("posaljiZahtev/{naziv}/{prijava}/{napomena}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevAktivnosti>> PosaljiZahtev(string naziv, bool prijava, string napomena) 
        {
            var aktivnosti = await iaktivnost.AktivnostiPoNazivu(naziv);
            if (aktivnosti.Count > 0) 
                return Ok("Već postoji aktivnost pod nazivom " + naziv + ", ili sličnim.");

            var slanje = await izahtev.PosaljiZahtev(naziv, prijava, napomena);
            if (slanje == null)
                return BadRequest("Već je poslat zahtev za " + naziv + ", ne može ponovo.");

            return Ok(slanje);         
        }




        [HttpPut("prihvatiZahtev/{zahtevID}"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult<ZahtevAktivnosti>> PrihvatiZahtev(string zahtevID) 
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if(zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            var prihvati = await izahtev.PrihvatiZahtev(zahtev); 
            if (prihvati == null)
                return BadRequest("Ovaj zahtev je već prihvaćen ili obrađen.");

            return Ok(zahtev);
        }


        [HttpPut("ispuniZahtev/{zahtevID}/{naziv}"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult<ZahtevAktivnosti>> IspuniZahtev(string zahtevID, string naziv)
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            var aktivnosti = await iaktivnost.AktivnostiPoNazivu(naziv);
            if (aktivnosti.Count == 0)
                return NotFound("Nije uneta aktivnost " + naziv + ".");

            var ispuni = await izahtev.IspuniZahtev(zahtev, aktivnosti);
            if (ispuni == null)
                return BadRequest("Zahtev je već obrađen.");

            return Ok(zahtev);
        }


        [HttpPut("odbijZahtev/{zahtevID}"), Authorize(Roles = "AdministratorAktivnosti")]
        public async Task<ActionResult<ZahtevAktivnosti>> OdbijZahtev(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            var odbij = await izahtev.OdbijZahtev(zahtev);
            if (odbij == null)
                return BadRequest("Zahtev je već obrađen");

            return Ok(zahtev);
        }


        [HttpDelete("povuciZahtev/{zahtevID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevAktivnosti>> PovuciZahtev(string zahtevID) 
        {
            var povlacenje = await izahtev.PovuciZahtev(zahtevID);
            if (povlacenje.Contains("postoji"))
                return NotFound(povlacenje);

            if (povlacenje.Contains("prihvatio"))
                return BadRequest(povlacenje);

            return Ok(povlacenje);
        }



    }
}
