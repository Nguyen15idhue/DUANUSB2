using Serilog;

namespace DongleSyncService.Utils
{
    /// <summary>
    /// Helper class to show Windows toast notifications
    /// </summary>
    public static class NotificationHelper
    {
        /// <summary>
        /// Show a Windows toast notification using PowerShell
        /// </summary>
        public static void ShowNotification(string title, string message, bool isWarning = false)
        {
            try
            {
                // Use PowerShell to show toast notification
                var ps = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $@"-NoProfile -WindowStyle Hidden -Command ""
                        [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null;
                        [Windows.UI.Notifications.ToastNotification, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null;
                        [Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] | Out-Null;
                        
                        $APP_ID = 'USBDongleService';
                        $xml = [Windows.Data.Xml.Dom.XmlDocument]::new();
                        $xml.LoadXml(@'
                        <toast>
                            <visual>
                                <binding template='ToastGeneric'>
                                    <text>{title}</text>
                                    <text>{message}</text>
                                </binding>
                            </visual>
                        </toast>
'@);
                        $toast = [Windows.UI.Notifications.ToastNotification]::new($xml);
                        [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier($APP_ID).Show($toast);
                    """,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                
                Log.Debug("Notification shown: {Title} - {Message}", title, message);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to show notification");
            }
        }

        /// <summary>
        /// Show notification about app being closed for patching
        /// </summary>
        public static void NotifyAppClosed(string appName)
        {
            ShowNotification(
                "USB Dongle Detected",
                $"{appName} was closed to apply updates. Please restart the application.",
                isWarning: true
            );
        }

        /// <summary>
        /// Show notification about successful patch
        /// </summary>
        public static void NotifyPatchSuccess()
        {
            ShowNotification(
                "USB Dongle",
                "Features updated successfully. You can now use the application.",
                isWarning: false
            );
        }

        /// <summary>
        /// Show notification about USB removal
        /// </summary>
        public static void NotifyUSBRemoved()
        {
            ShowNotification(
                "USB Dongle Removed",
                "Enhanced features have been disabled. Original functionality restored.",
                isWarning: true
            );
        }
    }
}
