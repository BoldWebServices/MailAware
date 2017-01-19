using System;
using System.Threading;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailAware.Utils.Config;
using System.Collections.Generic;

namespace MailAware.Utils.Services
{
    /// <summary>
    /// Monitors a 
    /// </summary>
    public class MailboxMonitor : IMailboxMonitor
    {
        /// <see cref="IMailboxMonitor.StartMonitoring() "/>
        public void StartMonitoring(TargetMailServer targetConfig,
            NotificationMailServer notificationConfig)
        {
            if (_running)
            {
                throw new InvalidOperationException("Already monitoring the mailbox.");
            }

            _running = true;
            _imapClient = new ImapClient();
            _imapClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
            _currentReconnectDelaySecs = MailAwareConfig.ReconnectMinimumDelaySecs;
            _targetConfig = targetConfig;
            _notificationConfig = notificationConfig;
            var thread = new Thread(() => Run());
            thread.Start();
        }

        /// <see cref="IMailboxMonitor.StopMonitoring" />
        public void StopMonitoring()
        {
            _running = false;
        }

        private void Run()
        {
            while (_running)
            {
                Console.WriteLine("{0} - Attempting to connect to the mail server...",
                    DateTime.Now);

                bool connected = false;
                try
                {
                    // Connect to the IMAP server.
                    _imapClient.Connect(_targetConfig.HostAddress);

                    // Authorize.
                    _imapClient.Authenticate(_targetConfig.Username, _targetConfig.Password);

                    _inbox = _imapClient.Inbox;
                    _inbox.Open(FolderAccess.ReadOnly);
                    connected = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} - Failed to connect. Exception: {1}",
                        DateTime.Now, e.Message);
                }

                if (connected)
                {

                    // If we were able to connect and open the mailbox successfully, reset the
                    // reconnect delay.
                    _currentReconnectDelaySecs = MailAwareConfig.ReconnectMinimumDelaySecs;

                    Console.WriteLine("Total messages: {0}", _inbox.Count);
                    Console.WriteLine("Recent messages: {0}", _inbox.Recent);

                    while (_running)
                    {
                        // Setup the threshold to search and a query.
                        var alarmThreshold = DateTime.Now.AddSeconds(-_targetConfig.AlarmThresholdSecs);
                        Console.WriteLine("{0} - Searching for messages delivered after: {1}",
                            DateTime.Now, alarmThreshold);
                        var query = SearchQuery.DeliveredAfter(alarmThreshold).And(
                            SearchQuery.SubjectContains(_targetConfig.TargetSubjectSnippet));

                        // These results are only narrowed down to our target day, we need to
                        // manually filter down to the time.
                        var searchResults = _inbox.Search(query);
                        var filteredResults = NarrowDown(searchResults, alarmThreshold);

                        if (filteredResults.Count > 0)
                        {
                            Console.WriteLine("{0} - System is alive. Found {1} target message(s).",
                                DateTime.Now, filteredResults.Count);
                        }

                        foreach (var message in filteredResults)
                        {
                            Console.WriteLine("Subject: {0}, sent date: {1}", message.Subject,
                                message.Date);
                        }

                        _imapClient.NoOp();

                        Thread.Sleep(_targetConfig.PollingFrequencyMs);
                    } 
                }

                // Make sure we cleanup.
                if (_inbox.IsOpen)
                {
                    _inbox.Close();
                }
                if (_imapClient.IsConnected)
                {
                    Console.WriteLine("Disconnecting from mail server.");
                    _imapClient.Disconnect(true);
                }

                // Double the reconnect delay.
                _currentReconnectDelaySecs *= 2;
                if (_currentReconnectDelaySecs > MailAwareConfig.ReconnectMaximumDelaySecs)
                {
                    _currentReconnectDelaySecs = MailAwareConfig.ReconnectMaximumDelaySecs;
                }

                // If we got here and are running, something bad happened. Sleep for a bit,
                // until the next reconnect.
                if (_running)
                {
                    Thread.Sleep(_currentReconnectDelaySecs * 1000);
                }
            }
        }

        private IList<MimeKit.MimeMessage> NarrowDown(IList<UniqueId> uids, DateTime threshold)
        {
            var results = new List<MimeKit.MimeMessage>();

            foreach (var uid in uids)
            {
                var message = _inbox.GetMessage(uid);
                if (message.Date.DateTime > threshold)
                {
                    results.Add(message);
                }
            }

            return results;
        }

        #region Fields

        private bool _running;
        private TargetMailServer _targetConfig;
        private NotificationMailServer _notificationConfig;
        private ImapClient _imapClient;
        private int _currentReconnectDelaySecs;
        private IMailFolder _inbox;

        #endregion
    }
}
