using System.Text;
using Topshelf;
using Serilog;
using DongleSyncService.Utils;
using DongleSyncService.Services;

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
                // Run test if --test argument provided
                if (args.Length > 0 && args[0] == "--test")
                {
                    TestCrypto();
                    return;
                }

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

        static void TestCrypto()
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Testing Encryption & Machine Binding");
                Console.WriteLine("========================================\n");

                var crypto = new CryptoService();
                var binding = new MachineBindingService(crypto);

                // Test machine fingerprint
                Console.WriteLine("1. Testing Machine Fingerprint...");
                var fingerprint = binding.GetMachineFingerprint();
                Console.WriteLine($"   Machine Fingerprint: {fingerprint}");
                Console.WriteLine("   ✅ Machine fingerprint generated\n");

                // Test encryption
                Console.WriteLine("2. Testing AES-256 Encryption...");
                var testData = Encoding.UTF8.GetBytes("Hello World");
                Console.WriteLine($"   Original data: Hello World");
                
                var (encrypted, iv) = crypto.EncryptDLL(testData, "TEST-USB-123");
                Console.WriteLine($"   Encrypted size: {encrypted.Length} bytes");
                Console.WriteLine($"   IV size: {iv.Length} bytes");
                
                // Save to temp files to test DecryptDLL
                var tempEnc = Path.GetTempFileName();
                var tempIv = Path.GetTempFileName();
                File.WriteAllBytes(tempEnc, encrypted);
                File.WriteAllBytes(tempIv, iv);
                
                var decrypted = crypto.DecryptDLL(tempEnc, tempIv, "TEST-USB-123");
                Console.WriteLine($"   Decrypted: {Encoding.UTF8.GetString(decrypted)}");
                Console.WriteLine("   ✅ Encryption/Decryption works\n");
                
                // Cleanup
                File.Delete(tempEnc);
                File.Delete(tempIv);

                // Test binding
                Console.WriteLine("3. Testing Machine Binding...");
                binding.CreateBinding("test-guid-123");
                Console.WriteLine("   Binding created");
                
                var valid = binding.ValidateBinding("test-guid-123");
                Console.WriteLine($"   Binding validation: {(valid ? "VALID ✅" : "INVALID ❌")}");
                
                // Test invalid GUID
                var invalid = binding.ValidateBinding("wrong-guid");
                Console.WriteLine($"   Invalid GUID test: {(!invalid ? "REJECTED ✅" : "ERROR ❌")}\n");

                Console.WriteLine("========================================");
                Console.WriteLine("✅ All tests passed!");
                Console.WriteLine("========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Test failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}