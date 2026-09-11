namespace ColumbusWeighing.Forms
{
    partial class UserManagementForm
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

        private DevExpress.XtraEditors.PanelControl _headerPanel;
        private DevExpress.XtraEditors.LabelControl _titleLabel;
        private DevExpress.XtraEditors.PanelControl _filterPanel;
        private DevExpress.XtraEditors.LabelControl _searchLabel;
        private DevExpress.XtraEditors.TextEdit _searchEdit;
        private DevExpress.XtraEditors.SimpleButton _btnRetrieve;
        private DevExpress.XtraEditors.SimpleButton _btnClose;
        private DevExpress.XtraGrid.GridControl _gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView _gridView;

        private void InitializeComponent()
        {
            this._headerPanel = new DevExpress.XtraEditors.PanelControl();
            this._titleLabel = new DevExpress.XtraEditors.LabelControl();
            this._filterPanel = new DevExpress.XtraEditors.PanelControl();
            this._btnClose = new DevExpress.XtraEditors.SimpleButton();
            this._btnRetrieve = new DevExpress.XtraEditors.SimpleButton();
            this._searchEdit = new DevExpress.XtraEditors.TextEdit();
            this._searchLabel = new DevExpress.XtraEditors.LabelControl();
            this._gridControl = new DevExpress.XtraGrid.GridControl();
            this._gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this._headerPanel)).BeginInit();
            this._headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._filterPanel)).BeginInit();
            this._filterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._searchEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).BeginInit();
            this.SuspendLayout();
            //
            // _headerPanel  (타이틀: 사용자 관리)
            //
            this._headerPanel.Appearance.BackColor = System.Drawing.Color.FromArgb(20, 45, 110);
            this._headerPanel.Appearance.Options.UseBackColor = true;
            this._headerPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._headerPanel.Controls.Add(this._titleLabel);
            this._headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._headerPanel.Location = new System.Drawing.Point(0, 0);
            this._headerPanel.Name = "_headerPanel";
            this._headerPanel.Size = new System.Drawing.Size(900, 34);
            this._headerPanel.TabIndex = 0;
            //
            // _titleLabel
            //
            this._titleLabel.Appearance.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold);
            this._titleLabel.Appearance.ForeColor = System.Drawing.Color.White;
            this._titleLabel.Appearance.Options.UseFont = true;
            this._titleLabel.Appearance.Options.UseForeColor = true;
            this._titleLabel.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this._titleLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this._titleLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this._titleLabel.Location = new System.Drawing.Point(0, 0);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this._titleLabel.Size = new System.Drawing.Size(120, 34);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = "사용자 관리";
            //
            // _filterPanel
            //
            this._filterPanel.Appearance.BackColor = System.Drawing.Color.FromArgb(255, 251, 224);
            this._filterPanel.Appearance.Options.UseBackColor = true;
            this._filterPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._filterPanel.Controls.Add(this._btnClose);
            this._filterPanel.Controls.Add(this._btnRetrieve);
            this._filterPanel.Controls.Add(this._searchEdit);
            this._filterPanel.Controls.Add(this._searchLabel);
            this._filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._filterPanel.Location = new System.Drawing.Point(0, 34);
            this._filterPanel.Name = "_filterPanel";
            this._filterPanel.Size = new System.Drawing.Size(900, 40);
            this._filterPanel.TabIndex = 1;
            //
            // _searchLabel
            //
            this._searchLabel.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold);
            this._searchLabel.Appearance.Options.UseFont = true;
            this._searchLabel.Location = new System.Drawing.Point(10, 13);
            this._searchLabel.Name = "_searchLabel";
            this._searchLabel.Size = new System.Drawing.Size(64, 16);
            this._searchLabel.TabIndex = 0;
            this._searchLabel.Text = "조회내역 :";
            //
            // _searchEdit
            //
            this._searchEdit.Location = new System.Drawing.Point(84, 9);
            this._searchEdit.Name = "_searchEdit";
            this._searchEdit.Size = new System.Drawing.Size(220, 20);
            this._searchEdit.TabIndex = 1;
            //
            // _btnRetrieve
            //
            this._btnRetrieve.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnRetrieve.Location = new System.Drawing.Point(703, 6);
            this._btnRetrieve.Name = "_btnRetrieve";
            this._btnRetrieve.Size = new System.Drawing.Size(85, 28);
            this._btnRetrieve.TabIndex = 2;
            this._btnRetrieve.Text = "조회(F5)";
            //
            // _btnClose
            //
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.Location = new System.Drawing.Point(794, 6);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(85, 28);
            this._btnClose.TabIndex = 3;
            this._btnClose.Text = "종료(ESC)";
            //
            // _gridControl
            //
            this._gridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._gridControl.Location = new System.Drawing.Point(0, 74);
            this._gridControl.MainView = this._gridView;
            this._gridControl.Name = "_gridControl";
            this._gridControl.Size = new System.Drawing.Size(900, 426);
            this._gridControl.TabIndex = 2;
            this._gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this._gridView});
            //
            // _gridView
            //
            this._gridView.GridControl = this._gridControl;
            this._gridView.Name = "_gridView";
            this._gridView.OptionsView.ShowIndicator = false;
            //
            // UserManagementForm
            //
            this.ClientSize = new System.Drawing.Size(900, 500);
            this.Controls.Add(this._gridControl);
            this.Controls.Add(this._filterPanel);
            this.Controls.Add(this._headerPanel);
            this.Name = "UserManagementForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "사용자 관리";
            ((System.ComponentModel.ISupportInitialize)(this._headerPanel)).EndInit();
            this._headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._filterPanel)).EndInit();
            this._filterPanel.ResumeLayout(false);
            this._filterPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._searchEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
