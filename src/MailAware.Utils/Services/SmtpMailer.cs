using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace MailAware.Utils.Services
{
    /// <summary>
    /// Implementation of an SMTP mailer service.
    /// </summary>
    public class SmtpMailer : ISmtpMailer
    {
        #region Properties

        /// <see cref="ISmtpMailer.IgnoreSslCertificates" />
        public bool IgnoreSslCertificates { get; set; }

        #endregion

        /// <summary>
        /// Initializes an instance of the <see cref="SmtpMailer" /> class.
        /// </summary>
        public SmtpMailer()
        {
            _client = new SmtpClient();
        }

        /// <see cref="ISmtpMailer.ConnectAsync(string, int, string, string)" />
        public async Task<bool> ConnectAsync(string hostName, int hostPort = 0, string username = null,
            string password = null)
        {
            if (hostName == null)
            {
                throw new ArgumentNullException(nameof(hostName));
            }

            try
            {
                if (IgnoreSslCertificates)
                {
                    _client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                }
                else
                {
                    _client.ServerCertificateValidationCallback = null;
                }

                await _client.ConnectAsync(hostName, hostPort);

                if (!string.IsNullOrEmpty(username))
                {
                    await _client.AuthenticateAsync(username, password);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting: {0}", e.Message);
            }

            return false;
        }

        /// <see cref="ISmtpMailer.ConnectAsync(string, string, string)" />
        public async Task<bool> ConnectAsync(string hostName, string username = null,
            string password = null)
        {
            return await ConnectAsync(hostName, 0, username, password);
        }

        /// <see cref="ISmtpMailer.SendMessageAsync" />
        public async Task<bool> SendMessageAsync(string subject, string body, string fromAddress,
            string[] recipients)
        {
            if (fromAddress == null)
            {
                throw new ArgumentNullException(nameof(fromAddress));
            }

            if (recipients == null)
            {
                throw new ArgumentNullException(nameof(recipients));
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromAddress));

                foreach (var recipient in recipients)
                {
                    message.To.Add(new MailboxAddress(recipient));
                }

                message.Subject = subject;
                message.Body = new TextPart("plain")
                {
                    Text = body
                };

                await _client.SendAsync(message);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception sending message: {0}", e.Message);
            }

            return false;
        }

        /// <see cref="ISmtpMailer.DisconnectAsync" />
        public async Task<bool> DisconnectAsync()
        {
            try
            {
                await _client.DisconnectAsync(true);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception disconnecting: {0}", e.Message);
            }

            return false;
        }

        #region Fields

        private readonly SmtpClient _client;

        #endregion
    }
}