using API.Servisi.Interfejsi;
using API.Servisi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class PopunjavanjeController : ControllerBase
    {
        private IMongoDB imdb;

        public PopunjavanjeController(IMongoDB mongodb) 
        {
            this.imdb = mongodb;
        }


        [HttpPost("prvaAktivnost")]
        public async Task<ActionResult> UbaciPrvuAktivnost()
        {
            return Ok(await imdb.UpisPrveAktivnosti());
        }

        [HttpPost("prvaNamirnica")]
        public async Task<ActionResult> UbaciPrvuNamirnicu()
        {
            return Ok(await imdb.UpisPrveNamirnice());
        }

        [HttpPost]
        public async Task<ActionResult> Popuni()
        {
            await imdb.Popuni();
  
            return Ok("popunjeno");
        }

        [HttpPut]
        public async Task<ActionResult> Promeni()
        {
            await imdb.Promeni();

            return Ok("promenjeno");
        }

        [HttpDelete]
        public async Task<ActionResult> Obriši()
        {
            await imdb.Obrisi();

            return Ok("obrisano");
        }
    }
}
