using System.Threading.Tasks;

namespace MailAware.Utils.Services
{
    /// <summary>
    /// An interface for a service that notifies receipients of alarm conditions.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends a notification that an alarm condition was encountered.
        /// </summary>
        /// <param name="mailboxName">The display name of the mailbox.</param>
        /// <returns>Whether or not the notification was sent.</returns>
        Task<bool> SendAlarmNotificationAsync(string mailboxName);

        /// <summary>
        /// Sends a notification that a normal condition was encountered.
        /// </summary>
        /// <param name="mailboxName">The display name of the mailbox.</param>
        /// <returns>Whether or not the notification was sent.</returns>
        Task<bool> SendNormalNotificationAsync(string mailboxName);
    }
}
