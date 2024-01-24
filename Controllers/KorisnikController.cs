namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class KorisnikController : ControllerBase
    {
        public INalog inalog;
        public IDan idan;
        public KorisnikController(INalog nalog, IDan dan)
        { 
            inalog = nalog;
            idan = dan;
        }


        [HttpGet("korisnici"), Authorize(Roles ="Korisnik")]
        public async Task<ActionResult<ICollection<Korisnik>>> Korisnici()
        {
            var korisnici = await inalog.Korisnici();
            if (korisnici == null)
                return NotFound("Nema korisnika.");

            return Ok(korisnici);
        }


        [HttpGet("prijavljen"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Korisnik>> Prijavljen() 
        {
            return Ok( await inalog.KorisnikPoID( inalog.PrijavljenID()) );
        } 

        [HttpGet("korisnikPoIDu/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Korisnik>> KorisnikPoID(string id)
        {
            var korisnik = await inalog.KorisnikPoID(id);

            if (korisnik == null)
                return NotFound("Ne postoji traženi korisnik.");

            return Ok(korisnik);
        }

        [HttpGet("korisniciPoImenu/{ime}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Korisnik>>> KorisniciPoImenu(string ime)
        {
            ICollection<Korisnik> korisnici = await inalog.KorisniciPoImenu(ime);
            if (!korisnici.Any())
                return NotFound("Ne postoji korisnik sa unetim imenom " + ime + ".");

            return Ok(korisnici);
        }

        [HttpGet("korisnikPoImenu/{ime}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Korisnik>> KorisnikPoImenu(string ime)
        {
            Korisnik korisnik = await inalog.KorisnikPoImenu(ime);
            if (korisnik == null)
                return NotFound("Ne postoji korisnik sa imenom " + ime + ".");

            return Ok(korisnik);
        }

        [HttpGet("pratioci"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Korisnik>>> Pratioci() 
        {
            var pratioci = await inalog.Pratioci();
            if (pratioci.Count == 0)
                return NotFound("Nema pratilaca.");

            return Ok(pratioci);
        }

        [HttpGet("praceni"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Korisnik>>> Praceni()
        {
            var praceni = await inalog.Praceni();
            if (praceni.Count == 0)
                return NotFound("Ne pratiš nikoga.");

            return Ok(praceni);
        }

        [HttpGet("pracen/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<bool>> Pracen(string id)
        {
            var korisnik = await inalog.KorisnikPoID(id);
            if (korisnik == null)
                return NotFound("Ne postoji traženi korisnik.");

            var pracen = await inalog.Pracen(korisnik);
            if (pracen == null)
                return NotFound("Ne pratiš korisnika " + korisnik.Ime + ". Želiš li da ga zapratiš?");

            return Ok(pracen);
        }

        [HttpGet("pratilac/{id}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<bool>> Pratilac(string id)
        {
            var korisnik = await inalog.KorisnikPoID(id);
            if (korisnik == null)
                return NotFound("Ne postoji traženi korisnik.");

            var pracen = await inalog.Pratilac(korisnik);
            if (pracen == null)
                return NotFound("Korisnik " + korisnik.Ime + " te ne prati.");

            return Ok(pracen);
        }


        [HttpPost("prijava/{ime}/{sifra}")]
        public async Task<IActionResult> Prijava(string ime, string sifra)
        {
            var prijava = await inalog.Prijava(ime, sifra);

            if (prijava.Contains("pogrešna"))
                return BadRequest(prijava);
            else if (prijava.Contains("pogrešno"))
                return NotFound(prijava);
    
           
            return Ok(prijava);
        }

        [HttpGet("ponudiBrDana/{mesec}")]
        public ActionResult<int> PonudiBrDana(int mesec)
        {
            return Ok(inalog.PonudiBrDana(mesec));
        }



        [HttpPost("registracija/{ime}/{unetaSifra}/{godina}/{mesec}/{dan}/{pol}/{slika}")]
        public async Task<ActionResult<string>> Registracija(string ime, string unetaSifra, int godina, int mesec, int dan, PolKorisnika pol, string slika)
        {
            string rezutat = await inalog.Registracija(ime, unetaSifra, godina, mesec, dan, pol, slika);

            if (!rezutat.Contains("registrovan"))
                return BadRequest(rezutat);
            
            return Ok(rezutat);
        }

        [HttpPut("promeniIme/{novoIme}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Korisnik>> PromeniImeKorisnika(string novoIme)
        {
            if (String.IsNullOrEmpty(novoIme))
                return BadRequest("Nije uneto novo ime.");

            if ((await inalog.KorisnikPoImenu(novoIme) != null) || ((await inalog.AdministratorAktivnosti()).Ime.Equals(novoIme))
                || ((await inalog.AdministratorNamirnica()).Ime.Equals(novoIme)))
                return BadRequest("Ime " + novoIme + " je već zauzeto. Izaberi drugo.");

            return Ok(await inalog.PromeniIme(novoIme));
        }



        [HttpPut("promeniSifru/{novaSifra}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Korisnik>> PromeniSifruKorisnika(string novaSifra)
        {
            if (String.IsNullOrEmpty(novaSifra))
                return BadRequest("Nije uneta nova šifra.");

            return Ok(await inalog.PromeniSifru(novaSifra));
        }





        [HttpPut("promeniSliku/{slika}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Korisnik>> PromeniSliku( string slika)
        {
            if (String.IsNullOrEmpty(slika.Trim()))
                    return NotFound("Nije uneta nova slika.");

            var promena = await inalog.PromeniSliku(slika);
           if (promena == null)
               return BadRequest("Nije pronađena slika.");

            return Ok(promena);
        }

        [HttpPut("promeniDatumRodjenja/{godina}/{mesec}/{dan}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> PromeniDatumRodjenja(int godina, int mesec, int dan)
        {
            string rezultat = string.Empty;
            rezultat = await inalog.PromeniDatumRodjenja(godina, mesec, dan);
            if (rezultat.Contains("Nemoguće") || rezultat.Contains("Unesi"))
                return BadRequest(rezultat);

            return Ok(rezultat);
        }

        [HttpDelete("obrisiSvojNalog"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<String>> ObrisiSvojNalog()
        {
            return Ok(await inalog.ObrisiKorisnika());
        }





    }
}
