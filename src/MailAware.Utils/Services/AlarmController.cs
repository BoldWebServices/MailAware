using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MailAware.Utils.Config;
using NLog;

namespace MailAware.Utils.Services
{
    /// <summary>
    /// Implementation of an alarm state controller.
    /// </summary>
    public class AlarmController : IAlarmController
    {
        #region Constants and Enums

        private enum AlarmState
        {
            Uninitialized,
            Normal,
            AlarmThresholdExceeded
        }

        #endregion

        /// <summary>
        /// Initializes an instance of the <see cref="AlarmController" /> class.
        /// </summary>
        public AlarmController(INotificationService notificationService)
        {
            _notificationService = notificationService ??
                                   throw new ArgumentNullException(nameof(notificationService));
            _currentState = AlarmState.Uninitialized;
            _lastMessageReceivedDate = DateTime.MinValue;
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <see cref="IAlarmController.Initialize" />
        public void Initialize(TargetMailServer targetMailServer)
        {
            _targetMailServer = targetMailServer ??
                                throw new ArgumentNullException(nameof(targetMailServer));
            _currentState = AlarmState.Normal;
            _lastMessageReceivedDate = DateTime.Now;
        }

        /// <see cref="IAlarmController.MessageSeen" />
        public void MessageSeen(DateTime date)
        {
            if (_currentState == AlarmState.Uninitialized)
            {
                throw new InvalidOperationException("Alarm controller not initialized.");
            }

            _lastMessageReceivedDate = date;
        }

        /// <see cref="IAlarmController.ProcessState" />
        public async Task ProcessState()
        {
            if (_currentState == AlarmState.Uninitialized)
            {
                throw new InvalidOperationException("Alarm controller not initialized.");
            }

            // Handle transition to alarm state
            if (_currentState == AlarmState.Normal)
            {
                if (!WithinThreshold())
                {
                    await HandleStateChange(_currentState, AlarmState.AlarmThresholdExceeded);
                }
            }
            // Handle transition back to normal state
            else if (_currentState == AlarmState.AlarmThresholdExceeded)
            {
                if (WithinThreshold())
                {
                    await HandleStateChange(_currentState, AlarmState.Normal);
                }
            }
        }

        private async Task HandleStateChange(AlarmState oldState, AlarmState newState)
        {
            Debug.Assert(oldState != newState);
            if (oldState == AlarmState.Normal && newState == AlarmState.AlarmThresholdExceeded)
            {
                _logger.Info("Alarm state: Alarm Threshold Exceeded");
                await _notificationService.SendAlarmNotificationAsync(_targetMailServer.DisplayName);
            }
            if (oldState == AlarmState.AlarmThresholdExceeded && newState == AlarmState.Normal)
            {
                _logger.Info("Alarm state: Normal");
                await _notificationService.SendNormalNotificationAsync(_targetMailServer.DisplayName);
            }

            // Update the current state
            _currentState = newState;
        }

        private bool WithinThreshold()
        {
            return DateTime.Now > _lastMessageReceivedDate &&
                   DateTime.Now < _lastMessageReceivedDate.AddSeconds(_targetMailServer.AlarmThresholdSecs);
        }

        #region Fields

        private AlarmState _currentState;
        private DateTime _lastMessageReceivedDate;
        private TargetMailServer _targetMailServer;
        private readonly INotificationService _notificationService;
        private readonly Logger _logger;

        #endregion
    }
}
