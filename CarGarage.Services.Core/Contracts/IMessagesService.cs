using CarGarage.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGarage.Services.Core.Contracts
{
    public interface IMessagesService
    {
        Task AddMessageAsync(
            string senderId,
            string receiverId,
            string content,
            string? conversationId = null);

        Task<IEnumerable<Message>> GetInboxAsync(
            string userId);
    }
}
