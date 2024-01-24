namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ZahtevNamirniceController : ControllerBase
    {
        public IZahtevNamirnice izahtev;
        public INamirnica inamirnica;
        public ZahtevNamirniceController(IZahtevNamirnice zahtev, INamirnica namirnica)
        {
            izahtev = zahtev;
            inamirnica = namirnica;
        }


        [HttpGet("poslatiZahtevi"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<ZahtevNamirnice>>> PoslatiZahtevi()
        {
            var zahtevi = await izahtev.PoslatiZahtevi();
            if (zahtevi.Count == 0)
                return NotFound("Nema poslatih zahteva.");

            return Ok(zahtevi);
        }


        [HttpGet("nadjiPoslatZahtev/{zahtevID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevNamirnice>> NadjiPoslatZahtev(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPoslatZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            return Ok(zahtev);
        }


        [HttpGet("nadjiPoslateZahteve/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevNamirnice>> NadjiPoslateZahteve(string naziv)
        {
            var zahtevi = await izahtev.NadjiPoslateZahteve(naziv);
            if (zahtevi.Count == 0)
                return NotFound("Ne postoji poslat zahtev za " + naziv + ".");

            return Ok(zahtevi);
        }


        [HttpGet("primljeniZahtevi"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<ICollection<ZahtevNamirnice>>> PrimljeniZahtevi()
        {
            var zahtevi = await izahtev.PrimljeniZahtevi();
            if (zahtevi.Count == 0)
                return NotFound("Nema primljenih zahteva.");

            return Ok(zahtevi);
        }


        [HttpGet("noviPrimljeniZahtevi"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<ICollection<ZahtevNamirnice>>> NoviPrimljeniZahtevi()
        {
            var zahtevi = await izahtev.NoviPrimljeniZahtevi();
            if (zahtevi.Count == 0)
                return NotFound("Nema novih primljenih zahteva.");

            return Ok(zahtevi);
        }


        [HttpGet("stariPrimljeniZahtevi"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<ICollection<ZahtevNamirnice>>> ZakljuceniPrimljeniZahtevi()
        {
            var zahtevi = await izahtev.ZakljuceniPrimljeniZahtevi();
            if (zahtevi.Count == 0)
                return NotFound("Nema obrađenih primljenih zahteva.");

            return Ok(zahtevi);
        }


        [HttpGet("nadjiPrimljenZahtev/{zahtevID}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<ZahtevNamirnice>> NadjiPrimljenZahtev(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            return Ok(zahtev);
        }


        [HttpGet("nadjiPrimljeneZahteve/{naziv}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<ZahtevNamirnice>> NadjiPrimljeneZahteve(string naziv)
        {
            var zahtevi = await izahtev.NadjiPrimljeneZahteve(naziv);
            if (zahtevi.Count == 0)
                return NotFound("Ne postoji primljen zahtev za " + naziv + ".");

            return Ok(zahtevi);
        }


        [HttpGet("rezultatPrimljenogZahteva/{zahtevID}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<RezultatZahteva>> RezultatPrimljenogZahteva(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            var rezultat = await izahtev.RezultatZahteva(zahtev);
            if (rezultat == null)
                return NotFound("Ne postoji rezultat za traženi zahtev.");

            return Ok(rezultat);
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


        [HttpPost("posaljiZahtev/{naziv}/{prijava}/{napomena}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevNamirnice>> PosaljiZahtev(string naziv, bool prijava, string napomena)
        {
            var namirnice = await inamirnica.NadjiNamirnicePoNazivu(naziv);
            if (namirnice.Count > 0)
                return Ok("Već postoji namirnica sa nazivom " + naziv + ", ili sličnim.");

            var slanje = await izahtev.PosaljiZahtev(naziv, prijava, napomena);
            if (slanje == null)
                return BadRequest("Već je poslat zahtev za " + naziv + ", ne može ponovo.");

            return Ok(slanje);
        }




        [HttpPut("prihvatiZahtev/{zahtevID}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<ZahtevNamirnice>> PrihvatiZahtev(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            var prihvati = await izahtev.PrihvatiZahtev(zahtev);
            if (prihvati == null)
                return BadRequest("Zahtev je već prihvaćen ili obrađen.");

            return Ok(zahtev);
        } 


        [HttpPut("ispuniZahtev/{zahtevID}/{naziv}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<ZahtevNamirnice>> IspuniZahtev(string zahtevID, string naziv)
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            var namirnice = await inamirnica.NadjiNamirnicePoNazivu(naziv);
            if (namirnice.Count == 0)
                return NotFound("Nije uneta namirnica " + naziv + ".");

            var ispuni = await izahtev.IspuniZahtev(zahtev, namirnice);
            if (ispuni == null)
                return BadRequest("Zahtev je već prihvaćen ili obrađen.");

            return Ok(zahtev);
        }

        [HttpGet("prijavaPoslednjeg"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<bool>> PrijavaPoslednjeg()
        {
            return Ok(await izahtev.PrijavaPoslednjeg());
        }

        [HttpPut("odbijZahtev/{zahtevID}"), Authorize(Roles = "AdministratorNamirnica")]
        public async Task<ActionResult<ZahtevNamirnice>> OdbijZahtev(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            var odbij = await izahtev.OdbijZahtev(zahtev);
            if (odbij == null)
                return BadRequest("Zahtev je već obrađen.");

            return Ok(zahtev);
        }



        [HttpDelete("povuciZahtev/{zahtevID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevNamirnice>> PovuciZahtev(string zahtevID)
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
