namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ObjavaController : ControllerBase
    {
        public IObjava iobjava;
        public ObjavaController(IObjava objava)
        {
            iobjava = objava;
        }


        [HttpGet("objave"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Objava>>> Objave()
        {
            var objave = await iobjava.Objave();
            if (objave.Count == 0)
                return NotFound("Trenutno nema objava.");

            return Ok(objave);
        }


        [HttpGet("vidljiveObjave"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Objava>>> VidljiveObjave()
        {
            var objave = await iobjava.VidljiveObjave();
            if (objave.Count == 0)
                return NotFound("Trenutno nema objava.");

            return Ok(objave);
        }


        [HttpGet("objavaPoIDu/{objavaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Objava>> ObjavaPoIDu(string objavaID)
        {
            var objava = await iobjava.ObjavaPoIDu(objavaID);
            if (objava == null)
                return NotFound("Ne postoji tražena objava.");

            return Ok(objava);
        }

        [HttpGet("objavaPracenog/{objavaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Objava>> ObjavaPracenog(string objavaID)
        {
            var objava = await iobjava.ObjavaPracenog(objavaID);
            if (objava == null)
                return NotFound("Ne postoji tražena objava.");

            return Ok(objava);
        }

        [HttpGet("objavePoTekstu/{tekst}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Objava>>> ObjavaPoTekstu(string tekst)
        {
            if (tekst.Equals(string.Empty))
                return BadRequest("Mora se uneti tekst.");

            var objave = await iobjava.ObjavePoTekstu(tekst);
            if (objave.Count == 0)
                return NotFound("Nemaš objavu koja sadrži: " + tekst);

            return Ok(objave);
        }


        [HttpGet("objavaPrijavljenogKorisnika/{objavaID}"), Authorize(Roles ="Korisnik")]
        public async Task<ActionResult<bool>> ObjavaPrijavljenogKorisnika(string objavaID) 
        {
            var objava = await iobjava.ObjavaPoIDu(objavaID);
            if (objava == null)
                return NotFound("Ne postoji tražena objava.");

            return Ok(await iobjava.ObjavaPrijavljenogKorisnika(objava));
        }


        [HttpGet("komentari/{objavaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Objava>>> Komentari(string objavaID)
        {
            var objava = await iobjava.ObjavaPoIDu(objavaID);
            if (objava == null)
                objava = await iobjava.ObjavaPracenog(objavaID);

            if (objava == null)
                return NotFound("Ne postoji tražena objava.");

            var komentari = await iobjava.Komentari(objava);
            if (komentari.Count == 0)
                return NotFound("Nema komentara na objavu.");

            return Ok(komentari);

        }


        [HttpGet("ocena/{objavaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Ocena>> OcenaNaObjavu(string objavaID)
        {
            var objava = await iobjava.ObjavaPoIDu(objavaID);
            if (objava != null)
                return NotFound("Možeš videti svoju ocenu na objavi drugog korisnika,  " +
                    "ili ocene drugih korisnika na svojoj objavi.");

            objava = await iobjava.ObjavaPracenog(objavaID);
            if (objava == null)
                return NotFound("Ne postoji tražena objava.");

            var ocena = await iobjava.OcenaNaObjavu(objava);
            if (ocena == null)
                return NotFound("Objava nije ocenjena.");

            return Ok(ocena);
        }


        [HttpGet("ocene/{objavaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Ocena>>> Ocene(string objavaID)
        {
            var objava = await iobjava.ObjavaPracenog(objavaID);
            if (objava != null)
                return NotFound("Ne možete videti ocene drugih korisnika na tuđoj objavi, samo svoju ocenu. " +
                    "Ocene drugih korisnika možeš videti jedino na svojoj objavi.");

            objava = await iobjava.ObjavaPoIDu(objavaID);
            if (objava == null)
                return NotFound("Ne postoji tražena objava.");

            var ocene = await iobjava.Ocene(objava);
            if (ocene.Count == 0)
                return NotFound("Objava nije ocenjena.");

            return Ok(ocene);
        }


        [HttpGet("sveObjavePracenog/{korisnikID}"), Authorize(Roles ="Korisnik")]
        public async Task<ActionResult<ICollection<Objava>>> SveObjavePracenog(string korisnikID)
        {
            return Ok(await iobjava.SveObjavePracenog(korisnikID));
        }

        [HttpGet("prosek/{objavaID}"), Authorize(Roles ="Korisnik")]
        public async Task<ActionResult<double>> Prosek(string objavaID) 
        {
            var objava = await iobjava.ObjavaPoIDu(objavaID);
            if (objava == null)
                objava = await iobjava.ObjavaPracenog(objavaID);
                if (objava == null)
                    return NotFound("Ne postoji tražena objava.");
  

            return Ok(await iobjava.Prosek(objava));
        }


        [HttpGet("autori"), Authorize(Roles ="Korisnik")]
        public async Task<ActionResult<ICollection<Korisnik>>> Autori() 
        {
            return Ok(await iobjava.AutoriObjava());
        }

        [HttpGet("autoriKomentara/{objavaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Korisnik>>> AutoriKomentara(string objavaID)
        {
            var objava = await iobjava.ObjavaPoIDu(objavaID);
            if (objava == null)
                objava = await iobjava.ObjavaPracenog(objavaID);
            if(objava == null)
                return NotFound("Ne postoji tražena objava.");

            return Ok(await iobjava.AutoriKomentara(objava));
        }

        [HttpGet("autoriOcena/{objavaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Korisnik>>> AutoriOcena(string objavaID)
        {
            var objava = await iobjava.ObjavaPoIDu(objavaID);
            if (objava == null)
                return NotFound("Ne postoji tražena objava.");

            return Ok(await iobjava.AutoriOcena(objava));
        }

        [HttpGet("komentarPratioca/{komID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Objava>> KomentarPratioca(string komID) 
        {
            var komentar = await iobjava.KomentarPratiocaNaObjavu(komID);
            if (komentar == null)
                return NotFound("Odabrani komentar ne postoji, ili je obrisan.");

            return Ok(komentar);
        }

        [HttpGet("komentarPrijavljenog/{komID}"), Authorize(Roles ="Korisnik")]
        public async Task<ActionResult<Objava>> KomentarPrijavljenog(string komID)
        {
            var komentar = await iobjava.KomentarPrijavljenog(komID);
            if (komentar == null)
                return NotFound("Odabrani komentar ne postoji, ili je obrisan.");

            return Ok(komentar);
        }



        [HttpPost("objavi/{tekst}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Objava>> Objavi(string tekst)
        {
            if (tekst.Equals(string.Empty))
                return BadRequest("Mora se uneti tekst.");

            return Ok(await iobjava.Objavi(tekst));
        }

        [HttpPost("objaviSaSlikom/{tekst}/{slika}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Objava>> ObjaviSaSlikom(string tekst, string slika)
        {
            if (tekst.Equals(string.Empty))
                return BadRequest("Mora se uneti tekst.");

            return Ok(await iobjava.ObjaviSaSlikom(tekst, slika));
        }


        [HttpPut("komentarisiPracenom/{objavaID}/{tekst}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Objava>> KomentarisiObjavuPracenom(string objavaID, string tekst)
        {
            var objava = await iobjava.ObjavaPracenog(objavaID);
            if (objava == null)
                return NotFound("Ne postoji tražena objava.");

            if (tekst.Equals(string.Empty))
                return BadRequest("Mora se uneti tekst.");

            return Ok(await iobjava.Komentarisi(objava, tekst));
        }


        [HttpPut("komentarisiSvojuObjavu/{objavaID}/{tekst}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Objava>> KomentarisiSvojuObjavu(string objavaID, string tekst)
        {
            var objava = await iobjava.ObjavaPoIDu(objavaID);
            if (objava == null)
                return NotFound("Ne postoji tražena objava.");

            if (tekst.Equals(string.Empty))
                return BadRequest("Mora se uneti tekst.");

            return Ok(await iobjava.Komentarisi(objava, tekst));
        }

        [HttpPut("oceni/{objavaID}/{vrednost}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Objava>> Oceni(string objavaID, int vrednost) 
        {
            var objava = await iobjava.ObjavaPoIDu(objavaID);
            if (objava != null)
                return BadRequest("Ne možeš oceniti sopstvenu objavu, nego samo objave korisnika koje pratiš.");

            objava = await iobjava.ObjavaPracenog(objavaID);
            if (objava == null)
                return NotFound("Ne postoji tražena objava.");

            if (vrednost < 0 || vrednost > 5)
                return BadRequest("Možete dati ocenu od 1 do 5.");

            var ocena = await iobjava.Oceni(objava, vrednost);
            if (ocena == null)
                return BadRequest("Došlo je do greške.");

            return Ok(objava);
        }



        [HttpPut("prepraviObjavu/{objavaID}/{tekst}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Objava>> PromeniObjavu(string objavaID, string tekst)
        {
            var objava = await iobjava.KomentarPrijavljenog(objavaID); 

            if (objava == null)
                objava = await iobjava.ObjavaPoIDu(objavaID);

            if (objava == null)
                return NotFound("Ne postoji tražena objava.");

            return Ok(await iobjava.PrepraviObjavu(objava, tekst));
        }



        [HttpDelete("povuciOcenu/{objavaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<Objava>> PovuciOcenu(string objavaID)
        {
           var objava = await iobjava.ObjavaPracenog(objavaID);
                    if (objava == null)
                         return NotFound("Ne postoji tražena objava.");

            var brisanje = await iobjava.PovuciOcenu(objava);
            if (brisanje == null)
                return BadRequest("Nisi ocenio/la objavu, pa ne možeš ni povući ocenu.");

            return Ok(brisanje);
        }


        [HttpDelete("obrisiObjavu/{objavaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ObrisiObjavu(string objavaID)
        {
            var brisanje = await iobjava.ObrisiObjavu(objavaID);
            if(brisanje == string.Empty)
                return NotFound("Ne postoji tražena objava.");

            return Ok(brisanje);
        }

        [HttpDelete("obrisiKomentar/{objavaID}"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<ICollection<Objava>>> ObrisiKomentar(string objavaID)
        {
            var brisanje = await iobjava.ObrisiKomentar(objavaID);
            if (brisanje == null)
                return NotFound(brisanje);

            return Ok(brisanje);
        }


        [HttpDelete("obrisiObjave"), Authorize(Roles = "Korisnik")]
        public async Task<ActionResult<string>> ObrisiObjave()
        {
            var brisanje = await iobjava.ObrisiObjave();
            if (brisanje.Contains("nije"))
                return NotFound(brisanje);

            return Ok(brisanje);
        }



    }
}
