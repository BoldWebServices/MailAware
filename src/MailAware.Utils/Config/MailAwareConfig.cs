using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MailAware.Utils.Config
{
    /// <summary>
    /// Configuration data.
    /// </summary>
    public class MailAwareConfig : IConfigItem
    {
        #region Constants and Enums

        /// <summary>
        /// The name of the configuration file.
        /// </summary>
        public const string ConfigFileName = "config.json";

        /// <summary>
        /// The minimum reconnect delay for connecting to servers.
        /// </summary>
        public const int ReconnectMinimumDelaySecs = 3;

        /// <summary>
        /// The maximum reconnect delay for connecting to servers.
        /// </summary>
        public const int ReconnectMaximumDelaySecs = 300;

        #endregion

        #region Properties

        /// <summary>
        /// The target mail server configs.
        /// </summary>
        [JsonProperty(PropertyName = "targetMailServers")]
        public TargetMailServer[] TargetMailServers { get; set; }

        /// <summary>
        /// The mail server used for sending notifications config.
        /// </summary>
        [JsonProperty(PropertyName = "notificationMailServer")]
        public NotificationMailServer NotificationMailServer { get; set; }

        #endregion

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
            if (TargetMailServers == null || NotificationMailServer == null)
            {
                return false;
            }

            // Validate sub items.
            var targets = new List<TargetMailServer>(TargetMailServers);
            if (!NotificationMailServer.Validate() || targets.Any(target => !target.Validate()))
            {
                return false;
            }

            return true;
        }
    }
}