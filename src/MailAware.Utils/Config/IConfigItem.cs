namespace MailAware.Utils.Config
{
    /// <summary>
    /// Defines an interface for a configuration item.
    /// </summary>
    interface IConfigItem
    {
        /// <summary>
        /// Validates the configuration item.
        /// </summary>
        /// <returns>Whether or not the config item is valid.</returns>
        bool Validate();
    }
}
