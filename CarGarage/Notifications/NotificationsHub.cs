using Microsoft.AspNetCore.SignalR;

namespace CarGarage.Notifications
{
    public class NotificationsHub : Hub
    {
        // send unread count update to specific user
        public async Task SendUnreadCount(string userId, int count)
        {
            await Clients.User(userId).SendAsync("UnreadCountUpdated", count);
        }
    }
}
