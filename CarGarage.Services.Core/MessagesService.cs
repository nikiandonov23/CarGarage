using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.Services.Core.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class MessagesService(ApplicationDbContext context) : IMessagesService
    {
        public async Task<Message> AddMessageAsync(
            string senderId,
            string receiverId,
            string content,
            string? conversationId = null)
        {
            var msg = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                ConversationId = conversationId,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                IsDeleted = false
            };

            await context.Messages.AddAsync(msg);
            await context.SaveChangesAsync();

            return msg;
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(
            int messageId,
            string userId)
        {
            var msg = await context.Messages
                .FirstOrDefaultAsync(m =>
                    m.Id == messageId &&
                    (m.ReceiverId == userId ||
                     m.SenderId == userId) &&
                    !m.IsDeleted);

            if (msg == null)
                return Enumerable.Empty<Message>();

            if (!string.IsNullOrEmpty(msg.ConversationId))
            {
                return await context.Messages
                    .Where(m =>
                        m.ConversationId == msg.ConversationId &&
                        (m.ReceiverId == userId ||
                         m.SenderId == userId) &&
                        !m.IsDeleted)
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();
            }

            var otherId =
                msg.SenderId == userId
                    ? msg.ReceiverId
                    : msg.SenderId;

            return await context.Messages
                .Where(m =>
                    (
                        (m.SenderId == userId &&
                         m.ReceiverId == otherId)
                        ||
                        (m.SenderId == otherId &&
                         m.ReceiverId == userId)
                    )
                    && !m.IsDeleted)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetInboxAsync(
            string userId)
        {
            return await context.Messages
                .Where(m =>
                    (m.ReceiverId == userId ||
                     m.SenderId == userId) &&
                    !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetOutboxAsync(
            string userId)
        {
            return await context.Messages
                .Where(m =>
                    m.SenderId == userId &&
                    !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(
            string userId)
        {
            return await context.Messages
                .CountAsync(m =>
                    m.ReceiverId == userId &&
                    !m.IsRead &&
                    !m.IsDeleted);
        }

        public async Task<Message?> GetByIdAsync(
            int id,
            string userId)
        {
            return await context.Messages
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    (m.ReceiverId == userId ||
                     m.SenderId == userId) &&
                    !m.IsDeleted);
        }

        public async Task MarkAsReadAsync(
            int id,
            string userId)
        {
            var msg = await context.Messages
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.ReceiverId == userId &&
                    !m.IsDeleted);

            if (msg == null)
                return;

            msg.IsRead = true;

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var msg = await context.Messages
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    (m.ReceiverId == userId || m.SenderId == userId));

            if (msg == null)
                return;

            if (!string.IsNullOrEmpty(msg.ConversationId))
            {
                var conversationMessages = await context.Messages
                    .Where(m =>
                        m.ConversationId == msg.ConversationId &&
                        (m.ReceiverId == userId || m.SenderId == userId))
                    .ToListAsync();

                foreach (var message in conversationMessages)
                {
                    message.IsDeleted = true;
                }
            }
            else
            {
                var otherId = msg.SenderId == userId ? msg.ReceiverId : msg.SenderId;

                var directMessages = await context.Messages
                    .Where(m =>
                        ((m.SenderId == userId && m.ReceiverId == otherId) ||
                         (m.SenderId == otherId && m.ReceiverId == userId)))
                    .ToListAsync();

                foreach (var message in directMessages)
                {
                    message.IsDeleted = true;
                }
            }

            await context.SaveChangesAsync();
        }

        public async Task TogglePinAsync(
            int id,
            string userId)
        {
            var msg = await context.Messages
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    (m.ReceiverId == userId ||
                     m.SenderId == userId));

            if (msg == null)
                return;

            msg.IsPinned = !msg.IsPinned;

            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Message>> SearchInboxAsync(
            string userId,
            string? query,
            int page = 1,
            int pageSize = 20)
        {
            var q = context.Messages
                .Where(m =>
                    (m.ReceiverId == userId ||
                     m.SenderId == userId) &&
                    !m.IsDeleted);

            if (!string.IsNullOrWhiteSpace(query))
            {
                q = q.Where(m =>
                    m.Content.Contains(query));
            }

            return await q
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}