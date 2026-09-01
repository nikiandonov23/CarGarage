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

        public async Task<IEnumerable<CarGarage.DataModels.Message>> GetConversationAsync(int messageId, string userId)
        {
            var msg = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId && (m.ReceiverId == userId || m.SenderId == userId));
            if (msg == null) return Enumerable.Empty<CarGarage.DataModels.Message>();

            if (!string.IsNullOrEmpty(msg.ConversationId))
            {
                return await _context.Messages
                    .Where(m => m.ConversationId == msg.ConversationId && (m.ReceiverId == userId || m.SenderId == userId) && !m.IsDeleted)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();
            }

            // fallback: conversation between the two participants
            var otherId = msg.SenderId == userId ? msg.ReceiverId : msg.SenderId;
            return await _context.Messages
                .Where(m => ((m.SenderId == userId && m.ReceiverId == otherId) || (m.SenderId == otherId && m.ReceiverId == userId)) && !m.IsDeleted)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
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

            // Notifications are sent from the web controller via SignalR hub after adding message.
        }

        public async Task<IEnumerable<CarGarage.DataModels.Message>> GetInboxAsync(string userId)
        {
            return await _context.Messages
                .Where(m => m.ReceiverId == userId && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CarGarage.DataModels.Message>> GetOutboxAsync(string userId)
        {
            return await _context.Messages
                .Where(m => m.SenderId == userId && !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Messages.CountAsync(m => m.ReceiverId == userId && !m.IsRead && !m.IsDeleted);
        }

        public async Task<CarGarage.DataModels.Message?> GetByIdAsync(int id, string userId)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id && (m.ReceiverId == userId || m.SenderId == userId) && !m.IsDeleted);
        }

        public async Task MarkAsReadAsync(int id, string userId)
        {
            var msg = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id && m.ReceiverId == userId);
            if (msg == null) return;
            msg.IsRead = true;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var msg = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id && (m.ReceiverId == userId || m.SenderId == userId));
            if (msg == null) return;
            // soft-delete
            msg.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

        public async Task TogglePinAsync(int id, string userId)
        {
            var msg = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id && (m.ReceiverId == userId || m.SenderId == userId));
            if (msg == null) return;
            msg.IsPinned = !msg.IsPinned;
            await _context.SaveChangesAsync();
        }

        // Simple search + pagination for inbox
        public async Task<IEnumerable<CarGarage.DataModels.Message>> SearchInboxAsync(string userId, string? query, int page = 1, int pageSize = 20)
        {
            var q = _context.Messages.Where(m => m.ReceiverId == userId && !m.IsDeleted);
            if (!string.IsNullOrWhiteSpace(query))
            {
                q = q.Where(m => m.Content.Contains(query));
            }

            return await q.OrderByDescending(m => m.SentAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
        }
    }
}
