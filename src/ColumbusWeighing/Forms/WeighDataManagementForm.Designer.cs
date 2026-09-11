namespace ColumbusWeighing.Forms
{
    partial class WeighDataManagementForm
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
        private DevExpress.XtraEditors.SimpleButton _btnExcel;
        private DevExpress.XtraEditors.SimpleButton _btnRetrieve;
        private DevExpress.XtraEditors.SimpleButton _btnPrint;
        private DevExpress.XtraEditors.SimpleButton _btnClose;
        private DevExpress.XtraGrid.GridControl _gridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView _gridView;
        private DevExpress.XtraEditors.PanelControl _summaryPanel;

        // 조회 조건 입력칸/집계 라벨은 개수가 많아 Designer가 아니라 코드(BuildFilterControls/
        // BuildSummaryLabels)에서 만든다 - 계량 화면 설정 팝업의 체크박스 행과 같은 방식이다.

        private void InitializeComponent()
        {
            this._headerPanel = new DevExpress.XtraEditors.PanelControl();
            this._titleLabel = new DevExpress.XtraEditors.LabelControl();
            this._filterPanel = new DevExpress.XtraEditors.PanelControl();
            this._btnClose = new DevExpress.XtraEditors.SimpleButton();
            this._btnPrint = new DevExpress.XtraEditors.SimpleButton();
            this._btnRetrieve = new DevExpress.XtraEditors.SimpleButton();
            this._btnExcel = new DevExpress.XtraEditors.SimpleButton();
            this._gridControl = new DevExpress.XtraGrid.GridControl();
            this._gridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this._summaryPanel = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(this._headerPanel)).BeginInit();
            this._headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._filterPanel)).BeginInit();
            this._filterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._gridControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._summaryPanel)).BeginInit();
            this.SuspendLayout();
            //
            // _headerPanel  (타이틀: 계량 데이터 관리)
            //
            this._headerPanel.Appearance.BackColor = System.Drawing.Color.FromArgb(20, 45, 110);
            this._headerPanel.Appearance.Options.UseBackColor = true;
            this._headerPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._headerPanel.Controls.Add(this._titleLabel);
            this._headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._headerPanel.Location = new System.Drawing.Point(0, 0);
            this._headerPanel.Name = "_headerPanel";
            this._headerPanel.Size = new System.Drawing.Size(1600, 34);
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
            this._titleLabel.Size = new System.Drawing.Size(150, 34);
            this._titleLabel.TabIndex = 0;
            this._titleLabel.Text = "계량 데이터 관리";
            //
            // _filterPanel
            //
            this._filterPanel.Appearance.BackColor = System.Drawing.Color.FromArgb(255, 251, 224);
            this._filterPanel.Appearance.Options.UseBackColor = true;
            this._filterPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._filterPanel.Controls.Add(this._btnClose);
            this._filterPanel.Controls.Add(this._btnPrint);
            this._filterPanel.Controls.Add(this._btnRetrieve);
            this._filterPanel.Controls.Add(this._btnExcel);
            this._filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._filterPanel.Location = new System.Drawing.Point(0, 34);
            this._filterPanel.Name = "_filterPanel";
            this._filterPanel.Size = new System.Drawing.Size(1600, 76);
            this._filterPanel.TabIndex = 1;
            //
            // _btnExcel
            //
            this._btnExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnExcel.Location = new System.Drawing.Point(782, 24);
            this._btnExcel.Name = "_btnExcel";
            this._btnExcel.Size = new System.Drawing.Size(85, 28);
            this._btnExcel.TabIndex = 10;
            this._btnExcel.Text = "엑셀";
            //
            // _btnRetrieve
            //
            this._btnRetrieve.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnRetrieve.Location = new System.Drawing.Point(873, 24);
            this._btnRetrieve.Name = "_btnRetrieve";
            this._btnRetrieve.Size = new System.Drawing.Size(85, 28);
            this._btnRetrieve.TabIndex = 11;
            this._btnRetrieve.Text = "조회(F5)";
            //
            // _btnPrint
            //
            this._btnPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnPrint.Location = new System.Drawing.Point(964, 24);
            this._btnPrint.Name = "_btnPrint";
            this._btnPrint.Size = new System.Drawing.Size(85, 28);
            this._btnPrint.TabIndex = 12;
            this._btnPrint.Text = "인쇄";
            //
            // _btnClose
            //
            this._btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._btnClose.Location = new System.Drawing.Point(1055, 24);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(85, 28);
            this._btnClose.TabIndex = 13;
            this._btnClose.Text = "종료(ESC)";
            //
            // _gridControl
            //
            this._gridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._gridControl.Location = new System.Drawing.Point(0, 110);
            this._gridControl.MainView = this._gridView;
            this._gridControl.Name = "_gridControl";
            this._gridControl.Size = new System.Drawing.Size(1600, 464);
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
            // _summaryPanel
            //
            this._summaryPanel.Appearance.BackColor = System.Drawing.Color.FromArgb(235, 235, 235);
            this._summaryPanel.Appearance.Options.UseBackColor = true;
            this._summaryPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._summaryPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._summaryPanel.Location = new System.Drawing.Point(0, 574);
            this._summaryPanel.Name = "_summaryPanel";
            this._summaryPanel.Size = new System.Drawing.Size(1600, 36);
            this._summaryPanel.TabIndex = 3;
            //
            // WeighDataManagementForm
            //
            // 시스템 DPI/글꼴 설정에 따른 자동 재스케일로 픽셀 단위 레이아웃이 어긋나는 것을
            // 막기 위해 None으로 끈다(계량 화면 설정 팝업과 같은 이유).
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1600, 610);
            this.Controls.Add(this._gridControl);
            this.Controls.Add(this._summaryPanel);
            this.Controls.Add(this._filterPanel);
            this.Controls.Add(this._headerPanel);
            this.Name = "WeighDataManagementForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "계량 데이터 관리";
            ((System.ComponentModel.ISupportInitialize)(this._headerPanel)).EndInit();
            this._headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._filterPanel)).EndInit();
            this._filterPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._gridControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._gridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._summaryPanel)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
