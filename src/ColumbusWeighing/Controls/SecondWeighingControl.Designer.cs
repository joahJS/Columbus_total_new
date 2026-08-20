namespace ColumbusWeighing.Controls
{
    partial class SecondWeighingControl
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

        #region Component Designer generated code

        private DevExpress.XtraEditors.PanelControl _headerPanel;
        private DevExpress.XtraEditors.LabelControl _titleLabel;
        private DevExpress.XtraEditors.SimpleButton _btnSingleWeighing;
        private DevExpress.XtraEditors.SimpleButton _btnSecondSlip;
        private DevExpress.XtraEditors.PanelControl _filterPanel;
        private DevExpress.XtraEditors.LabelControl _dateLabel;
        private DevExpress.XtraEditors.DateEdit _dateEdit;
        private DevExpress.XtraGrid.GridControl _gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView _gridView;

        private void InitializeComponent()
        {
            this._headerPanel = new DevExpress.XtraEditors.PanelControl();
            this._btnSecondSlip = new DevExpress.XtraEditors.SimpleButton();
            this._btnSingleWeighing = new DevExpress.XtraEditors.SimpleButton();
            this._titleLabel = new DevExpress.XtraEditors.LabelControl();
            this._filterPanel = new DevExpress.XtraEditors.PanelControl();
            this._dateEdit = new DevExpress.XtraEditors.DateEdit();
            this._dateLabel = new DevExpress.XtraEditors.LabelControl();
            this._gridControl = new DevExpress.XtraGrid.GridControl();
            this._gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this._headerPanel)).BeginInit();
            this._headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._filterPanel)).BeginInit();
            this._filterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dateEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).BeginInit();
            this.SuspendLayout();
            //
            // _headerPanel  (타이틀: 2차계량)
            //
            this._headerPanel.Appearance.BackColor = System.Drawing.Color.FromArgb(20, 45, 110);
            this._headerPanel.Appearance.Options.UseBackColor = true;
            this._headerPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._headerPanel.Controls.Add(this._btnSecondSlip);
            this._headerPanel.Controls.Add(this._btnSingleWeighing);
            this._headerPanel.Controls.Add(this._titleLabel);
            this._headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._headerPanel.Location = new System.Drawing.Point(0, 0);
            this._headerPanel.Name = "_headerPanel";
            this._headerPanel.Size = new System.Drawing.Size(1200, 34);
            this._headerPanel.TabIndex = 0;
            //
            // _titleLabel
            //
            this._titleLabel.Appearance.Font = new System.Drawing.Font("맑은 고딕", 11F, System.Drawing.FontStyle.Bold);
            this._titleLabel.Appearance.ForeColor = System.Drawing.Color.White;
            this._titleLabel.Appearance.Options.UseFont = true;
            this._titleLabel.Appearance.Options.UseForeColor = true;
            this._titleLabel.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this._titleLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this._titleLabel.Location = new System.Drawing.Point(0, 0);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this._titleLabel.Size = new System.Drawing.Size(120, 34);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = "2차계량";
            //
            // _btnSingleWeighing
            //
            this._btnSingleWeighing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnSingleWeighing.Location = new System.Drawing.Point(980, 4);
            this._btnSingleWeighing.Name = "_btnSingleWeighing";
            this._btnSingleWeighing.Size = new System.Drawing.Size(110, 26);
            this._btnSingleWeighing.TabIndex = 1;
            this._btnSingleWeighing.Text = "1회계량(F7)";
            //
            // _btnSecondSlip
            //
            this._btnSecondSlip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnSecondSlip.Location = new System.Drawing.Point(1098, 4);
            this._btnSecondSlip.Name = "_btnSecondSlip";
            this._btnSecondSlip.Size = new System.Drawing.Size(90, 26);
            this._btnSecondSlip.TabIndex = 2;
            this._btnSecondSlip.Text = "2차 전표";
            //
            // _filterPanel
            //
            this._filterPanel.Appearance.BackColor = System.Drawing.Color.FromArgb(255, 251, 224);
            this._filterPanel.Appearance.Options.UseBackColor = true;
            this._filterPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._filterPanel.Controls.Add(this._dateEdit);
            this._filterPanel.Controls.Add(this._dateLabel);
            this._filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._filterPanel.Location = new System.Drawing.Point(0, 34);
            this._filterPanel.Name = "_filterPanel";
            this._filterPanel.Size = new System.Drawing.Size(1200, 32);
            this._filterPanel.TabIndex = 1;
            //
            // _dateLabel
            //
            this._dateLabel.Appearance.Font = new System.Drawing.Font("맑은 고딕", 9.5F, System.Drawing.FontStyle.Bold);
            this._dateLabel.Appearance.Options.UseFont = true;
            this._dateLabel.Location = new System.Drawing.Point(10, 9);
            this._dateLabel.Name = "_dateLabel";
            this._dateLabel.Size = new System.Drawing.Size(52, 16);
            this._dateLabel.TabIndex = 0;
            this._dateLabel.Text = "조회일자";
            //
            // _dateEdit
            //
            this._dateEdit.EditValue = null;
            this._dateEdit.Location = new System.Drawing.Point(70, 5);
            this._dateEdit.Name = "_dateEdit";
            this._dateEdit.Properties.Mask.EditMask = "yyyy-MM-dd";
            this._dateEdit.Size = new System.Drawing.Size(120, 20);
            this._dateEdit.TabIndex = 1;
            //
            // _gridControl
            //
            this._gridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._gridControl.Location = new System.Drawing.Point(0, 66);
            this._gridControl.MainView = this._gridView;
            this._gridControl.Name = "_gridControl";
            this._gridControl.Size = new System.Drawing.Size(1200, 334);
            this._gridControl.TabIndex = 2;
            this._gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this._gridView});
            //
            // _gridView
            //
            this._gridView.GridControl = this._gridControl;
            this._gridView.Name = "_gridView";
            this._gridView.OptionsBehavior.Editable = false;
            this._gridView.OptionsView.ShowGroupPanel = false;
            this._gridView.OptionsView.ShowIndicator = false;
            //
            // SecondWeighingControl
            //
            this.Controls.Add(this._gridControl);
            this.Controls.Add(this._filterPanel);
            this.Controls.Add(this._headerPanel);
            this.Name = "SecondWeighingControl";
            this.Size = new System.Drawing.Size(1200, 400);
            ((System.ComponentModel.ISupportInitialize)(this._headerPanel)).EndInit();
            this._headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._filterPanel)).EndInit();
            this._filterPanel.ResumeLayout(false);
            this._filterPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dateEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
