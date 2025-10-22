using ConsoulLibrary;
using Microsoft.Extensions.Logging;
using Mtconnect;
using Mtconnect.AdapterSdk;
using Mtconnect.PCAdapter;

namespace TcpTerminal
{
    public static class Program
    {
        public const int SAMPLE_RATE = 2_000;
        private const string PUTTY_EXE = "C:\\Program Files\\PuTTY\\putty.exe";
        private const string CMD_EXE = "C:\\windows\\system32\\cmd.exe";
        private class AdapterLogger : IAdapterLogger
        {
            private readonly ILogger _logger;

            public AdapterLogger(ILogger logger)
            {
                _logger = logger;
            }

            public void LogDebug(string message, params object[] args)
                => _logger?.LogDebug(message, args);

            public void LogError(string message, params object[] args)
                => _logger?.LogError(message, args);

            public void LogError(Exception exception, string message, params object[] args)
                => _logger?.LogError(exception, message, args);

            public void LogInformation(string message, params object[] args)
                => _logger?.LogInformation(message, args);

            public void LogTrace(string message, params object[] args)
                => _logger?.LogTrace(message, args);

            public void LogWarning(string message, params object[] args)
                => _logger?.LogWarning(message, args);

            public void LogWarning(Exception exception, string message, params object[] args)
                => _logger?.LogWarning(exception, message, args);
        }

        public static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create((o) =>
            {
                o.SetMinimumLevel(LogLevel.Debug);
                o.AddConsoulLogger();

#if DEBUG
                o.SetMinimumLevel(LogLevel.Trace);
#endif
            });

            var options = new TcpAdapterOptions(port: 7875);
            
            options.UpdateFromConfig();

            using (var adapter = new TcpAdapter(options, new AdapterLogger(loggerFactory.CreateLogger<PCAdapterSource>())))
            {
                adapter.Start(new PCAdapterSource(SAMPLE_RATE));

                Consoul.Write($"Adapter running @ http://*:{adapter.Port}");

                if (File.Exists(PUTTY_EXE) && Consoul.Ask("Would you like to run PuTTY?"))
                    StartPuTTY(adapter.Port);

                Consoul.Wait();

                adapter.Stop();
            }
        }

        private static void StartPuTTY(int port)
        {
            using (var cmd = new System.Diagnostics.Process())
            {
                cmd.StartInfo.FileName = CMD_EXE;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.CreateNoWindow = true;

                cmd.Start();

                cmd.StandardInput.WriteLine($"\"{PUTTY_EXE}\" -raw -P {port} localhost");
            }
        }
    }
}