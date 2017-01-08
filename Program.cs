using System;
using System.Threading.Tasks;
using MailKit.Net.Imap;
using MailKit;

namespace ConsoleApplication
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
            _config = new Config();
            _client = new ImapClient();
            _client.ServerCertificateValidationCallback = (s, c, h, e) => true;
        }

        public async Task Run()
        {
            Console.WriteLine("Hello World!");

            if (!_config.ReadConfig())
            {
                Console.WriteLine("Failed to read configuration file.");
                return;
            }

            if (!_config.Validate())
            {
                Console.WriteLine("Configuration file invalid.");
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

                    Console.WriteLine("Total messages: {0}", inbox.Count);
                    Console.WriteLine("Recent messages: {0}", inbox.Recent);

                    while (true)
                    {
                        for (int i = 0; i < inbox.Count; i++)
                        {
                            var message = inbox.GetMessage(i);
                            Console.WriteLine("Subject: {0}", message.Subject);
                        }

                        await _client.NoOpAsync();

                        await Task.Delay(_config.PollingFrequencyMs);
                    }

                    await _client.DisconnectAsync(true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e.Message);
                }
            }
        }

        #region Fields

        private Config _config;
        private ImapClient _client;

        #endregion
    }
}
