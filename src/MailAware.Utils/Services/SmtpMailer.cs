using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using NLog;

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
            _logger = LogManager.GetCurrentClassLogger();
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
                _logger.Error($"Failed to connect to the SMTP server: {hostName}");
                _logger.Error(e);
            }

            return false;
        }

        /// <see cref="ISmtpMailer.ConnectAsync(string, string, string)" />
        public async Task<bool> ConnectAsync(string hostName, string username = null,
            string password = null)
        {
            return await ConnectAsync(hostName, 0, username, password);
        }

        /// <see cref="ISmtpMailer.SendMessageAsync(string, string, string, string[])" />
        public async Task<bool> SendMessageAsync(string subject, string body, string fromAddress,
            string[] recipients)
        {
            var tasks = new List<Task<bool>>();
            foreach (var recipient in recipients)
            {
                tasks.Add(SendMessageAsync(subject, body, fromAddress, recipient));
            }

            var results = await Task.WhenAll(tasks);

            return !results.Any(result => !result);
        }


        /// <see cref="ISmtpMailer.SendMessageAsync(string, string, string, string)" />
        public async Task<bool> SendMessageAsync(string subject, string body, string fromAddress,
            string recipient)
        {

            if (fromAddress == null)
            {
                throw new ArgumentNullException(nameof(fromAddress));
            }

            if (recipient == null)
            {
                throw new ArgumentNullException(nameof(recipient));
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromAddress));
                message.To.Add(new MailboxAddress(recipient));

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
                _logger.Error("Failed to send email message.");
                _logger.Error(e);
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
                _logger.Error("Failed to disconnect from the SMTP server.");
                _logger.Error(e);
            }

            return false;
        }

        #region Fields

        private readonly SmtpClient _client;
        private readonly Logger _logger;

        #endregion
    }
}