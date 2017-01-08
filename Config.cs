using System.IO;
using System;
using Newtonsoft.Json;

namespace ConsoleApplication
{
	/// <summary>
	/// Configuration data.
	/// </summary>
	public class Config
	{
		#region Constants and Enums

		/// <summary>
		/// The name of the configuration file.
		/// </summary>
		public const string ConfigFileName = "config.json";

		#endregion

		#region Properties

		/// <summary>
		/// Host address for the mail server to monitor.
		/// </summary>
		[JsonProperty(PropertyName = "mailServerAddress")]
		public string MailServerAddress { get; set; }

		/// <summary>
		/// Mailbox username.
		/// </summary>
		[JsonProperty(PropertyName = "username")]
		public string Username { get; set; }

		/// <summary>
		/// Mailbox password.
		/// </summary>
		[JsonProperty(PropertyName = "password")]
		public string Password { get; set; }

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

		public Config()
		{

		}

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
				Console.WriteLine("Exception: {0}", e.Message);
			}

			return false;
		}

		public bool Validate()
		{
			if (string.IsNullOrEmpty(MailServerAddress) ||
				AlarmThresholdSecs <= 0 ||
				PollingFrequencyMs <= 0)
			{
				return false;
			}

			return true;
		}
	}
}