using Topshelf;
using Serilog;
using DongleSyncService.Utils;

namespace DongleSyncService
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    Path.Combine(Constants.LogFolder, "service-.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                var exitCode = HostFactory.Run(config =>
                {
                    config.Service<DongleService>(service =>
                    {
                        service.ConstructUsing(name => new DongleService());
                        service.WhenStarted(s => s.Start());
                        service.WhenStopped(s => s.Stop());
                    });

                    config.RunAsLocalSystem();
                    config.SetServiceName(Constants.ServiceName);
                    config.SetDisplayName(Constants.ServiceDisplayName);
                    config.SetDescription("Manages USB dongle for App X licensing");
                    config.StartAutomatically();
                    
                    config.EnableServiceRecovery(recovery =>
                    {
                        recovery.RestartService(1);
                    });

                    config.UseSerilog();
                });

                Environment.ExitCode = (int)exitCode;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Service terminated unexpectedly");
                Environment.ExitCode = 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}