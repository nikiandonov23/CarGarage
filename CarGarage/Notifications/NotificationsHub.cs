using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CarGarage.Notifications
{
    [Authorize]
    public class NotificationsHub : Hub
    {
        public async Task SendUnreadCount(
            string userId,
            int count)
        {
            await Clients.User(userId)
                .SendAsync(
                    "UnreadCountUpdated",
                    count);
        }
    }
}