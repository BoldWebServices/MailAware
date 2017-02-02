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
            "The alarm threshold has been exceeded. Current state: Alarm.";

        private const string NormalMessage =
            "The system is functioning properly. Current state: Normal.";

        #endregion

        /// <summary>
        /// Initializes an instance of the <see cref="NotificationService" /> class.
        /// </summary>
        /// <param name="mailer">The mailer to use for sending emails.</param>
        /// <param name="notificationConfig">The configuration settings for notifications.</param>
        public NotificationService(ISmtpMailer mailer, NotificationMailServer notificationConfig)
        {
            if (mailer == null)
            {
                throw new ArgumentNullException(nameof(mailer));
            }
            if (notificationConfig == null)
            {
                throw new ArgumentNullException(nameof(notificationConfig));
            }

            _mailer = mailer;
            _notificationConfig = notificationConfig;

            _mailer.IgnoreSslCertificates = _notificationConfig.IgnoreSslCertificates;
        }

        public async Task<bool> SendAlarmNotificationAsync()
        {
            return await SendNotificationAsync(AlarmMessage);
        }

        public async Task<bool> SendNormalNotificationAsync()
        {
            return await SendNotificationAsync(NormalMessage);
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
