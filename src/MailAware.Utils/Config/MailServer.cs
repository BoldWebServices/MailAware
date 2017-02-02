using Newtonsoft.Json;

namespace MailAware.Utils.Config
{
    /// <summary>
    /// Configuration related to connecting to a mail server.
    /// </summary>
    public class MailServer
    {
        #region Properties

        /// <summary>
        /// Whether or not to ignore SSL certificate validation.
        /// </summary>
        [JsonProperty(PropertyName = "ignoreSslCertificates")]
        public bool IgnoreSslCertificates { get; set; }

        /// <summary>
        /// Host address for the mail server.
        /// </summary>
        [JsonProperty(PropertyName = "hostAddress")]
        public string HostAddress { get; set; }

        /// <summary>
        /// Host port for the mail server.
        /// </summary>
        /// <remarks>Can be left empty to utilize the default.</remarks>
        [JsonProperty(PropertyName = "hostPort")]
        public int HostPort { get; set; }

        /// <summary>
        /// Mailbox username.
        /// </summary>
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        /// <summary>
        /// Mailbox password.
        /// </summary>
        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MailServer()
        {
            IgnoreSslCertificates = false;
            HostPort = 0;
        }
    }
}