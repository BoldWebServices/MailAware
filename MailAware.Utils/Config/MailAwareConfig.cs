using System;
using System.IO;
using Newtonsoft.Json;

namespace MailAware.Utils.Config
{
	/// <summary>
	/// Configuration data.
	/// </summary>
	public class MailAwareConfig
	{
		#region Constants and Enums

		/// <summary>
		/// The name of the configuration file.
		/// </summary>
		public const string ConfigFileName = "config.json";
        
	    private const int DefaultAlarmThresholdSecs = 1600;
	    private const int DefaultPollingFrequencyMs = 10000;

		#endregion

		#region Properties

        /// <summary>
        /// The target mail server config.
        /// </summary>
        [JsonProperty(PropertyName = "targetMailServer")]
		public MailServer TargetMailServer { get; set; }

        /// <summary>
        /// The mail server used for sending notifications config.
        /// </summary>
        [JsonProperty(PropertyName = "notificationMailServer")]
        public NotificationMailServer NotificationMailServer { get; set; }

        /// <summary>
        /// The target subject prefix to monitor for.
        /// </summary>
        public string TargetSubjectPrefix { get; set; }

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

		#endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
		public MailAwareConfig()
		{
		    AlarmThresholdSecs = DefaultAlarmThresholdSecs;
		    PollingFrequencyMs = DefaultPollingFrequencyMs;
		}

        /// <summary>
        /// Reads the configuration.
        /// </summary>
        /// <returns>Whether or not reading the config succeeded.</returns>
		public bool ReadConfig()
		{
			try
			{
				var json = File.ReadAllText(ConfigFileName);
				JsonConvert.PopulateObject(json, this);
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception reading config: {0}", e.Message);
			}

			return false;
		}

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        /// <returns>Whether or not the config is valid.</returns>
		public bool Validate()
		{
			if (TargetMailServer == null ||
                NotificationMailServer == null ||
				AlarmThresholdSecs <= 0 ||
				PollingFrequencyMs <= 0)
			{
				return false;
			}

			return true;
		}
	}
}