namespace ColumbusWeighing.Forms
{
    partial class SystemSettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private DevExpress.XtraEditors.PanelControl _topBar;
        private DevExpress.XtraEditors.LabelControl _titleLabel;
        private DevExpress.XtraEditors.SimpleButton _btnSave;
        private DevExpress.XtraEditors.SimpleButton _btnClose;
        private System.Windows.Forms.Panel _bodyPanel;
        private System.Windows.Forms.Panel _leftPanel;
        private System.Windows.Forms.Panel _rightPanel;
        private DevExpress.XtraEditors.GroupControl _grpUser;
        private DevExpress.XtraEditors.GroupControl _grpWeighing;
        private DevExpress.XtraEditors.GroupControl _grpPrint;
        private DevExpress.XtraEditors.GroupControl _grpCamera;
        private DevExpress.XtraEditors.GroupControl _grpIpCamera;

        private void InitializeComponent()
        {
            this._topBar = new DevExpress.XtraEditors.PanelControl();
            this._titleLabel = new DevExpress.XtraEditors.LabelControl();
            this._btnSave = new DevExpress.XtraEditors.SimpleButton();
            this._btnClose = new DevExpress.XtraEditors.SimpleButton();
            this._bodyPanel = new System.Windows.Forms.Panel();
            this._leftPanel = new System.Windows.Forms.Panel();
            this._rightPanel = new System.Windows.Forms.Panel();
            this._grpUser = new DevExpress.XtraEditors.GroupControl();
            this._grpWeighing = new DevExpress.XtraEditors.GroupControl();
            this._grpPrint = new DevExpress.XtraEditors.GroupControl();
            this._grpCamera = new DevExpress.XtraEditors.GroupControl();
            this._grpIpCamera = new DevExpress.XtraEditors.GroupControl();
            ((System.ComponentModel.ISupportInitialize)(this._topBar)).BeginInit();
            this._topBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._grpUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._grpWeighing)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._grpPrint)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._grpCamera)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._grpIpCamera)).BeginInit();
            this.SuspendLayout();
            //
            // _topBar
            //
            this._topBar.Appearance.BackColor = System.Drawing.Color.FromArgb(202, 229, 182);
            this._topBar.Appearance.Options.UseBackColor = true;
            this._topBar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._topBar.Controls.Add(this._btnClose);
            this._topBar.Controls.Add(this._btnSave);
            this._topBar.Controls.Add(this._titleLabel);
            this._topBar.Dock = System.Windows.Forms.DockStyle.Top;
            this._topBar.Location = new System.Drawing.Point(0, 0);
            this._topBar.Name = "_topBar";
            this._topBar.Size = new System.Drawing.Size(1010, 42);
            this._topBar.TabIndex = 0;
            //
            // _titleLabel
            //
            this._titleLabel.Appearance.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold);
            this._titleLabel.Appearance.Options.UseFont = true;
            this._titleLabel.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this._titleLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this._titleLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this._titleLabel.Location = new System.Drawing.Point(0, 0);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this._titleLabel.Size = new System.Drawing.Size(220, 42);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = "시스템 설정";
            //
            // _btnSave
            //
            this._btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnSave.Location = new System.Drawing.Point(868, 7);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(64, 28);
            this._btnSave.TabIndex = 1;
            this._btnSave.Text = "저장";
            //
            // _btnClose
            //
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.Location = new System.Drawing.Point(938, 7);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(64, 28);
            this._btnClose.TabIndex = 2;
            this._btnClose.Text = "종료";
            //
            // _bodyPanel
            //
            this._bodyPanel.Controls.Add(this._rightPanel);
            this._bodyPanel.Controls.Add(this._leftPanel);
            this._bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._bodyPanel.Location = new System.Drawing.Point(0, 42);
            this._bodyPanel.Name = "_bodyPanel";
            this._bodyPanel.Size = new System.Drawing.Size(1010, 638);
            this._bodyPanel.TabIndex = 1;
            //
            // _leftPanel
            //
            this._leftPanel.AutoScroll = true;
            this._leftPanel.Controls.Add(this._grpWeighing);
            this._leftPanel.Controls.Add(this._grpUser);
            this._leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this._leftPanel.Location = new System.Drawing.Point(0, 0);
            this._leftPanel.Name = "_leftPanel";
            this._leftPanel.Size = new System.Drawing.Size(500, 638);
            this._leftPanel.TabIndex = 0;
            //
            // _rightPanel
            //
            this._rightPanel.AutoScroll = true;
            this._rightPanel.Controls.Add(this._grpIpCamera);
            this._rightPanel.Controls.Add(this._grpCamera);
            this._rightPanel.Controls.Add(this._grpPrint);
            this._rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rightPanel.Location = new System.Drawing.Point(500, 0);
            this._rightPanel.Name = "_rightPanel";
            this._rightPanel.Size = new System.Drawing.Size(510, 638);
            this._rightPanel.TabIndex = 1;
            //
            // _grpUser
            //
            this._grpUser.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this._grpUser.Appearance.Options.UseFont = true;
            this._grpUser.Location = new System.Drawing.Point(10, 10);
            this._grpUser.Name = "_grpUser";
            this._grpUser.Size = new System.Drawing.Size(470, 240);
            this._grpUser.TabIndex = 0;
            this._grpUser.Text = "사용자 설정";
            //
            // _grpWeighing
            //
            this._grpWeighing.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this._grpWeighing.Appearance.Options.UseFont = true;
            this._grpWeighing.Location = new System.Drawing.Point(10, 260);
            this._grpWeighing.Name = "_grpWeighing";
            this._grpWeighing.Size = new System.Drawing.Size(470, 330);
            this._grpWeighing.TabIndex = 1;
            this._grpWeighing.Text = "계량 설정";
            //
            // _grpPrint
            //
            this._grpPrint.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this._grpPrint.Appearance.Options.UseFont = true;
            this._grpPrint.Location = new System.Drawing.Point(10, 10);
            this._grpPrint.Name = "_grpPrint";
            this._grpPrint.Size = new System.Drawing.Size(470, 140);
            this._grpPrint.TabIndex = 0;
            this._grpPrint.Text = "인쇄 설정";
            //
            // _grpCamera
            //
            this._grpCamera.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this._grpCamera.Appearance.Options.UseFont = true;
            this._grpCamera.Location = new System.Drawing.Point(10, 160);
            this._grpCamera.Name = "_grpCamera";
            this._grpCamera.Size = new System.Drawing.Size(470, 120);
            this._grpCamera.TabIndex = 1;
            this._grpCamera.Text = "카메라 설정";
            //
            // _grpIpCamera
            //
            this._grpIpCamera.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this._grpIpCamera.Appearance.Options.UseFont = true;
            this._grpIpCamera.Location = new System.Drawing.Point(10, 290);
            this._grpIpCamera.Name = "_grpIpCamera";
            this._grpIpCamera.Size = new System.Drawing.Size(470, 190);
            this._grpIpCamera.TabIndex = 2;
            this._grpIpCamera.Text = "IP 카메라 설정";
            //
            // SystemSettingsForm
            //
            this.ClientSize = new System.Drawing.Size(1010, 680);
            this.Controls.Add(this._bodyPanel);
            this.Controls.Add(this._topBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SystemSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "시스템 설정";
            ((System.ComponentModel.ISupportInitialize)(this._topBar)).EndInit();
            this._topBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._grpUser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._grpWeighing)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._grpPrint)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._grpCamera)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._grpIpCamera)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
