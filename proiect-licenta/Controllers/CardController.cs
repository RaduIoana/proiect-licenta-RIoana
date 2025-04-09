using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using proiect_licenta.Services;

namespace proiect_licenta.Controllers
{
    [Route("api/Cards")]
    [ApiController]
    [Authorize]
    public class CardController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly CardService _cardService;

        // GET: api/MyModel
        [HttpGet("get_all_cards")]
        public async Task<ActionResult<IEnumerable<Card>>> GetAllCards()
        {
            // Simulate async database access
            return Ok(await _cardService.GetAllCards());
        }

        // GET: api/MyModel/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Card>> GetById(int id)
        {
            return Ok(await _cardService.GetCard(id));
        }

        [HttpPut]
        public async Task<ActionResult<Card>> PutCard(Card card)
        {
            return Ok(await _cardService.EditCard(card));
        }

        [HttpPost]
        public async Task<ActionResult<Card>> PostCard(Card card)
        {
            return Ok(await _cardService.CreateCard(card));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCard(int id)
        {
            await _cardService.DeleteCard(id);
            return NoContent();
        }
    }

}
