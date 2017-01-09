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
            _currentReconnectDelaySecs = MailAwareConfig.ReconnectMinimumDelaySecs;
        }

        public async Task Run()
        {
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
                    System.Console.WriteLine("{0} - Attempting to connect to the mail server...", DateTime.Now);

                    // Connect to the IMAP server.
                    await _client.ConnectAsync(_config.TargetMailServer.HostAddress);

                    // Authorize.
                    await _client.AuthenticateAsync(_config.TargetMailServer.Username, _config.TargetMailServer.Password);

                    var inbox = _client.Inbox;
                    await inbox.OpenAsync(FolderAccess.ReadOnly);

                    // If we were able to connect and open the mailbox successfully, reset the
                    // reconnect delay.
                    _currentReconnectDelaySecs = MailAwareConfig.ReconnectMinimumDelaySecs;

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
                    System.Console.WriteLine("{0} - Exception: {1}\n", DateTime.Now, e.Message);
                }

                await Task.Delay(_currentReconnectDelaySecs * 1000);

                // Double the reconnect delay
                _currentReconnectDelaySecs *= 2;
                if (_currentReconnectDelaySecs > MailAwareConfig.ReconnectMaximumDelaySecs)
                {
                    _currentReconnectDelaySecs = MailAwareConfig.ReconnectMaximumDelaySecs;
                }
            }
        }

        #region Fields

        private MailAwareConfig _config;
        private ImapClient _client;
        private int _currentReconnectDelaySecs;

        #endregion
    }
}
