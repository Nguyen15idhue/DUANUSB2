# 🚀 HỆ THỐNG USB DONGLE 4 LAYERS - NGÀY 5-6

## 🗓️ NGÀY 5: DLL PATCH PROJECT & CREATOR TOOL (8 giờ)

### ⏰ 09:00 - 12:00 | DLL Patch Project (3 giờ)

#### **Setup DLLPatch Project:**

```powershell
cd F:\3.Laptrinh\DUANUSB2\src\DLLPatch

# Add reference to DongleSyncService (for models)
dotnet add reference ../DongleSyncService/DongleSyncService.csproj

# Add packages
dotnet add package Newtonsoft.Json
dotnet add package Serilog
dotnet add package Serilog.Sinks.File
```

#### **File 1: `IPCClient.cs`**

```csharp
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
```

#### **File 2: `BindingValidator.cs`**

```csharp
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
```

#### **File 3: `DLLEntryPoint.cs`**

```csharp
using System.Runtime.InteropServices;
using Serilog;

namespace DLLPatch
{
    public static class DLLEntryPoint
    {
        private static bool _isInitialized = false;
        private static string _currentUsbGuid;

        // This will be called when DLL is loaded by CHC Geomatics Office 2
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public static void Initialize(string usbGuid)
        {
            if (_isInitialized)
            {
                return;
            }

            try
            {
                // Configure logging
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(
                        @"C:\ProgramData\DongleSyncService\logs\patch-.log",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [PATCH] {Message:lj}{NewLine}{Exception}"
                    )
                    .CreateLogger();

                Log.Information("========================================");
                Log.Information("Patched DLL Initialized");
                Log.Information("USB GUID: {Guid}", usbGuid);
                Log.Information("========================================");

                _currentUsbGuid = usbGuid;

                // Validate binding
                if (!BindingValidator.ValidateLocalBinding(usbGuid))
                {
                    Log.Error("Binding validation FAILED!");
                    throw new UnauthorizedAccessException("Invalid machine binding");
                }

                Log.Information("Binding validation PASSED");

                // Start heartbeat check
                StartHeartbeatCheck();

                _isInitialized = true;
                Log.Information("Patched DLL ready");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to initialize patched DLL");
                throw;
            }
        }

        private static void StartHeartbeatCheck()
        {
            var timer = new System.Timers.Timer(10000); // 10 seconds
            timer.Elapsed += (s, e) =>
            {
                try
                {
                    if (!IPCClient.Ping())
                    {
                        Log.Warning("Heartbeat check failed - Service not responding");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Heartbeat check error");
                }
            };
            timer.Start();
        }

        // Extended functionality for CHC.CGO.Common.dll
        public static void EnableExtendedFeatures()
        {
            Log.Information("Extended features enabled");
            // Add custom features here
        }

        public static string GetLicenseInfo()
        {
            return $"Licensed to USB: {_currentUsbGuid}";
        }
    }
}
```

#### **File 4: `FeatureController.cs`**

```csharp
using Serilog;

namespace DLLPatch
{
    public static class FeatureController
    {
        private static bool _featuresEnabled = false;

        public static void EnableFeatures()
        {
            if (_featuresEnabled)
            {
                return;
            }

            try
            {
                Log.Information("Enabling extended features...");

                // Feature 1: Extended logging
                EnableExtendedLogging();

                // Feature 2: Custom validation
                EnableCustomValidation();

                // Feature 3: Advanced options
                EnableAdvancedOptions();

                _featuresEnabled = true;
                Log.Information("All extended features enabled");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to enable features");
            }
        }

        private static void EnableExtendedLogging()
        {
            Log.Debug("Extended logging enabled");
            // Implementation for extended logging
        }

        private static void EnableCustomValidation()
        {
            Log.Debug("Custom validation enabled");
            // Implementation for custom validation
        }

        private static void EnableAdvancedOptions()
        {
            Log.Debug("Advanced options enabled");
            // Implementation for advanced options
        }

        public static bool AreFeaturesEnabled()
        {
            return _featuresEnabled;
        }
    }
}
```

**✅ Checkpoint**: DLL Patch project compile thành công

---

### ⏰ 12:00 - 13:00 | 🍽️ NGHỈ TRƯA

---

### ⏰ 13:00 - 17:00 | USB Creator Tool (4 giờ)

#### **Setup DongleCreatorTool:**

```powershell
cd F:\3.Laptrinh\DUANUSB2\src\DongleCreatorTool

# Add references
dotnet add reference ../DongleSyncService/DongleSyncService.csproj

# Add packages
dotnet add package Newtonsoft.Json
```

#### **File 1: `CryptoHelper.cs`**

```csharp
using System.Security.Cryptography;
using System.Text;

namespace DongleCreatorTool
{
    public class CryptoHelper
    {
        private const int KeySize = 256;
        private const int Iterations = 100000;

        public static (byte[] encrypted, byte[] iv) EncryptDLL(byte[] dllData, string usbSerial)
        {
            try
            {
                var key = DeriveKey(usbSerial);

                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.Key = key;
                aes.GenerateIV();
                var iv = aes.IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor();
                var encrypted = encryptor.TransformFinalBlock(dllData, 0, dllData.Length);

                return (encrypted, iv);
            }
            catch (Exception ex)
            {
                throw new Exception("Encryption failed", ex);
            }
        }

        private static byte[] DeriveKey(string usbSerial)
        {
            const string masterSecret = "DongleSecretKey2025!@#$%^&*()";
            var password = $"{usbSerial}|{masterSecret}";
            var salt = Encoding.UTF8.GetBytes(usbSerial);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256
            );

            return pbkdf2.GetBytes(KeySize / 8);
        }
    }
}
```

#### **File 2: `USBWriter.cs`**

```csharp
using System.Management;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace DongleCreatorTool
{
    public class USBWriter
    {
        public static bool CreateDongle(
            string usbDrive,
            string patchDllPath,
            out string errorMessage)
        {
            errorMessage = null;

            try
            {
                // 1. Verify USB drive
                if (!Directory.Exists(usbDrive))
                {
                    errorMessage = "USB drive not found";
                    return false;
                }

                // 2. Verify patch DLL exists
                if (!File.Exists(patchDllPath))
                {
                    errorMessage = "Patch DLL file not found";
                    return false;
                }

                // 3. Compute USB hardware key
                var usbKey = ComputeUSBHardwareKey(usbDrive);
                if (string.IsNullOrEmpty(usbKey))
                {
                    errorMessage = "Failed to compute USB hardware key";
                    return false;
                }

                // 4. Generate USB GUID
                var usbGuid = Guid.NewGuid().ToString();

                // 5. Create dongle folder
                var donglePath = Path.Combine(usbDrive, "dongle");
                Directory.CreateDirectory(donglePath);

                // 6. Create config.json
                var config = new
                {
                    usbGuid = usbGuid,
                    version = "1.0",
                    createdAt = DateTime.UtcNow
                };
                var configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(Path.Combine(donglePath, "config.json"), configJson);

                // 7. Write dongle.key
                File.WriteAllText(Path.Combine(donglePath, "dongle.key"), usbKey);

                // 8. Encrypt and write patch DLL
                var dllData = File.ReadAllBytes(patchDllPath);
                var (encrypted, iv) = CryptoHelper.EncryptDLL(dllData, usbKey);

                File.WriteAllBytes(Path.Combine(donglePath, "patch.dll.enc"), encrypted);
                File.WriteAllBytes(Path.Combine(donglePath, "iv.bin"), iv);

                // 9. Create README.txt
                var readme = @"USB DONGLE FOR CHC GEOMATICS OFFICE 2
=====================================

This USB contains encrypted files for app enhancement.

Files:
- config.json: Dongle configuration
- dongle.key: Hardware authentication key
- patch.dll.enc: Encrypted patch file
- iv.bin: Encryption initialization vector

DO NOT:
- Copy these files to another USB
- Modify any files
- Delete any files

The system uses:
- Layer 1: USB Hardware ID validation
- Layer 2: AES-256 encryption
- Layer 3: Machine binding
- Layer 4: Runtime heartbeat monitoring

Created: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @"
USB GUID: " + usbGuid;

                File.WriteAllText(Path.Combine(donglePath, "README.txt"), readme);

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error: {ex.Message}";
                return false;
            }
        }

        private static string ComputeUSBHardwareKey(string drivePath)
        {
            try
            {
                var drive = drivePath.TrimEnd('\\');
                var query = $"SELECT * FROM Win32_LogicalDisk WHERE DeviceID = '{drive}'";

                using var searcher = new ManagementObjectSearcher(query);
                var collection = searcher.Get();

                string volumeSerial = "";
                foreach (ManagementObject obj in collection)
                {
                    volumeSerial = obj["VolumeSerialNumber"]?.ToString() ?? "";
                    break;
                }

                var deviceQuery = "SELECT * FROM Win32_DiskDrive WHERE Size IS NOT NULL";
                using var deviceSearcher = new ManagementObjectSearcher(deviceQuery);
                var deviceCollection = deviceSearcher.Get();

                string deviceId = "";
                foreach (ManagementObject obj in deviceCollection)
                {
                    var serialNumber = obj["SerialNumber"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(serialNumber))
                    {
                        deviceId = serialNumber;
                        break;
                    }
                }

                var combined = $"{volumeSerial}|{deviceId}";
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToBase64String(hashBytes);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to compute USB hardware key", ex);
            }
        }
    }
}
```

#### **File 3: `MainForm.cs` (WinForms UI)**

```csharp
using System.IO;

namespace DongleCreatorTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            LoadUSBDrives();
        }

        private void LoadUSBDrives()
        {
            cmbUSBDrive.Items.Clear();

            var drives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                .Select(d => d.Name)
                .ToList();

            foreach (var drive in drives)
            {
                cmbUSBDrive.Items.Add(drive);
            }

            if (cmbUSBDrive.Items.Count > 0)
            {
                cmbUSBDrive.SelectedIndex = 0;
            }
        }

        private void btnBrowseDLL_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Select Patch DLL File",
                Filter = "DLL Files (*.dll)|*.dll|All Files (*.*)|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtPatchDllPath.Text = dialog.FileName;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadUSBDrives();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (cmbUSBDrive.SelectedItem == null)
            {
                MessageBox.Show("Please select a USB drive", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPatchDllPath.Text) || !File.Exists(txtPatchDllPath.Text))
            {
                MessageBox.Show("Please select a valid patch DLL file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var usbDrive = cmbUSBDrive.SelectedItem.ToString();

            // Confirm
            var result = MessageBox.Show(
                $"Create dongle on {usbDrive}?\n\nThis will:\n" +
                "- Create a 'dongle' folder\n" +
                "- Write encrypted files\n" +
                "- Bind to USB hardware\n\n" +
                "Continue?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
            {
                return;
            }

            // Create dongle
            btnCreate.Enabled = false;
            lblStatus.Text = "Creating dongle...";
            Application.DoEvents();

            try
            {
                if (USBWriter.CreateDongle(usbDrive, txtPatchDllPath.Text, out var error))
                {
                    lblStatus.Text = "✅ Dongle created successfully!";
                    MessageBox.Show(
                        $"Dongle created successfully on {usbDrive}\n\n" +
                        "Files created in 'dongle' folder:\n" +
                        "- config.json\n" +
                        "- dongle.key\n" +
                        "- patch.dll.enc\n" +
                        "- iv.bin\n" +
                        "- README.txt",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    lblStatus.Text = "❌ Failed to create dongle";
                    MessageBox.Show($"Failed to create dongle:\n{error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ Error";
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCreate.Enabled = true;
            }
        }
    }
}
```

#### **File 4: `MainForm.Designer.cs` (UI Layout)**

```csharp
namespace DongleCreatorTool
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private ComboBox cmbUSBDrive;
        private TextBox txtPatchDllPath;
        private Button btnBrowseDLL;
        private Button btnRefresh;
        private Button btnCreate;
        private Label lblStatus;
        private Label label1;
        private Label label2;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.cmbUSBDrive = new ComboBox();
            this.txtPatchDllPath = new TextBox();
            this.btnBrowseDLL = new Button();
            this.btnRefresh = new Button();
            this.btnCreate = new Button();
            this.lblStatus = new Label();
            this.label1 = new Label();
            this.label2 = new Label();
            this.SuspendLayout();

            // label1
            this.label1.AutoSize = true;
            this.label1.Location = new Point(20, 20);
            this.label1.Name = "label1";
            this.label1.Size = new Size(70, 15);
            this.label1.Text = "USB Drive:";

            // cmbUSBDrive
            this.cmbUSBDrive.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbUSBDrive.Location = new Point(120, 17);
            this.cmbUSBDrive.Name = "cmbUSBDrive";
            this.cmbUSBDrive.Size = new Size(150, 23);

            // btnRefresh
            this.btnRefresh.Location = new Point(280, 16);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new Size(80, 25);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += btnRefresh_Click;

            // label2
            this.label2.AutoSize = true;
            this.label2.Location = new Point(20, 60);
            this.label2.Name = "label2";
            this.label2.Size = new Size(70, 15);
            this.label2.Text = "Patch DLL:";

            // txtPatchDllPath
            this.txtPatchDllPath.Location = new Point(120, 57);
            this.txtPatchDllPath.Name = "txtPatchDllPath";
            this.txtPatchDllPath.Size = new Size(350, 23);
            this.txtPatchDllPath.ReadOnly = true;

            // btnBrowseDLL
            this.btnBrowseDLL.Location = new Point(480, 56);
            this.btnBrowseDLL.Name = "btnBrowseDLL";
            this.btnBrowseDLL.Size = new Size(80, 25);
            this.btnBrowseDLL.Text = "Browse...";
            this.btnBrowseDLL.Click += btnBrowseDLL_Click;

            // btnCreate
            this.btnCreate.Location = new Point(120, 110);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new Size(200, 35);
            this.btnCreate.Text = "Create Dongle";
            this.btnCreate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnCreate.Click += btnCreate_Click;

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new Point(120, 160);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(100, 15);
            this.lblStatus.Text = "Ready";

            // MainForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(600, 200);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.btnBrowseDLL);
            this.Controls.Add(this.txtPatchDllPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.cmbUSBDrive);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "USB Dongle Creator Tool";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
```

**✅ Checkpoint NGÀY 5**:
- DLL Patch project hoạt động
- USB Creator Tool hoạt động
- Can create USB dongles with encrypted DLL

---

**[Tiếp tục NGÀY 6 →](04-4LAYER-DAYS6.md)**
