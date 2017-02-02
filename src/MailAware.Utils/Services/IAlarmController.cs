using System;
using System.Threading.Tasks;

namespace MailAware.Utils.Services
{
    /// <summary>
    /// Interface for an alarm state controller.
    /// </summary>
    public interface IAlarmController
    {
        /// <summary>
        /// Sets up the alarm controller to be in a good non-alarmed state.
        /// </summary>
        /// <param name="alarmThresholdSecs">The threshold in seconds for signaling an alarm state.</param>
        void Initialize(int alarmThresholdSecs);

        /// <summary>
        /// Processes a message being received at a particular date.
        /// </summary>
        /// <param name="date">The date and time that the message was received.</param>
        void MessageSeen(DateTime date);

        /// <summary>
        /// Handles alarm state management, sending notifications as necessary.
        /// </summary>
        Task ProcessState();
    }
}
