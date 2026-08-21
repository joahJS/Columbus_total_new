namespace ColumbusWeighing.Controls
{
    partial class WeightDisplayControl
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

        private DevExpress.XtraEditors.LabelControl _captionLabel;
        private DevExpress.XtraEditors.LabelControl _valueLabel;
        private DevExpress.XtraEditors.PanelControl _captionPanel;
        private DevExpress.XtraEditors.PanelControl _valuePanel;

        private void InitializeComponent()
        {
            this._captionPanel = new DevExpress.XtraEditors.PanelControl();
            this._captionLabel = new DevExpress.XtraEditors.LabelControl();
            this._valuePanel = new DevExpress.XtraEditors.PanelControl();
            this._valueLabel = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this._captionPanel)).BeginInit();
            this._captionPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._valuePanel)).BeginInit();
            this._valuePanel.SuspendLayout();
            this.SuspendLayout();
            //
            // _captionPanel
            //
            this._captionPanel.Appearance.BackColor = System.Drawing.Color.Black;
            this._captionPanel.Appearance.Options.UseBackColor = true;
            this._captionPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._captionPanel.Controls.Add(this._captionLabel);
            this._captionPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this._captionPanel.Location = new System.Drawing.Point(0, 0);
            this._captionPanel.Name = "_captionPanel";
            this._captionPanel.Size = new System.Drawing.Size(90, 100);
            this._captionPanel.TabIndex = 0;
            //
            // _captionLabel
            //
            this._captionLabel.Appearance.Font = new System.Drawing.Font("맑은 고딕", 14F, System.Drawing.FontStyle.Bold);
            this._captionLabel.Appearance.ForeColor = System.Drawing.Color.Gold;
            this._captionLabel.Appearance.Options.UseFont = true;
            this._captionLabel.Appearance.Options.UseForeColor = true;
            this._captionLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this._captionLabel.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this._captionLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this._captionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._captionLabel.Location = new System.Drawing.Point(0, 0);
            this._captionLabel.Name = "_captionLabel";
            this._captionLabel.Size = new System.Drawing.Size(90, 100);
            this._captionLabel.TabIndex = 0;
            this._captionLabel.Text = "중  량";
            //
            // _valuePanel
            //
            this._valuePanel.Appearance.BackColor = System.Drawing.Color.Black;
            this._valuePanel.Appearance.Options.UseBackColor = true;
            this._valuePanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._valuePanel.Controls.Add(this._valueLabel);
            this._valuePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._valuePanel.Location = new System.Drawing.Point(90, 0);
            this._valuePanel.Name = "_valuePanel";
            this._valuePanel.Size = new System.Drawing.Size(310, 100);
            this._valuePanel.TabIndex = 1;
            //
            // _valueLabel
            //
            this._valueLabel.Appearance.Font = new System.Drawing.Font("Consolas", 30F, System.Drawing.FontStyle.Bold);
            this._valueLabel.Appearance.ForeColor = System.Drawing.Color.Lime;
            this._valueLabel.Appearance.Options.UseFont = true;
            this._valueLabel.Appearance.Options.UseForeColor = true;
            this._valueLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this._valueLabel.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this._valueLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this._valueLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._valueLabel.Location = new System.Drawing.Point(0, 0);
            this._valueLabel.Name = "_valueLabel";
            this._valueLabel.Size = new System.Drawing.Size(310, 100);
            this._valueLabel.TabIndex = 0;
            this._valueLabel.Text = "0 kg";
            //
            // WeightDisplayControl
            //
            this.Controls.Add(this._valuePanel);
            this.Controls.Add(this._captionPanel);
            this.Name = "WeightDisplayControl";
            this.Size = new System.Drawing.Size(400, 100);
            ((System.ComponentModel.ISupportInitialize)(this._captionPanel)).EndInit();
            this._captionPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._valuePanel)).EndInit();
            this._valuePanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
