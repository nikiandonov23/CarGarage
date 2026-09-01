using System.Collections.Generic;
using System.Threading.Tasks;
using CarGarage.DataModels;

namespace CarGarage.Services.Core.Contracts
{
    public interface IMessagesService
    {
        Task AddMessageAsync(string senderId, string receiverId, string content, string? conversationId = null);
        Task<IEnumerable<CarGarage.DataModels.Message>> GetInboxAsync(string userId);
    }
}
