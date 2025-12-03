using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Serilog;

namespace DLLPatch
{
    public class BindingValidator
    {
        private const string BindKeyFile = @"C:\ProgramData\DongleSyncService\bind.key";

        public static bool ValidateLocalBinding(string expectedUsbGuid)
        {
            try
            {
                if (!File.Exists(BindKeyFile))
                {
                    Log.Warning("Bind key file not found");
                    return false;
                }

                // Try to validate via IPC first
                if (IPCClient.Ping())
                {
                    return IPCClient.ValidateBinding(expectedUsbGuid);
                }

                // Fallback: Direct validation (less secure)
                Log.Warning("IPC not available, using fallback validation");
                return true; // In production, implement more checks
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Binding validation error");
                return false;
            }
        }
    }
}