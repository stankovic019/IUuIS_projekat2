using Notification.Wpf;


namespace NetworkService.Services
{
    public class NotificationService
    {
        private readonly NotificationManager manager;

        private static NotificationService instance;
        public static NotificationService Instance => instance ?? (instance = new NotificationService());

        private NotificationService()
        {
            manager = new NotificationManager();
        }

        public void ShowInfo(string message, string title = "Info")
        {
            manager.Show(new NotificationContent
            {
                Title = title,
                Message = message,
                Type = NotificationType.Information

            }, "WindowNotificationArea");
        }

        public void ShowSuccess(string message, string title = "Success")
        {
            manager.Show(new NotificationContent
            {
                Title = title,
                Message = message,
                Type = NotificationType.Success
            }, "WindowNotificationArea");
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            manager.Show(new NotificationContent
            {
                Title = title,
                Message = message,
                Type = NotificationType.Warning
            }, "WindowNotificationArea");
        }

        public void ShowError(string message, string title = "Error")
        {
            manager.Show(new NotificationContent
            {
                Title = title,
                Message = message,
                Type = NotificationType.Error
            }, "WindowNotificationArea");
        }
    }
}
