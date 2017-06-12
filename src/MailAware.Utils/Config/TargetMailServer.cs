using Newtonsoft.Json;

namespace MailAware.Utils.Config
{
    /// <summary>
    /// A target mail server to monitor.
    /// </summary>
    public class TargetMailServer : MailServer, IConfigItem
    {
        #region Constants and Enums

        private const int DefaultAlarmThresholdSecs = 1800;
        private const int DefaultPollingFrequencyMs = 10000;

        #endregion

        #region Properties

        /// <summary>
        /// The target subject snippet to monitor for. This is a portion of the subject that
        /// should be contained within the target messages.
        /// </summary>
        [JsonProperty(PropertyName = "targetSubjectSnippet")]
        public string TargetSubjectSnippet { get; set; }

        /// <summary>
        /// Threshold in seconds to allow until a no email alarm is triggered.
        /// </summary>
        [JsonProperty(PropertyName = "alarmThresholdSecs")]
        public int AlarmThresholdSecs { get; set; }

        /// <summary>
        /// How often to poll the mailbox.
        /// </summary>
        [JsonProperty(PropertyName = "pollingFrequencyMs")]
        public int PollingFrequencyMs { get; set; }

        /// <summary>
        /// Display name used for alarm notifications.
        /// </summary>
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TargetMailServer()
        {
            AlarmThresholdSecs = DefaultAlarmThresholdSecs;
            PollingFrequencyMs = DefaultPollingFrequencyMs;
        }

        /// <see cref="IConfigItem.Validate" />
        public bool Validate()
        {
            return AlarmThresholdSecs > 0 && PollingFrequencyMs > 1000 &&
                   !string.IsNullOrEmpty(DisplayName);
        }
    }
}