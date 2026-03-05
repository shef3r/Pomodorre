using System;
using System.Threading.Tasks;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace Pomodorre.WinUI.Services
{
    public static class NotificationService
    {
        private const string GROUP = "pomodoro";

        public static async Task ShowOrUpdateAsync(PomodoroSession session, double progress)
        {
            string tag = session.Id.ToString();

            try
            {
                var progressData = new AppNotificationProgressData(0)
                {
                    Title = "Progress",
                    Value = progress,
                    ValueStringOverride = $"{session.Remaining:mm\\:ss} remaining",
                    Status = session.IsBreak ? "Break" : "Focus"
                };

                var result = await AppNotificationManager.Default.UpdateAsync(progressData, tag, GROUP);

                if (result != AppNotificationProgressResult.Succeeded)
                {
                    Console.WriteLine("[Notification] Progress update failed; creating new notification.");

                    var builder = new AppNotificationBuilder()
                        .AddText(session.IsBreak ? "Break time" : "Focus time")
                        .AddText($"Block {session.CurrentBlockIndex} of {session.TotalBlocks}")
                        .AddProgressBar(new AppNotificationProgressBar()
                        {
                            Title = "Progress",
                            Value = progress,
                            ValueStringOverride = $"{session.Remaining:mm\\:ss} remaining",
                            Status = session.IsBreak ? "Break" : "Focus"
                        });

                    var notification = builder.BuildNotification();
                    notification.Tag = tag;
                    notification.Group = GROUP;

                    try
                    {
                        AppNotificationManager.Default.Show(notification);
                        Console.WriteLine("[Notification] Shown successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Notification] Failed to show notification: {ex.Message}");
                        Console.WriteLine($"[Notification] Fallback: {session.IsBreak} Block {session.CurrentBlockIndex}/{session.TotalBlocks}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Notification] Failed to update/show: {ex.Message}");
                Console.WriteLine($"[Notification Fallback] {session.IsBreak} Block {session.CurrentBlockIndex}/{session.TotalBlocks} ({session.Remaining:mm\\:ss})");
            }
        }

        public static async Task RemoveAsync(string id)
        {
            try
            {
                await AppNotificationManager.Default.RemoveByTagAsync(id);
                Console.WriteLine($"[Notification] Removed {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Notification] Failed to remove {id}: {ex.Message}");
            }
        }
    }
}