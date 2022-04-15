using DesktopToast;
using System.Reflection;

namespace MediaDownloader
{
    internal class ToastNotification
    {
        internal static void ShowToast(int expire, ToastAudio toastAudio, string ToastTitle, string ToastMessage)
        {
            ToastRequest request = new ToastRequest
            {
                ToastTitle = ToastTitle,
                ToastBodyList = new[] { ToastMessage },
                ToastAudio = toastAudio,
                ShortcutFileName = "MediaDownloader.lnk",
                ShortcutTargetFilePath = Assembly.GetExecutingAssembly().Location,
                AppId = "MediaDownloader",
            };

            ToastManager.Show(request, expire);
        }
    }
}
