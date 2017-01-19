using System;
using Newtonsoft.Json;

namespace MailAware.Utils.Config
{
    /// <summary>
    /// Configuration for the mail server to use for sending notifications.
    /// </summary>
    public class NotificationMailServer : MailServer, IConfigItem
    {
        #region Properties

        /// <summary>
        /// The prefix for all notification messages.
        /// </summary>
        /// <remarks>May be left blank.</remarks>
        [JsonProperty(PropertyName = "subjectPrefix")]
        public string SubjectPrefix { get; set; }

        /// <summary>
        /// The from address to send notifications from.
        /// </summary>
        [JsonProperty(PropertyName = "fromAddress")]
        public string FromAddress { get; set; }

        /// <summary>
        /// Recipients to send notifications to.
        /// </summary>
        [JsonProperty(PropertyName = "recipients")]
        public string[] Recipients { get; set; }

        #endregion

        /// <see cref="IConfigItem.Validate" />
        public bool Validate()
        {
            if (string.IsNullOrEmpty(FromAddress) ||
                Recipients == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
