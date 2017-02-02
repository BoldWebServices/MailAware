using System.Threading.Tasks;
using MailAware.Utils.Config;

namespace MailAware.Utils.Services
{
    /// <summary>
    /// Defines an interface for monitoring a mailbox.
    /// </summary>
    public interface IMailboxMonitor
    {
        /// <summary>
        /// Connects to the mailbox and starts to monitor it.
        /// </summary>
        /// <param name="targetConfig">The target mailbox config.</param>
        /// <param name="notificationConfig">The notification email config.</param>
        void StartMonitoring(TargetMailServer targetConfig, NotificationMailServer notificationConfig);

        /// <summary>
        /// Shuts down all communication with the mailbox and ceases to monitor it.
        /// </summary>
        void StopMonitoring();

        /// <summary>
        /// Deletes and purges emails matching the target subject filter.
        /// </summary>
        /// <returns>Whether or not the operation was successful.</returns>
        Task<bool> PurgeMatchingEmails();
    }
}