using Microsoft.AspNetCore.SignalR;

namespace OnlineSinav.Hubs
{
    public class QuizHub : Hub
    {
        public async Task JoinTeacherGroup(string teacherId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Teacher_{teacherId}");
        }

        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }
    }
} 