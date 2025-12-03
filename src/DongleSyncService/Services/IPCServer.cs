using System.IO.Pipes;
using System.Text;
using Serilog;
using DongleSyncService.Utils;

namespace DongleSyncService.Services
{
    public class IPCServer : IDisposable
    {
        private NamedPipeServerStream _pipeServer;
        private Thread _listenerThread;
        private bool _isRunning;
        private readonly StateManager _stateManager;

        public IPCServer(StateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public void Start()
        {
            try
            {
                Log.Information("Starting IPC Server...");
                _isRunning = true;
                _listenerThread = new Thread(ListenForConnections);
                _listenerThread.IsBackground = true;
                _listenerThread.Start();
                Log.Information("IPC Server started successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start IPC Server");
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                _isRunning = false;
                _pipeServer?.Close();
                _listenerThread?.Join(1000);
                Log.Information("IPC Server stopped");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error stopping IPC Server");
            }
        }

        private void ListenForConnections()
        {
            while (_isRunning)
            {
                try
                {
                    _pipeServer = new NamedPipeServerStream(
                        Constants.PipeName,
                        PipeDirection.InOut,
                        1,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous
                    );

                    Log.Debug("IPC Server waiting for connection...");
                    _pipeServer.WaitForConnection();
                    Log.Debug("IPC Client connected");

                    HandleClient(_pipeServer);
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Log.Error(ex, "Error in IPC listener");
                        Thread.Sleep(1000); // Wait before retry
                    }
                }
                finally
                {
                    _pipeServer?.Dispose();
                }
            }
        }

        private void HandleClient(NamedPipeServerStream pipe)
        {
            try
            {
                // Read request
                var buffer = new byte[1024];
                var bytesRead = pipe.Read(buffer, 0, buffer.Length);
                var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Log.Debug("IPC Request: {Request}", request);

                // Process request
                var response = ProcessRequest(request);

                // Send response
                var responseBytes = Encoding.UTF8.GetBytes(response);
                pipe.Write(responseBytes, 0, responseBytes.Length);
                pipe.Flush();

                Log.Debug("IPC Response sent: {Response}", response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling IPC client");
            }
        }

        private string ProcessRequest(string request)
        {
            try
            {
                var parts = request.Split('|');
                var command = parts[0];

                switch (command)
                {
                    case "CHECK_STATUS":
                        return GetStatusResponse();

                    case "VALIDATE_BINDING":
                        if (parts.Length > 1)
                        {
                            var usbGuid = parts[1];
                            return ValidateBindingResponse(usbGuid);
                        }
                        return "ERROR|Invalid request format";

                    case "PING":
                        return "PONG";

                    default:
                        return "ERROR|Unknown command";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing IPC request");
                return $"ERROR|{ex.Message}";
            }
        }

        private string GetStatusResponse()
        {
            var state = _stateManager.GetState();
            return $"STATUS|IsPatched={state.IsPatched}|UsbGuid={state.UsbGuid}";
        }

        private string ValidateBindingResponse(string usbGuid)
        {
            var state = _stateManager.GetState();
            
            if (!state.IsPatched)
            {
                return "INVALID|Not patched";
            }

            if (state.UsbGuid != usbGuid)
            {
                return "INVALID|USB GUID mismatch";
            }

            return "VALID";
        }

        public void Dispose()
        {
            Stop();
            _pipeServer?.Dispose();
        }
    }
}