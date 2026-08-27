namespace ColumbusWeighing.Controls
{
    partial class FirstWeighingControl
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
        private DevExpress.XtraEditors.SimpleButton _btnFirstSlip;
        private DevExpress.XtraGrid.GridControl _gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView _gridView;

        private void InitializeComponent()
        {
            this._headerPanel = new DevExpress.XtraEditors.PanelControl();
            this._btnFirstSlip = new DevExpress.XtraEditors.SimpleButton();
            this._titleLabel = new DevExpress.XtraEditors.LabelControl();
            this._gridControl = new DevExpress.XtraGrid.GridControl();
            this._gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            ((System.ComponentModel.ISupportInitialize)(this._headerPanel)).BeginInit();
            this._headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._gridControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).BeginInit();
            this.SuspendLayout();
            //
            // _headerPanel
            //
            this._headerPanel.Appearance.BackColor = System.Drawing.Color.FromArgb(20, 45, 110);
            this._headerPanel.Appearance.Options.UseBackColor = true;
            this._headerPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._headerPanel.Controls.Add(this._btnFirstSlip);
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
            this._titleLabel.Text = "1차 계량 대기";
            //
            // _btnFirstSlip
            //
            this._btnFirstSlip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnFirstSlip.Location = new System.Drawing.Point(798, 4);
            this._btnFirstSlip.Name = "_btnFirstSlip";
            this._btnFirstSlip.Size = new System.Drawing.Size(90, 26);
            this._btnFirstSlip.TabIndex = 1;
            this._btnFirstSlip.Text = "1차 전표";
            //
            // _gridControl
            //
            this._gridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._gridControl.Location = new System.Drawing.Point(0, 34);
            this._gridControl.MainView = this._gridView;
            this._gridControl.Name = "_gridControl";
            this._gridControl.Size = new System.Drawing.Size(900, 166);
            this._gridControl.TabIndex = 1;
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
            // FirstWeighingControl
            //
            this.Controls.Add(this._gridControl);
            this.Controls.Add(this._headerPanel);
            this.Name = "FirstWeighingControl";
            this.Size = new System.Drawing.Size(900, 200);
            ((System.ComponentModel.ISupportInitialize)(this._headerPanel)).EndInit();
            this._headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._gridControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
