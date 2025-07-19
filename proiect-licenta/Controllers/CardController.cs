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
    public class CardController : ControllerBase
    {
        private readonly CardService _cardService;
        private readonly ApplicationDbContext _context;

        public CardController(CardService cardService, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _cardService = cardService;
            _context = context;
        }

        [HttpGet("get_all_cards")]
        public async Task<ActionResult<IEnumerable<Card>>> GetAllCards()
        {
            return Ok(await _cardService.GetAllCards());
        }

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
