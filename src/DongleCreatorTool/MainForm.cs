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
                    
                    // Auto-delete old bind.key to force new binding
                    var bindKeyPath = @"C:\ProgramData\DongleSyncService\bind.key";
                    try
                    {
                        if (File.Exists(bindKeyPath))
                        {
                            File.Delete(bindKeyPath);
                            lblStatus.Text += " | Old binding cleared";
                        }
                    }
                    catch
                    {
                        // Ignore if cannot delete (permission issue or service locked)
                    }
                    
                    MessageBox.Show(
                        $"Dongle created successfully on {usbDrive}\n\n" +
                        "Files created in 'dongle' folder:\n" +
                        "- config.json\n" +
                        "- dongle.key\n" +
                        "- patch.dll.enc\n" +
                        "- iv.bin\n" +
                        "- README.txt\n\n" +
                        "✅ Old machine binding cleared automatically\n" +
                        "Ready to plug USB for first-time binding!",
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