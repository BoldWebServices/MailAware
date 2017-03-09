using System;
using System.Threading;
using System.Threading.Tasks;
using MailAware.Utils.Config;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;

namespace MailAware.Utils.Services
{
    /// <summary>
    /// Monitors a
    /// </summary>
    public class MailboxMonitor : IMailboxMonitor
    {
        /// <summary>
        /// Initializes an instance of the <see cref="MailboxMonitor" /> class.
        /// </summary>
        /// <param name="alarmController">An alarm state controller.</param>
        public MailboxMonitor(IAlarmController alarmController)
        {
            if (alarmController == null)
            {
                throw new ArgumentNullException(nameof(alarmController));
            }

            _alarmController = alarmController;
        }

        /// <see cref="IMailboxMonitor.StartMonitoring" />
        public void StartMonitoring(TargetMailServer targetConfig)
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
            var thread = new Thread(Run);
            thread.Start();
        }

        /// <see cref="IMailboxMonitor.StopMonitoring" />
        public void StopMonitoring()
        {
            _running = false;
        }

        public async Task<bool> PurgeMatchingEmails()
        {
            if (!_running)
            {
                throw new InvalidOperationException("Not connected to the mailbox.");
            }

            try
            {
                // Search for matching emails.
                var query = SearchQuery.SubjectContains(_targetConfig.TargetSubjectSnippet);
                var searchResults = await _inbox.SearchAsync(query);

                Console.WriteLine("Found {0} matching emails for deletion.", searchResults.Count);

                // Flag for deletion and then expunge the mailbox.
                if (searchResults.Count > 0)
                {
                    await _inbox.SetFlagsAsync(searchResults, MessageFlags.Deleted, false);
                    await _inbox.ExpungeAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Encountered an exception while purging messages. Exception: {0}", e.Message);
                return false;
            }

            return true;
        }

        private void Run()
        {
            while (_running)
            {
                Console.WriteLine("{0} - Attempting to connect to the mail server...", DateTime.Now);

                bool connected = false;
                try
                {
                    // Connect to the IMAP server.
                    _imapClient.Connect(_targetConfig.HostAddress);

                    // Authorize.
                    _imapClient.Authenticate(_targetConfig.Username, _targetConfig.Password);

                    _inbox = _imapClient.Inbox;
                    _inbox.Open(FolderAccess.ReadWrite);
                    connected = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} - Failed to connect. Exception: {1}", DateTime.Now, e.Message);
                }

                if (connected)
                {
                    // If we were able to connect and open the mailbox successfully, reset the
                    // reconnect delay.
                    _currentReconnectDelaySecs = MailAwareConfig.ReconnectMinimumDelaySecs;

                    Console.WriteLine("Total messages: {0}", _inbox.Count);

                    // Delete matching emails to start off with a fresh state.
                    PurgeMatchingEmails().Wait();

                    // Reinitialize our alarm controller.
                    _alarmController.Initialize(_targetConfig.AlarmThresholdSecs);

                    while (_running)
                    {
                        // Setup the threshold to search and a query.
                        var alarmThreshold = DateTime.Now.AddSeconds(-_targetConfig.AlarmThresholdSecs);
                        Console.WriteLine("{0} - Searching for messages delivered after: {1}", DateTime.Now,
                            alarmThreshold);
                        var query =
                            SearchQuery.DeliveredAfter(alarmThreshold).And(
                                SearchQuery.SubjectContains(_targetConfig.TargetSubjectSnippet));

                        try
                        {
                            // These results are only narrowed down to our target day.
                            var searchResults = _inbox.Search(query);

                            if (searchResults.Count > 0)
                            {
                                Console.WriteLine("{0} - Found {1} target message(s).", DateTime.Now,
                                    searchResults.Count);

                                var message = _inbox.GetMessage(searchResults[searchResults.Count - 1]);
                                _alarmController.MessageSeen(message.Date.DateTime);
                                Console.WriteLine("Subject: {0}, sent date: {1}", message.Subject, message.Date);

                                PurgeMatchingEmails().Wait();
                            }

                            _alarmController.ProcessState().Wait();

                            _imapClient.NoOp();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(
                                "{0} - Failed to process mailbox messages. Exception: {1}",
                                DateTime.Now, e.Message);
                            break;
                        }

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

        #region Fields

        private bool _running;
        private TargetMailServer _targetConfig;
        private ImapClient _imapClient;
        private int _currentReconnectDelaySecs;
        private IMailFolder _inbox;
        private readonly IAlarmController _alarmController;

        #endregion
    }
}