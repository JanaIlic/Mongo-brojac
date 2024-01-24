namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class PorukaController : ControllerBase
    {
        public IPoruka iporuka;
        public INalog inalog;
        public PorukaController(IPoruka poruka, INalog nalog)
        {
            iporuka = poruka;
            inalog = nalog;
        }



        [HttpGet("poslataPoruka/{porukaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Poruka>> PoslataPoruka(string porukaID)
        {
            var poruka = await iporuka.NadjiPoslatuPoruku(porukaID);
            if (poruka == null)
                return NotFound("Nema poruke.");

            return Ok(poruka);  
        }


        [HttpGet("poslatePoruke"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Poruka>>> PoslatePoruke() 
        {
            var poruke = await iporuka.PoslatePoruke();
            if(poruke.Count == 0)
                return NotFound("Nema poslatih poruka.");

            return Ok(poruke);
        }


        [HttpGet("primljenePoruke"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Poruka>>> PrimljenePoruke() 
        {
            var poruke = await iporuka.PrimljenePoruke();
            if (poruke.Count == 0)
                return NotFound("Nema primljenih poruka.");

            return Ok(poruke);
        }


        [HttpGet("razgovor/{korisnikID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Poruka>>> Razgovor(string korisnikID) 
        {
            var korisnik = await inalog.KorisnikPoID(korisnikID);
            if (korisnik == null)
                return NotFound("Ne postoji traženi korisnik.");

            if (!(await iporuka.Pracen(korisnikID) && await iporuka.Pratilac(korisnikID)))
                return NotFound("Nema razgovora sa ovim korisnikom, jer se međusobno ne pratite.");

            var razgovor = await iporuka.Razgovor(korisnik);
            if (razgovor.Count == 0)
                return NotFound("Nema poruka razmenjenih sa korisnikom " + korisnik.Ime + ".");

            return Ok(razgovor);
        }


        [HttpGet("autoriPoruka/{korisnikID}"), Authorize(Roles ="Korisnik")]
        public async Task<ActionResult<bool>> AutoriPoruka(string korisnikID) 
        {
            var korisnik = await inalog.KorisnikPoID(korisnikID);
            if (korisnik == null)
                return NotFound("Ne postoji traženi korisnik");

            if (!(await iporuka.Pracen(korisnikID) && await iporuka.Pratilac(korisnikID)))
                return NotFound("Nema razgovora sa ovim korisnikom, jer se međusobno ne pratite.");


            return Ok(await iporuka.AutoriPoruka(korisnik));
        }



        [HttpGet("sagovornici"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Korisnik>>> Sagovornici() 
        {
            var sagovornici = await iporuka.Sagovornici();
            if(sagovornici.Count == 0)
                return NotFound("Nema poruka.");

            return Ok(sagovornici); 
        }


        [HttpPost("posaljiPoruku/{korisnikID}/{tekst}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Poruka>> PosaljiPoruku(string korisnikID, string tekst) 
        {
            var korisnik = await inalog.KorisnikPoID(korisnikID);
            if (korisnik == null)
                return NotFound("Ne postoji traženi korisnik.");

            if (!(await iporuka.Pracen(korisnikID) && await iporuka.Pratilac(korisnikID)))
                return NotFound("Ne možeš poslati poruku korisniku " + korisnik.Ime + ", jer se međusobno ne pratite.");

            if (tekst.Equals(string.Empty))
                return BadRequest("Mora se uneti tekst poruke.");

            return Ok(await iporuka.PosaljiPoruku(korisnik, tekst));
        }


        [HttpPut("prepraviPoruku/{porukaID}/{tekst}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Poruka>> PrepraviPoruku(string porukaID, string tekst) 
        {
            if (tekst.Equals(string.Empty))
                return BadRequest("Mora se uneti tekst poruke.");

            var prepravka = await iporuka.PrepraviPoruku(porukaID, tekst);
            if (prepravka == null)
                return NotFound("Tražena poruka nije poslata.");

            return Ok(prepravka);
        }


        [HttpDelete("obrisiPoruku/{porukaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ObrisiPoruku(string porukaID) 
        {
            var brisanje = await iporuka.ObrisiPoruku(porukaID);
            if (brisanje.Contains("ID"))
                return NotFound(brisanje);

            return Ok(brisanje);
        }


        [HttpDelete("obrisiRazgovor/{korisnikID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ObrisiRazgovor(string korisnikID) 
        {
            var korisnik = await inalog.KorisnikPoID(korisnikID);
            if (korisnik == null)
                return NotFound("Ne postoji traženi korisnik.");

            var brisanje = await iporuka.ObrisiRazgovor(korisnik);
            if (brisanje.Contains("Nema"))
                return NotFound(brisanje);

            return Ok(brisanje);
        }


        [HttpDelete("obrisiRazgovore"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ObrisiRazgovore() 
        {
            var brisanje = await iporuka.ObrisiRazgovore();
            return Ok(brisanje);
        }

        [HttpDelete("obrisiPoslatePoruke"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ObrisiPoslatePoruke()
        {
            var brisanje = await iporuka.ObrisiPoslatePoruke();
            return Ok(brisanje);
        }


    }
}
