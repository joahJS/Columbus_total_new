namespace ColumbusWeighing.Forms
{
    partial class MainForm
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

        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem _menuFile;
        private System.Windows.Forms.ToolStripMenuItem _menuFileExit;
        private System.Windows.Forms.ToolStripMenuItem _menuBaseData;
        private System.Windows.Forms.ToolStripMenuItem _menuBaseDataCustomer;
        private System.Windows.Forms.ToolStripMenuItem _menuBaseDataVehicle;
        private System.Windows.Forms.ToolStripMenuItem _menuBaseDataProduct;
        private System.Windows.Forms.ToolStripMenuItem _menuStatus;
        private System.Windows.Forms.ToolStripMenuItem _menuStatusDaily;
        private System.Windows.Forms.ToolStripMenuItem _menuStatusPeriod;

        private System.Windows.Forms.Panel _topInfoPanel;
        private ColumbusWeighing.Controls.WeightDisplayControl _weightDisplay;
        private System.Windows.Forms.Panel _rightInfoPanel;
        private System.Windows.Forms.TextBox _logMemo;
        private System.Windows.Forms.Panel _userPanel;
        private DevExpress.XtraEditors.SimpleButton _btnLogin;
        private DevExpress.XtraEditors.LabelControl _companyLabel;

        private DevExpress.XtraEditors.SplitContainerControl _splitContainer;
        private ColumbusWeighing.Controls.FirstWeighingControl _firstWeighingControl;
        private ColumbusWeighing.Controls.SecondWeighingControl _secondWeighingControl;

        private void InitializeComponent()
        {
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this._menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this._menuBaseData = new System.Windows.Forms.ToolStripMenuItem();
            this._menuBaseDataCustomer = new System.Windows.Forms.ToolStripMenuItem();
            this._menuBaseDataVehicle = new System.Windows.Forms.ToolStripMenuItem();
            this._menuBaseDataProduct = new System.Windows.Forms.ToolStripMenuItem();
            this._menuStatus = new System.Windows.Forms.ToolStripMenuItem();
            this._menuStatusDaily = new System.Windows.Forms.ToolStripMenuItem();
            this._menuStatusPeriod = new System.Windows.Forms.ToolStripMenuItem();

            this._topInfoPanel = new System.Windows.Forms.Panel();
            this._weightDisplay = new ColumbusWeighing.Controls.WeightDisplayControl();
            this._rightInfoPanel = new System.Windows.Forms.Panel();
            this._logMemo = new System.Windows.Forms.TextBox();
            this._userPanel = new System.Windows.Forms.Panel();
            this._companyLabel = new DevExpress.XtraEditors.LabelControl();
            this._btnLogin = new DevExpress.XtraEditors.SimpleButton();

            this._splitContainer = new DevExpress.XtraEditors.SplitContainerControl();
            this._firstWeighingControl = new ColumbusWeighing.Controls.FirstWeighingControl();
            this._secondWeighingControl = new ColumbusWeighing.Controls.SecondWeighingControl();

            this._menuStrip.SuspendLayout();
            this._topInfoPanel.SuspendLayout();
            this._rightInfoPanel.SuspendLayout();
            this._userPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.SuspendLayout();
            this.SuspendLayout();
            //
            // _menuStrip
            //
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._menuFile,
            this._menuBaseData,
            this._menuStatus});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(1264, 24);
            this._menuStrip.TabIndex = 0;
            //
            // _menuFile
            //
            this._menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._menuFileExit});
            this._menuFile.Name = "_menuFile";
            this._menuFile.Text = "파일(&F)";
            //
            // _menuFileExit
            //
            this._menuFileExit.Name = "_menuFileExit";
            this._menuFileExit.Text = "종료(&X)";
            //
            // _menuBaseData
            //
            this._menuBaseData.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._menuBaseDataCustomer,
            this._menuBaseDataVehicle,
            this._menuBaseDataProduct});
            this._menuBaseData.Name = "_menuBaseData";
            this._menuBaseData.Text = "기초자료(&I)";
            //
            // _menuBaseDataCustomer
            //
            this._menuBaseDataCustomer.Name = "_menuBaseDataCustomer";
            this._menuBaseDataCustomer.Text = "거래처 관리";
            //
            // _menuBaseDataVehicle
            //
            this._menuBaseDataVehicle.Name = "_menuBaseDataVehicle";
            this._menuBaseDataVehicle.Text = "차량 관리";
            //
            // _menuBaseDataProduct
            //
            this._menuBaseDataProduct.Name = "_menuBaseDataProduct";
            this._menuBaseDataProduct.Text = "제품 관리";
            //
            // _menuStatus
            //
            this._menuStatus.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._menuStatusDaily,
            this._menuStatusPeriod});
            this._menuStatus.Name = "_menuStatus";
            this._menuStatus.Text = "계량현황 및 집계(&S)";
            //
            // _menuStatusDaily
            //
            this._menuStatusDaily.Name = "_menuStatusDaily";
            this._menuStatusDaily.Text = "일일 계량현황";
            //
            // _menuStatusPeriod
            //
            this._menuStatusPeriod.Name = "_menuStatusPeriod";
            this._menuStatusPeriod.Text = "기간별 집계";
            //
            // _topInfoPanel
            //
            this._topInfoPanel.BackColor = System.Drawing.Color.Black;
            this._topInfoPanel.Controls.Add(this._rightInfoPanel);
            this._topInfoPanel.Controls.Add(this._weightDisplay);
            this._topInfoPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._topInfoPanel.Location = new System.Drawing.Point(0, 24);
            this._topInfoPanel.Name = "_topInfoPanel";
            this._topInfoPanel.Size = new System.Drawing.Size(1264, 82);
            this._topInfoPanel.TabIndex = 1;
            //
            // _weightDisplay
            //
            this._weightDisplay.Dock = System.Windows.Forms.DockStyle.Left;
            this._weightDisplay.Location = new System.Drawing.Point(0, 0);
            this._weightDisplay.Name = "_weightDisplay";
            this._weightDisplay.Size = new System.Drawing.Size(430, 82);
            this._weightDisplay.TabIndex = 0;
            //
            // _rightInfoPanel
            //
            this._rightInfoPanel.BackColor = System.Drawing.Color.Black;
            this._rightInfoPanel.Controls.Add(this._logMemo);
            this._rightInfoPanel.Controls.Add(this._userPanel);
            this._rightInfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rightInfoPanel.Location = new System.Drawing.Point(430, 0);
            this._rightInfoPanel.Name = "_rightInfoPanel";
            this._rightInfoPanel.Size = new System.Drawing.Size(834, 82);
            this._rightInfoPanel.TabIndex = 1;
            //
            // _logMemo
            //
            this._logMemo.BackColor = System.Drawing.Color.FromArgb(33, 37, 41);
            this._logMemo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._logMemo.Dock = System.Windows.Forms.DockStyle.Fill;
            this._logMemo.Font = new System.Drawing.Font("Consolas", 9F);
            this._logMemo.ForeColor = System.Drawing.Color.Silver;
            this._logMemo.Location = new System.Drawing.Point(0, 0);
            this._logMemo.Multiline = true;
            this._logMemo.Name = "_logMemo";
            this._logMemo.ReadOnly = true;
            this._logMemo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._logMemo.Size = new System.Drawing.Size(604, 82);
            this._logMemo.TabIndex = 0;
            //
            // _userPanel
            //
            this._userPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            this._userPanel.Controls.Add(this._companyLabel);
            this._userPanel.Controls.Add(this._btnLogin);
            this._userPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this._userPanel.Location = new System.Drawing.Point(604, 0);
            this._userPanel.Name = "_userPanel";
            this._userPanel.Size = new System.Drawing.Size(260, 82);
            this._userPanel.TabIndex = 1;
            //
            // _btnLogin
            //
            this._btnLogin.Appearance.BackColor = System.Drawing.Color.FromArgb(41, 128, 225);
            this._btnLogin.Appearance.ForeColor = System.Drawing.Color.White;
            this._btnLogin.Appearance.Options.UseBackColor = true;
            this._btnLogin.Appearance.Options.UseForeColor = true;
            this._btnLogin.Location = new System.Drawing.Point(12, 10);
            this._btnLogin.Name = "_btnLogin";
            this._btnLogin.Size = new System.Drawing.Size(90, 28);
            this._btnLogin.TabIndex = 0;
            this._btnLogin.Text = "LOGIN";
            //
            // _companyLabel
            //
            this._companyLabel.Appearance.Font = new System.Drawing.Font("맑은 고딕", 17F, System.Drawing.FontStyle.Bold);
            this._companyLabel.Appearance.ForeColor = System.Drawing.Color.Black;
            this._companyLabel.Appearance.Options.UseFont = true;
            this._companyLabel.Appearance.Options.UseForeColor = true;
            this._companyLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this._companyLabel.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this._companyLabel.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this._companyLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._companyLabel.Location = new System.Drawing.Point(0, 0);
            this._companyLabel.Name = "_companyLabel";
            this._companyLabel.Padding = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this._companyLabel.Size = new System.Drawing.Size(260, 82);
            this._companyLabel.TabIndex = 1;
            this._companyLabel.Text = "콜럼버스 주식회사";
            //
            // _splitContainer
            //
            this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer.Horizontal = false;
            this._splitContainer.Location = new System.Drawing.Point(0, 106);
            this._splitContainer.Name = "_splitContainer";
            this._splitContainer.Panel1.Controls.Add(this._firstWeighingControl);
            this._splitContainer.Panel1.Text = "Panel1";
            this._splitContainer.Panel2.Controls.Add(this._secondWeighingControl);
            this._splitContainer.Panel2.Text = "Panel2";
            this._splitContainer.Size = new System.Drawing.Size(1264, 597);
            this._splitContainer.SplitterPosition = 220;
            this._splitContainer.TabIndex = 2;
            //
            // _firstWeighingControl
            //
            this._firstWeighingControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._firstWeighingControl.Location = new System.Drawing.Point(0, 0);
            this._firstWeighingControl.Name = "_firstWeighingControl";
            this._firstWeighingControl.Size = new System.Drawing.Size(1264, 220);
            this._firstWeighingControl.TabIndex = 0;
            //
            // _secondWeighingControl
            //
            this._secondWeighingControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._secondWeighingControl.Location = new System.Drawing.Point(0, 0);
            this._secondWeighingControl.Name = "_secondWeighingControl";
            this._secondWeighingControl.Size = new System.Drawing.Size(1264, 373);
            this._secondWeighingControl.TabIndex = 0;
            //
            // MainForm
            //
            this.ClientSize = new System.Drawing.Size(1264, 703);
            this.Controls.Add(this._splitContainer);
            this.Controls.Add(this._topInfoPanel);
            this.Controls.Add(this._menuStrip);
            this.MainMenuStrip = this._menuStrip;
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "계량관리 프로그램 - 콜럼버스 주식회사";
            this.WindowState = System.Windows.Forms.FormWindowState.Normal;
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this._topInfoPanel.ResumeLayout(false);
            this._rightInfoPanel.ResumeLayout(false);
            this._rightInfoPanel.PerformLayout();
            this._userPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
