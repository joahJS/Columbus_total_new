namespace ColumbusWeighing.Forms
{
    partial class WeighingColumnSettingsForm
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
        private DevExpress.XtraEditors.SimpleButton _btnReset;
        private DevExpress.XtraEditors.SimpleButton _btnSave;
        private DevExpress.XtraEditors.SimpleButton _btnClose;
        private System.Windows.Forms.Panel _bodyPanel;

        private void InitializeComponent()
        {
            this._topBar = new DevExpress.XtraEditors.PanelControl();
            this._titleLabel = new DevExpress.XtraEditors.LabelControl();
            this._btnReset = new DevExpress.XtraEditors.SimpleButton();
            this._btnSave = new DevExpress.XtraEditors.SimpleButton();
            this._btnClose = new DevExpress.XtraEditors.SimpleButton();
            this._bodyPanel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this._topBar)).BeginInit();
            this._topBar.SuspendLayout();
            this.SuspendLayout();
            //
            // _topBar
            //
            this._topBar.Appearance.BackColor = System.Drawing.Color.FromArgb(202, 229, 182);
            this._topBar.Appearance.Options.UseBackColor = true;
            this._topBar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._topBar.Controls.Add(this._btnClose);
            this._topBar.Controls.Add(this._btnSave);
            this._topBar.Controls.Add(this._btnReset);
            this._topBar.Controls.Add(this._titleLabel);
            this._topBar.Dock = System.Windows.Forms.DockStyle.Top;
            this._topBar.Location = new System.Drawing.Point(0, 0);
            this._topBar.Name = "_topBar";
            this._topBar.Size = new System.Drawing.Size(420, 56);
            this._topBar.TabIndex = 0;
            //
            // _titleLabel
            //
            this._titleLabel.Appearance.Font = new System.Drawing.Font("맑은 고딕", 10.5F, System.Drawing.FontStyle.Bold);
            this._titleLabel.Appearance.Options.UseFont = true;
            this._titleLabel.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this._titleLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this._titleLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this._titleLabel.Location = new System.Drawing.Point(0, 0);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this._titleLabel.Size = new System.Drawing.Size(120, 56);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = "계량 화면 설정";
            //
            // _btnReset
            //
            this._btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnReset.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9.5F);
            this._btnReset.Appearance.Options.UseFont = true;
            this._btnReset.Location = new System.Drawing.Point(126, 14);
            this._btnReset.Name = "_btnReset";
            this._btnReset.Size = new System.Drawing.Size(90, 28);
            this._btnReset.TabIndex = 1;
            this._btnReset.Text = "초기상태";
            //
            // _btnSave
            //
            this._btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnSave.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9.5F);
            this._btnSave.Appearance.Options.UseFont = true;
            this._btnSave.Location = new System.Drawing.Point(222, 14);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(90, 28);
            this._btnSave.TabIndex = 2;
            this._btnSave.Text = "저장";
            //
            // _btnClose
            //
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9.5F);
            this._btnClose.Appearance.Options.UseFont = true;
            this._btnClose.Location = new System.Drawing.Point(318, 14);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(90, 28);
            this._btnClose.TabIndex = 3;
            this._btnClose.Text = "종료";
            //
            // _bodyPanel
            //
            this._bodyPanel.AutoScroll = true;
            this._bodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._bodyPanel.Location = new System.Drawing.Point(0, 56);
            this._bodyPanel.Name = "_bodyPanel";
            this._bodyPanel.Padding = new System.Windows.Forms.Padding(10);
            this._bodyPanel.Size = new System.Drawing.Size(420, 346);
            this._bodyPanel.TabIndex = 1;
            //
            // WeighingColumnSettingsForm
            //
            this.ClientSize = new System.Drawing.Size(420, 402);
            this.Controls.Add(this._bodyPanel);
            this.Controls.Add(this._topBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WeighingColumnSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "계량 화면 설정";
            ((System.ComponentModel.ISupportInitialize)(this._topBar)).EndInit();
            this._topBar.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
