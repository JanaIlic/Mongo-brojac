namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ZahtevZaPracenjeController : ControllerBase
    {
        public IZahtevZaPracenje izahtev;
        public INalog inalog;
        public ZahtevZaPracenjeController(IZahtevZaPracenje zahtev, INalog nalog)
        {
            izahtev = zahtev;
            inalog = nalog;
        }


        [HttpGet("poslatiZahtevi"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<ZahtevZaPracenje>>> PoslatiZahtevi()
        {
            var zahtevi = await izahtev.PoslatiZahtevi();
            if (zahtevi.Count == 0)
                return NotFound("Nema poslatih zahteva.");

            return Ok(zahtevi);
        }


        [HttpGet("nadjiPoslatZahtev/{zahtevID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevZaPracenje>> NadjiPoslatZahtev(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPoslatZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            return Ok(zahtev);
        }


        [HttpGet("nadjiPoslateZahteve/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevZaPracenje>> NadjiPoslateZahteve(string naziv)
        {
            var zahtevi = await izahtev.NadjiPoslateZahteve(naziv);
            if (zahtevi.Count == 0)
                return NotFound("Ne postoji zahtev poslat korisniku " + naziv + ".");

            return Ok(zahtevi);
        }


        [HttpGet("primljeniZahtevi"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<ZahtevZaPracenje>>> PrimljeniZahtevi()
        {
            var zahtevi = await izahtev.PrimljeniZahtevi();
            if (zahtevi.Count == 0)
                return NotFound("Nema primljenih zahteva.");

            return Ok(zahtevi);
        }


        [HttpGet("nadjiPrimljenZahtev/{zahtevID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevZaPracenje>> NadjiPrimljenZahtev(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            return Ok(zahtev);
        }


        [HttpGet("nadjiPrimljeneZahteve/{naziv}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevZaPracenje>> NadjiPrimljeneZahteve(string naziv)
        {
            var zahtevi = await izahtev.NadjiPrimljeneZahteve(naziv);
            if (zahtevi.Count == 0)
                return NotFound("Ne postoji primljen zahtev od " + naziv + ".");

            return Ok(zahtevi);
        }



        [HttpGet("rezultatPoslatogZahteva/{zahtevID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<RezultatZahteva>> RezultatPoslatogZahteva(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPoslatZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            var rezultat = await izahtev.RezultatPoslatogZahteva(zahtev);
            if (rezultat == null)
                return NotFound("Ne postoji rezultat traženog zahteva.");

            return Ok(rezultat);
        }

        [HttpGet("rezultatPrimljenogZahteva/{zahtevID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<RezultatZahteva>> RezultatPrimljenogZahteva(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");
            //PRIMLJENOG ZAHTEVA
            var rezultat = await izahtev.RezultatPoslatogZahteva(zahtev);
            if (rezultat == null)
                return NotFound("Ne postoji rezultat traženog zahteva.");

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

        [HttpGet("primaoci"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<string>>> PrimaociZahteva() 
        {
            var primaoci = await izahtev.Primaoci();
            if (primaoci.Count == 0)
                return NotFound("Nema poslatih zahteva za praćenje.");

            return Ok(primaoci);
        }

        [HttpGet("podnosioci"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<string>>> PodnosiociZahteva()
        {
            var podnosioci = await izahtev.Podnosioci();
            if (podnosioci.Count == 0)
                return NotFound("Nema primljenih zahteva za praćenje.");

            return Ok(podnosioci);
        }

        [HttpGet("prijavaPoslednjeg"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<bool>> PrijavaPoslednjeg() 
        {
            return Ok(await izahtev.PrijavaPoslednjeg());
        }

        [HttpGet("zahtevPoslatKorisniku/{primalacID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<bool>> ZahtevPoslatKorisniku(string primalacID) 
        {
            var korisnik = await inalog.KorisnikPoID(primalacID);
            if (korisnik == null)
                return NotFound("Ne postoji traženi korisnik.");

            return Ok(await izahtev.ZahtevPoslatKorisniku(korisnik));
        }

        [HttpPost("posaljiZahtev/{id}/{prijava}/{pozdrav}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevZaPracenje>> PosaljiZahtev(string id, bool prijava, string pozdrav)
        {
            var korisnik = await inalog.KorisnikPoID(id);
            if (korisnik == null)
                return NotFound("Ne postoji traženi korisnik.");

            var pracenje = await izahtev.Pracen(id);
            if (pracenje)
                return BadRequest("Već pratiš korisnika " + korisnik.Ime + ".");

            var slanje = await izahtev.PosaljiZahtev(korisnik, prijava, pozdrav);
            if (slanje == null)
                return BadRequest("Već je poslat zahtev za praćenje korisniku " + korisnik.Ime + ".");

            return Ok(slanje);
        }




        [HttpPut("prihvatiZahtev/{zahtevID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevZaPracenje>> PrihvatiZahtev(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            var prihvati = await izahtev.PrihvatiZahtev(zahtev);
            if (prihvati == null)
                return BadRequest("Zahtev je već prihvaćen ili odbijen.");

            return Ok(zahtev);
        }


        [HttpPut("odbijZahtev/{zahtevID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevZaPracenje>> OdbijZahtev(string zahtevID)
        {
            var zahtev = await izahtev.NadjiPrimljenZahtev(zahtevID);
            if (zahtev == null)
                return NotFound("Traženi zahtev ne postoji, ili je uklonjen.");

            var odbij = await izahtev.OdbijZahtev(zahtev);
            if (odbij == null)
                return BadRequest("Zahtev je već prihvaćen ili odbijen.");

            return Ok(zahtev);
        }



        [HttpDelete("povuciZahtev/{zahtevID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ZahtevZaPracenje>> PovuciZahtev(string zahtevID)
        {
            var povlacenje = await izahtev.PovuciZahtev(zahtevID);
            if (povlacenje.Contains("postoji"))
                return NotFound(povlacenje);

            if (povlacenje.Contains("prihvatio"))
                return BadRequest(povlacenje);

            return Ok(povlacenje);
        }


        [HttpDelete("otprati/{pracenID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> Otprati(string pracenID) 
        {
            var brisanje = await izahtev.Otrprati(pracenID);
            if (brisanje.Contains("ID"))
                return NotFound(brisanje);

            return Ok(brisanje);
        }

        [HttpDelete("obrisiPratioca/{pratilacID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ObrisiPratioca(string pratilacID)
        {
            var brisanje = await izahtev.ObrisiPratioca(pratilacID);
            if (brisanje.Contains("ID"))
                return NotFound(brisanje);

            return Ok(brisanje);
        }




    }
}
