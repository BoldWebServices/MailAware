using System.Collections.Generic;
using MailAware.Utils.Config;
using MailAware.Utils.Services;
using StructureMap;

namespace MailAware.Console
{
    public class Program
    {
        public Program()
        {
            _config = new MailAwareConfig();
        }

        public static void Main(string[] args)
        {
            var program = new Program();
            program.Run();
        }

        public void Run()
        {
            // Setup DI container
            var container = new Container(c => c.Scan(scanner =>
            {
                scanner.AssembliesAndExecutablesFromApplicationBaseDirectory();
                scanner.WithDefaultConventions();
            }));


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
            
            container.Inject(_config.NotificationMailServer);

            // Setup monitors.
            var monitors = new List<IMailboxMonitor>();
            foreach (var target in _config.TargetMailServers)
            {
                var monitor = container.GetInstance<IMailboxMonitor>();
                monitor.StartMonitoring(target);
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

        private readonly MailAwareConfig _config;

        #endregion
    }
}