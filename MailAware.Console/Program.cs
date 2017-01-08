using System;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailAware.Utils.Config;

namespace MailAware.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var program = new Program();
            program.Run().Wait();
        }

        public Program()
        {
            _config = new MailAwareConfig();
            _client = new ImapClient();
            _client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        }

        public async Task Run()
        {
            System.Console.WriteLine("Hello World!");

            if (!_config.ReadConfig())
            {
                System.Console.WriteLine("Failed to read configuration file.");
                return;
            }

            if (!_config.Validate())
            {
                System.Console.WriteLine("Configuration file invalid.");
                return;
            }

            while (true)
            {
                try
                {
                    // Connect to the IMAP server
                    await _client.ConnectAsync(_config.MailServerAddress);

                    // Authorize
                    await _client.AuthenticateAsync(_config.Username, _config.Password);

                    var inbox = _client.Inbox;
                    await inbox.OpenAsync(FolderAccess.ReadOnly);

                    System.Console.WriteLine("Total messages: {0}", inbox.Count);
                    System.Console.WriteLine("Recent messages: {0}", inbox.Recent);

                    while (true)
                    {
                        for (int i = 0; i < inbox.Count; i++)
                        {
                            var message = inbox.GetMessage(i);
                            System.Console.WriteLine("Subject: {0}", message.Subject);
                        }

                        await _client.NoOpAsync();

                        await Task.Delay(_config.PollingFrequencyMs);
                    }

                    await _client.DisconnectAsync(true);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Exception: {0}", e.Message);
                }
            }
        }

        #region Fields

        private MailAwareConfig _config;
        private ImapClient _client;

        #endregion
    }
}
