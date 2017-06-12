using System;
using System.Threading.Tasks;
using MailAware.Utils.Config;

namespace MailAware.Utils.Services
{
    /// <summary>
    /// A service that uses an SMTP server to send out notifications.
    /// </summary>
    public class NotificationService : INotificationService
    {
        #region Constants and Enums

        private const string AlarmMessage =
            "Mailbox: {0}\nCurrent state: Alarm\nMessage: The alarm threshold has been exceeded.";

        private const string NormalMessage =
            "Mailbox: {0}\nCurrent state: Normal\nMessage: The system is functioning properly.";

        #endregion

        /// <summary>
        /// Initializes an instance of the <see cref="NotificationService" /> class.
        /// </summary>
        /// <param name="mailer">The mailer to use for sending emails.</param>
        /// <param name="notificationConfig">The configuration settings for notifications.</param>
        public NotificationService(ISmtpMailer mailer, NotificationMailServer notificationConfig)
        {
            _mailer = mailer ?? throw new ArgumentNullException(nameof(mailer));
            _notificationConfig = notificationConfig ??
                                  throw new ArgumentNullException(nameof(notificationConfig));

            _mailer.IgnoreSslCertificates = _notificationConfig.IgnoreSslCertificates;
        }

        public async Task<bool> SendAlarmNotificationAsync(string mailboxName)
        {
            return await SendNotificationAsync(string.Format(AlarmMessage, mailboxName));
        }

        public async Task<bool> SendNormalNotificationAsync(string mailboxName)
        {
            return await SendNotificationAsync(string.Format(NormalMessage, mailboxName));
        }

        private async Task<bool> SendNotificationAsync(string message)
        {
            if (
                !await _mailer.ConnectAsync(_notificationConfig.HostAddress,
                    _notificationConfig.HostPort, _notificationConfig.Username,
                    _notificationConfig.Password))
            {
                return false;
            }

            var subject = string.Format("{0} Alarm Status Update", _notificationConfig.SubjectPrefix);
            if (
                !await _mailer.SendMessageAsync(subject, message, _notificationConfig.FromAddress,
                    _notificationConfig.Recipients))
            {
                return false;
            }

            await _mailer.DisconnectAsync();
            return true;
        }

        #region Fields

        private readonly ISmtpMailer _mailer;
        private readonly NotificationMailServer _notificationConfig;

        #endregion
    }
}
