using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace MailAware.Utils.Services
{
    public class SmtpMailer : ISmtpMailer
    {
        #region Properties

        public bool IgnoreSslCertificates { get; set; }

        #endregion

        public SmtpMailer()
        {
            _client = new SmtpClient();
        }

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

        public async Task<bool> ConnectAsync(string hostName, string username = null,
            string password = null)
        {
            return await ConnectAsync(hostName, 0, username, password);
        }

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