using MailAware.Utils.Config;
using System.Collections.Generic;
using MailAware.Utils.Services;

namespace MailAware.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var program = new Program();
            program.Run();
        }

        public Program()
        {
            _config = new MailAwareConfig();
        }

        public void Run()
        {
            if (!_config.ReadConfig())
            {
                System.Console.WriteLine("Failed to read configuration file.");
                return;
            }

            if (!_config.Validate())
            {
                System.Console.WriteLine("Configuration file invalid.");
                return;
            }

            // Setup monitors.
            var monitors = new List<IMailboxMonitor>();
            foreach (var target in _config.TargetMailServers)
            {
                var monitor = new MailboxMonitor();
                monitor.StartMonitoring(target, _config.NotificationMailServer);
                monitors.Add(monitor);
            }

            System.Console.WriteLine("Started {0} monitor(s).", monitors.Count);
            System.Console.WriteLine("Press \"q\" to quit...");
            
            bool running = true;
            while (running)
            {
                string input = System.Console.ReadLine();
                if (input.Equals("q"))
                {
                    running = false;
                }
            }

            System.Console.WriteLine("Stopping monitors.");
            monitors.ForEach(monitor => monitor.StopMonitoring());
        }

        #region Fields

        private MailAwareConfig _config;

        #endregion
    }
}
