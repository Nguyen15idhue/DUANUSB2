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