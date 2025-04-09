using Microsoft.EntityFrameworkCore;
using proiect_licenta.Contexts;
using proiect_licenta.Models;
using System.Security.Claims;

namespace proiect_licenta.Services
{
    public class CardService
    {
        // add  access checking
        private readonly ApplicationDbContext _context;
        //private readonly PrivilegeChecker _privilegeChecker;
        //private readonly ClaimsPrincipal _user;

        public CardService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
        }

        public async Task<IEnumerable<Card>> GetAllCards()
        {
            var cards = await _context.Cards
                .ToListAsync();
            return cards;
        }

        public async Task<Card> GetCard(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card == null) { throw new Exception(); }
            return card;
        }

        public async Task<Card> CreateCard(Card card)
        {
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<Card> EditCard(Card card)
        {
            //check privilege
            var existingCard = await _context.Cards.FindAsync(card.Id);
            if (existingCard == null)
                throw new Exception("Card does not exist");

            _context.Entry(card).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task DeleteCard(int id)
        {
            var task = await _context.Cards.FindAsync(id);
            if (task == null)
                throw new Exception("Card does not exist");

            _context.Cards.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}
