using System.Threading.Tasks;

namespace MailAware.Utils.Services
{
    /// <summary>
    /// A service for sending quick notification messages via SMTP.
    /// </summary>
    public interface ISmtpMailer
    {
        #region Properties

        /// <summary>
        /// Whether or not to ignore SSL certificate validation.
        /// </summary>
        bool IgnoreSslCertificates { get; set; }

        #endregion

        /// <summary>
        /// Connects to a SMTP server.
        /// </summary>
        /// <param name="hostName">The SMTP host name.</param>
        /// <param name="hostPort">The SMTP host port. 0 to use protocol default.</param>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <returns>Whether or not connection succeeded.</returns>
        Task<bool> ConnectAsync(string hostName, int hostPort = 0, string username = null,
            string password = null);

        /// <summary>
        /// Connects to a SMTP server.
        /// </summary>
        /// <param name="hostName">The SMTP host name.</param>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <returns>Whether or not connection succeeded.</returns>
        Task<bool> ConnectAsync(string hostName, string username = null, string password = null);

        /// <summary>
        /// Sends a message to a set of recipients.
        /// </summary>
        /// <param name="subject">The message subject.</param>
        /// <param name="body">The message body as plain text.</param>
        /// <param name="fromAddress">The sender address.</param>
        /// <param name="recipients">The set of recipients to send the message to.</param>
        /// <returns>Whether or not sending succeeded.</returns>
        Task<bool> SendMessageAsync(string subject, string body, string fromAddress, string[] recipients);

        /// <summary>
        /// Disconnects from the SMTP server.
        /// </summary>
        /// <returns>Whether or not disconnecting succeeded.</returns>
        Task<bool> DisconnectAsync();
    }
}