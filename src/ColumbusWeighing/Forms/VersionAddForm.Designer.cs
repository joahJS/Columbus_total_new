namespace ColumbusWeighing.Forms
{
    partial class VersionAddForm
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

        private DevExpress.XtraEditors.LabelControl _versionLabel;
        private DevExpress.XtraEditors.TextEdit _tx_Version;
        private DevExpress.XtraEditors.LabelControl _fileNameLabel;
        private DevExpress.XtraEditors.TextEdit _tx_FileName;
        private DevExpress.XtraEditors.LabelControl _fileSizeLabel;
        private DevExpress.XtraEditors.TextEdit _tx_FileSize;
        private DevExpress.XtraEditors.LabelControl _remarkLabel;
        private DevExpress.XtraEditors.MemoEdit _mm_Remark;
        private DevExpress.XtraEditors.SimpleButton _btnUpload;
        private DevExpress.XtraEditors.SimpleButton _btnSave;
        private DevExpress.XtraEditors.SimpleButton _btnClose;

        private void InitializeComponent()
        {
            this._versionLabel = new DevExpress.XtraEditors.LabelControl();
            this._tx_Version = new DevExpress.XtraEditors.TextEdit();
            this._fileNameLabel = new DevExpress.XtraEditors.LabelControl();
            this._tx_FileName = new DevExpress.XtraEditors.TextEdit();
            this._fileSizeLabel = new DevExpress.XtraEditors.LabelControl();
            this._tx_FileSize = new DevExpress.XtraEditors.TextEdit();
            this._remarkLabel = new DevExpress.XtraEditors.LabelControl();
            this._mm_Remark = new DevExpress.XtraEditors.MemoEdit();
            this._btnUpload = new DevExpress.XtraEditors.SimpleButton();
            this._btnSave = new DevExpress.XtraEditors.SimpleButton();
            this._btnClose = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this._tx_Version.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._tx_FileName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._tx_FileSize.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._mm_Remark.Properties)).BeginInit();
            this.SuspendLayout();
            //
            // _versionLabel
            //
            this._versionLabel.Location = new System.Drawing.Point(24, 28);
            this._versionLabel.Name = "_versionLabel";
            this._versionLabel.Size = new System.Drawing.Size(60, 13);
            this._versionLabel.TabIndex = 0;
            this._versionLabel.Text = "VersionID";
            //
            // _tx_Version
            //
            this._tx_Version.Location = new System.Drawing.Point(120, 25);
            this._tx_Version.Name = "_tx_Version";
            this._tx_Version.Size = new System.Drawing.Size(200, 20);
            this._tx_Version.TabIndex = 1;
            //
            // _fileNameLabel
            //
            this._fileNameLabel.Location = new System.Drawing.Point(24, 64);
            this._fileNameLabel.Name = "_fileNameLabel";
            this._fileNameLabel.Size = new System.Drawing.Size(30, 13);
            this._fileNameLabel.TabIndex = 2;
            this._fileNameLabel.Text = "파일명";
            //
            // _tx_FileName
            //
            this._tx_FileName.Enabled = false;
            this._tx_FileName.Location = new System.Drawing.Point(120, 61);
            this._tx_FileName.Name = "_tx_FileName";
            this._tx_FileName.Size = new System.Drawing.Size(200, 20);
            this._tx_FileName.TabIndex = 3;
            this._tx_FileName.TabStop = false;
            //
            // _fileSizeLabel
            //
            this._fileSizeLabel.Location = new System.Drawing.Point(24, 100);
            this._fileSizeLabel.Name = "_fileSizeLabel";
            this._fileSizeLabel.Size = new System.Drawing.Size(37, 13);
            this._fileSizeLabel.TabIndex = 4;
            this._fileSizeLabel.Text = "파일크기";
            //
            // _tx_FileSize
            //
            this._tx_FileSize.Enabled = false;
            this._tx_FileSize.Location = new System.Drawing.Point(120, 97);
            this._tx_FileSize.Name = "_tx_FileSize";
            this._tx_FileSize.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this._tx_FileSize.Properties.Appearance.Options.UseTextOptions = true;
            this._tx_FileSize.Size = new System.Drawing.Size(200, 20);
            this._tx_FileSize.TabIndex = 5;
            this._tx_FileSize.TabStop = false;
            //
            // _remarkLabel
            //
            this._remarkLabel.Location = new System.Drawing.Point(24, 136);
            this._remarkLabel.Name = "_remarkLabel";
            this._remarkLabel.Size = new System.Drawing.Size(18, 13);
            this._remarkLabel.TabIndex = 6;
            this._remarkLabel.Text = "비고";
            //
            // _mm_Remark
            //
            this._mm_Remark.Location = new System.Drawing.Point(120, 133);
            this._mm_Remark.Name = "_mm_Remark";
            this._mm_Remark.Size = new System.Drawing.Size(200, 90);
            this._mm_Remark.TabIndex = 7;
            //
            // _btnUpload
            //
            this._btnUpload.Location = new System.Drawing.Point(30, 240);
            this._btnUpload.Name = "_btnUpload";
            this._btnUpload.Size = new System.Drawing.Size(100, 32);
            this._btnUpload.TabIndex = 8;
            this._btnUpload.Text = "파일업로드";
            //
            // _btnSave
            //
            this._btnSave.Location = new System.Drawing.Point(150, 240);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(85, 32);
            this._btnSave.TabIndex = 9;
            this._btnSave.Text = "저장(F3)";
            //
            // _btnClose
            //
            this._btnClose.Location = new System.Drawing.Point(245, 240);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(85, 32);
            this._btnClose.TabIndex = 10;
            this._btnClose.Text = "닫기(ESC)";
            //
            // VersionAddForm
            //
            this.AcceptButton = this._btnSave;
            this.CancelButton = this._btnClose;
            this.ClientSize = new System.Drawing.Size(360, 300);
            this.Controls.Add(this._btnClose);
            this.Controls.Add(this._btnSave);
            this.Controls.Add(this._btnUpload);
            this.Controls.Add(this._mm_Remark);
            this.Controls.Add(this._remarkLabel);
            this.Controls.Add(this._tx_FileSize);
            this.Controls.Add(this._fileSizeLabel);
            this.Controls.Add(this._tx_FileName);
            this.Controls.Add(this._fileNameLabel);
            this.Controls.Add(this._tx_Version);
            this.Controls.Add(this._versionLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VersionAddForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "새 버전 추가";
            ((System.ComponentModel.ISupportInitialize)(this._tx_Version.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._tx_FileName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._tx_FileSize.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._mm_Remark.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
