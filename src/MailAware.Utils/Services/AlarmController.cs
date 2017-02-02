using System;
using System.Threading.Tasks;

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

        public AlarmController()
        {
            _currentState = AlarmState.Uninitialized;
            _lastMessageReceivedDate = DateTime.MinValue;
        }

        /// <see cref="IAlarmController.Initialize" />
        public void Initialize(int alarmThresholdSecs)
        {
            _alarmThresholdSecs = alarmThresholdSecs;
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
        }

        /// <see cref="IAlarmController.MessageSeen" />
        public async Task ProcessState()
        {
            if (_currentState == AlarmState.Uninitialized)
            {
                throw new InvalidOperationException("Alarm controller not initialized.");
            }
        }

        #region Fields

        private AlarmState _currentState;
        private DateTime _lastMessageReceivedDate;
        private int _alarmThresholdSecs;

        #endregion
    }
}
