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
        private DevExpress.XtraEditors.SimpleButton _btnSecondSlip;
        private DevExpress.XtraEditors.PanelControl _filterPanel;
        private DevExpress.XtraEditors.LabelControl _dateLabel;
        private DevExpress.XtraEditors.DateEdit _dateEditFrom;
        private DevExpress.XtraEditors.LabelControl _dateRangeLabel;
        private DevExpress.XtraEditors.DateEdit _dateEditTo;
        private DevExpress.XtraGrid.GridControl _gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView _gridView;

        private void InitializeComponent()
        {
            this._headerPanel = new DevExpress.XtraEditors.PanelControl();
            this._btnSecondSlip = new DevExpress.XtraEditors.SimpleButton();
            this._titleLabel = new DevExpress.XtraEditors.LabelControl();
            this._filterPanel = new DevExpress.XtraEditors.PanelControl();
            this._dateEditTo = new DevExpress.XtraEditors.DateEdit();
            this._dateRangeLabel = new DevExpress.XtraEditors.LabelControl();
            this._dateEditFrom = new DevExpress.XtraEditors.DateEdit();
            this._dateLabel = new DevExpress.XtraEditors.LabelControl();
            this._gridControl = new DevExpress.XtraGrid.GridControl();
            this._gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this._headerPanel)).BeginInit();
            this._headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._filterPanel)).BeginInit();
            this._filterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dateEditTo.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._dateEditFrom.Properties)).BeginInit();
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
            this._titleLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this._titleLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this._titleLabel.Location = new System.Drawing.Point(0, 0);
            this._titleLabel.Name = "_titleLabel";
            this._titleLabel.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this._titleLabel.Size = new System.Drawing.Size(120, 34);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = "2차계량 완료";
            //
            // _btnSecondSlip
            //
            this._btnSecondSlip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnSecondSlip.Location = new System.Drawing.Point(1098, 4);
            this._btnSecondSlip.Name = "_btnSecondSlip";
            this._btnSecondSlip.Size = new System.Drawing.Size(90, 26);
            this._btnSecondSlip.TabIndex = 1;
            this._btnSecondSlip.Text = "2차 전표";
            //
            // _filterPanel
            //
            this._filterPanel.Appearance.BackColor = System.Drawing.Color.FromArgb(255, 251, 224);
            this._filterPanel.Appearance.Options.UseBackColor = true;
            this._filterPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._filterPanel.Controls.Add(this._dateEditTo);
            this._filterPanel.Controls.Add(this._dateRangeLabel);
            this._filterPanel.Controls.Add(this._dateEditFrom);
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
            this._dateLabel.Text = "조회기간";
            //
            // _dateEditFrom
            //
            this._dateEditFrom.EditValue = null;
            this._dateEditFrom.Location = new System.Drawing.Point(70, 5);
            this._dateEditFrom.Name = "_dateEditFrom";
            this._dateEditFrom.Properties.Mask.EditMask = "yyyy-MM-dd";
            this._dateEditFrom.Size = new System.Drawing.Size(120, 20);
            this._dateEditFrom.TabIndex = 1;
            //
            // _dateRangeLabel
            //
            this._dateRangeLabel.Location = new System.Drawing.Point(197, 9);
            this._dateRangeLabel.Name = "_dateRangeLabel";
            this._dateRangeLabel.Size = new System.Drawing.Size(6, 13);
            this._dateRangeLabel.TabIndex = 2;
            this._dateRangeLabel.Text = "~";
            //
            // _dateEditTo
            //
            this._dateEditTo.EditValue = null;
            this._dateEditTo.Location = new System.Drawing.Point(213, 5);
            this._dateEditTo.Name = "_dateEditTo";
            this._dateEditTo.Properties.Mask.EditMask = "yyyy-MM-dd";
            this._dateEditTo.Size = new System.Drawing.Size(120, 20);
            this._dateEditTo.TabIndex = 3;
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
            ((System.ComponentModel.ISupportInitialize)(this._dateEditTo.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._dateEditFrom.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
