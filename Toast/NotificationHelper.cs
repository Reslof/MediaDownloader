using DesktopToast.Helper;
using Microsoft.Win32;
using System;

namespace DesktopToast
{
    /// <summary>
    /// Helper class for notification activator (for Action Center of Windows 10)
    /// </summary>
    public class NotificationHelper
    {
        /// <summary>
        /// Register COM server in the registry.
        /// </summary>
        /// <param name="activatorType">Notification activator type</param>
        /// <param name="executablePath">Executable file path</param>
        /// <param name="arguments">Arguments</param>
        /// <remarks>If the application is not running, this executable file will be started by COM.</remarks>
        public static void RegisterComServer(Type activatorType, string executablePath, string arguments = null)
        {
            CheckArgument(activatorType);

            if (string.IsNullOrWhiteSpace(executablePath))
            {
                throw new ArgumentNullException(nameof(executablePath));
            }

            if (!OsVersion.IsTenOrNewer)
            {
                return;
            }

            string combinedPath = $@"""{executablePath}"" {arguments}";
            string keyName = $@"SOFTWARE\Classes\CLSID\{{{activatorType.GUID}}}\LocalServer32";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName))
            {
                if (string.Equals(key?.GetValue(null) as string, combinedPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyName))
            {
                key.SetValue(null, combinedPath);
            }
        }

        /// <summary>
        /// Unregister COM server in the registry.
        /// </summary>
        /// <param name="activatorType">Notification activator type</param>
        public static void UnregisterComServer(Type activatorType)
        {
            CheckArgument(activatorType);

            string keyName = $@"SOFTWARE\Classes\CLSID\{{{activatorType.GUID}}}";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName))
            {
                if (key == null)
                {
                    return;
                }
            }
            Registry.CurrentUser.DeleteSubKeyTree(keyName);
        }

        internal static void CheckArgument(Type activatorType)
        {
            if (activatorType == null)
            {
                throw new ArgumentNullException(nameof(activatorType));
            }

            if (!activatorType.IsSubclassOf(typeof(NotificationActivatorBase)))
            {
                throw new ArgumentException($"{nameof(activatorType)} must inherit from {nameof(NotificationActivatorBase)}.");
            }
        }
    }
}