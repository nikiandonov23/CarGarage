using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.Services.Core.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class MessagesService : IMessagesService
    {
        private readonly ApplicationDbContext _context;

        public MessagesService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddMessageAsync(string senderId, string receiverId, string content, string? conversationId = null)
        {
            var msg = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                ConversationId = conversationId
            };

            await _context.Messages.AddAsync(msg);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CarGarage.DataModels.Message>> GetInboxAsync(string userId)
        {
            return await _context.Messages
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }
    }
}
