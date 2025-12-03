using System.IO.Pipes;
using System.Text;
using Serilog;

namespace DLLPatch
{
    public class IPCClient
    {
        private const string PipeName = "DongleSyncPipe";
        private const int Timeout = 5000;

        public static string SendCommand(string command)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
                
                client.Connect(Timeout);
                
                // Send command
                var requestBytes = Encoding.UTF8.GetBytes(command);
                client.Write(requestBytes, 0, requestBytes.Length);
                client.Flush();

                // Read response
                var buffer = new byte[1024];
                var bytesRead = client.Read(buffer, 0, buffer.Length);
                var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                return response;
            }
            catch (TimeoutException)
            {
                Log.Warning("IPC timeout - Service may not be running");
                return "ERROR|Timeout";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "IPC communication error");
                return $"ERROR|{ex.Message}";
            }
        }

        public static bool CheckServiceStatus()
        {
            var response = SendCommand("CHECK_STATUS");
            return response.StartsWith("STATUS|");
        }

        public static bool ValidateBinding(string usbGuid)
        {
            var response = SendCommand($"VALIDATE_BINDING|{usbGuid}");
            return response == "VALID";
        }

        public static bool Ping()
        {
            var response = SendCommand("PING");
            return response == "PONG";
        }
    }
}