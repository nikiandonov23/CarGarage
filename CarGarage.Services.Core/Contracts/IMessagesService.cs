using CarGarage.DataModels;

namespace CarGarage.Services.Core.Contracts
{
    public interface IMessagesService
    {
        Task<Message> AddMessageAsync(
            string senderId,
            string receiverId,
            string content,
            string? conversationId = null);

        Task<IEnumerable<Message>> GetInboxAsync(
            string userId);

        Task<IEnumerable<Message>> SearchInboxAsync(
            string userId,
            string? query,
            int page,
            int pageSize);

        Task<IEnumerable<Message>> GetOutboxAsync(
            string userId);

        Task<int> GetUnreadCountAsync(
            string userId);

        Task<Message?> GetByIdAsync(
            int id,
            string userId);

        Task MarkAsReadAsync(
            int id,
            string userId);

        Task TogglePinAsync(
            int id,
            string userId);

        Task DeleteAsync(
            int id,
            string userId);

        Task<IEnumerable<Message>> GetConversationAsync(
            int messageId,
            string userId);
    }
}