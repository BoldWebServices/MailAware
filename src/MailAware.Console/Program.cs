using System;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailAware.Utils.Config;
using System.Collections;
using System.Collections.Generic;

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
                    await _client.AuthenticateAsync(_config.TargetMailServer.Username,
                        _config.TargetMailServer.Password);

                    var inbox = _client.Inbox;
                    await inbox.OpenAsync(FolderAccess.ReadOnly);

                    // If we were able to connect and open the mailbox successfully, reset the
                    // reconnect delay.
                    _currentReconnectDelaySecs = MailAwareConfig.ReconnectMinimumDelaySecs;

                    System.Console.WriteLine("Total messages: {0}", inbox.Count);
                    System.Console.WriteLine("Recent messages: {0}", inbox.Recent);

                    while (true)
                    {
                        // Setup the threshold to search and a query
                        var alarmThreshold = DateTime.Now.AddSeconds(-_config.AlarmThresholdSecs);
                        System.Console.WriteLine("{0} - Searching for messages delivered after: {1}",
                            DateTime.Now, alarmThreshold);
                        var query = SearchQuery.DeliveredAfter(alarmThreshold).And(
                            SearchQuery.SubjectContains(_config.TargetSubjectSnippet));

                        // These results are only narrowed down to our target day, we need to
                        // manually filter down to the time.
                        var searchResults = await inbox.SearchAsync(query);
                        var filteredResults = await NarrowDown(inbox, searchResults, alarmThreshold);

                        if (filteredResults.Count > 0)
                        {
                            System.Console.WriteLine("{0} - System is alive. Found {1} target message(s).",
                                DateTime.Now, filteredResults.Count);
                        }

                        foreach (var message in filteredResults)
                        {
                            System.Console.WriteLine("Subject: {0}, received date: {1}", message.Subject,
                                message.Date);
                        }

                        await _client.NoOpAsync();

                        await Task.Delay(_config.PollingFrequencyMs);
                    }

                }
                catch (Exception e)
                {
                    System.Console.WriteLine("{0} - Exception: {1}\n", DateTime.Now, e.Message);
                }
                finally
                {
                    if (_client.IsConnected)
                    {
                        await _client.DisconnectAsync(true);
                    }
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

        private async Task<IList<MimeKit.MimeMessage>> NarrowDown(IMailFolder inbox, IList<UniqueId> uids, DateTime threshold)
        {
            var results = new List<MimeKit.MimeMessage>();

            foreach (var uid in uids)
            {
                var message = await inbox.GetMessageAsync(uid);
                if (message.Date.DateTime > threshold)
                {
                    results.Add(message);
                }
            }

            return results;
        }

        #region Fields

        private MailAwareConfig _config;
        private ImapClient _client;
        private int _currentReconnectDelaySecs;

        #endregion
    }
}
