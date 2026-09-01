using System.Collections.Generic;
using System.Threading.Tasks;
using CarGarage.DataModels;

namespace CarGarage.Services.Core.Contracts
{
    public interface IMessagesService
    {
        Task AddMessageAsync(string senderId, string receiverId, string content, string? conversationId = null);
        Task<IEnumerable<CarGarage.DataModels.Message>> GetInboxAsync(string userId);
        Task<IEnumerable<CarGarage.DataModels.Message>> GetOutboxAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<CarGarage.DataModels.Message?> GetByIdAsync(int id, string userId);
        Task MarkAsReadAsync(int id, string userId);
        Task DeleteAsync(int id, string userId);


    }
}
