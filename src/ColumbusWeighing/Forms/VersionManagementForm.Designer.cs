namespace ColumbusWeighing.Forms
{
    partial class VersionManagementForm
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

        private System.Windows.Forms.Panel _topPanel;
        private DevExpress.XtraEditors.LabelControl _titleLabel;
        private DevExpress.XtraEditors.SimpleButton _btnRetrieve;
        private DevExpress.XtraEditors.SimpleButton _btnAdd;
        private DevExpress.XtraEditors.SimpleButton _btnClose;
        private DevExpress.XtraGrid.GridControl _gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView _gridView;

        private void InitializeComponent()
        {
            this._topPanel = new System.Windows.Forms.Panel();
            this._btnClose = new DevExpress.XtraEditors.SimpleButton();
            this._btnAdd = new DevExpress.XtraEditors.SimpleButton();
            this._btnRetrieve = new DevExpress.XtraEditors.SimpleButton();
            this._titleLabel = new DevExpress.XtraEditors.LabelControl();
            this._gridControl = new DevExpress.XtraGrid.GridControl();
            this._gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this._topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._gridControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).BeginInit();
            this.SuspendLayout();
            //
            // _topPanel
            //
            this._topPanel.Controls.Add(this._btnClose);
            this._topPanel.Controls.Add(this._btnAdd);
            this._topPanel.Controls.Add(this._btnRetrieve);
            this._topPanel.Controls.Add(this._titleLabel);
            this._topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._topPanel.Location = new System.Drawing.Point(0, 0);
            this._topPanel.Name = "_topPanel";
            this._topPanel.Size = new System.Drawing.Size(900, 44);
            this._topPanel.TabIndex = 0;
            //
            // _titleLabel
            //
            this._titleLabel.Appearance.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this._titleLabel.Appearance.Options.UseFont = true;
            this._titleLabel.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this._titleLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this._titleLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this._titleLabel.Location = new System.Drawing.Point(0, 0);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this._titleLabel.Size = new System.Drawing.Size(140, 44);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = "버전관리";
            //
            // _btnRetrieve
            //
            this._btnRetrieve.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnRetrieve.Location = new System.Drawing.Point(612, 8);
            this._btnRetrieve.Name = "_btnRetrieve";
            this._btnRetrieve.Size = new System.Drawing.Size(85, 28);
            this._btnRetrieve.TabIndex = 1;
            this._btnRetrieve.Text = "조회(F5)";
            //
            // _btnAdd
            //
            this._btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnAdd.Location = new System.Drawing.Point(703, 8);
            this._btnAdd.Name = "_btnAdd";
            this._btnAdd.Size = new System.Drawing.Size(85, 28);
            this._btnAdd.TabIndex = 2;
            this._btnAdd.Text = "추가(F1)";
            //
            // _btnClose
            //
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.Location = new System.Drawing.Point(794, 8);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(85, 28);
            this._btnClose.TabIndex = 3;
            this._btnClose.Text = "닫기(ESC)";
            //
            // _gridControl
            //
            this._gridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._gridControl.Location = new System.Drawing.Point(0, 44);
            this._gridControl.MainView = this._gridView;
            this._gridControl.Name = "_gridControl";
            this._gridControl.Size = new System.Drawing.Size(900, 456);
            this._gridControl.TabIndex = 1;
            this._gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this._gridView});
            //
            // _gridView
            //
            this._gridView.GridControl = this._gridControl;
            this._gridView.Name = "_gridView";
            this._gridView.OptionsView.ShowIndicator = false;
            //
            // VersionManagementForm
            //
            this.ClientSize = new System.Drawing.Size(900, 500);
            this.Controls.Add(this._gridControl);
            this.Controls.Add(this._topPanel);
            this.Name = "VersionManagementForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "버전관리";
            this._topPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._gridControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
