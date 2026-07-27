namespace ColumbusWeighing.Forms
{
    partial class LoginForm
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

        private DevExpress.XtraEditors.LabelControl _idLabel;
        private DevExpress.XtraEditors.LabelControl _pwLabel;
        private DevExpress.XtraEditors.TextEdit _idEdit;
        private DevExpress.XtraEditors.TextEdit _pwEdit;
        private DevExpress.XtraEditors.SimpleButton _btnOk;
        private DevExpress.XtraEditors.SimpleButton _btnCancel;

        private void InitializeComponent()
        {
            this._idLabel = new DevExpress.XtraEditors.LabelControl();
            this._pwLabel = new DevExpress.XtraEditors.LabelControl();
            this._idEdit = new DevExpress.XtraEditors.TextEdit();
            this._pwEdit = new DevExpress.XtraEditors.TextEdit();
            this._btnOk = new DevExpress.XtraEditors.SimpleButton();
            this._btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this._idEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._pwEdit.Properties)).BeginInit();
            this.SuspendLayout();
            //
            // _idLabel
            //
            this._idLabel.Location = new System.Drawing.Point(24, 28);
            this._idLabel.Name = "_idLabel";
            this._idLabel.Size = new System.Drawing.Size(42, 13);
            this._idLabel.TabIndex = 0;
            this._idLabel.Text = "사용자ID";
            //
            // _pwLabel
            //
            this._pwLabel.Location = new System.Drawing.Point(24, 64);
            this._pwLabel.Name = "_pwLabel";
            this._pwLabel.Size = new System.Drawing.Size(42, 13);
            this._pwLabel.TabIndex = 1;
            this._pwLabel.Text = "비밀번호";
            //
            // _idEdit
            //
            this._idEdit.Location = new System.Drawing.Point(90, 25);
            this._idEdit.Name = "_idEdit";
            this._idEdit.Size = new System.Drawing.Size(180, 20);
            this._idEdit.TabIndex = 2;
            //
            // _pwEdit
            //
            this._pwEdit.Location = new System.Drawing.Point(90, 61);
            this._pwEdit.Name = "_pwEdit";
            this._pwEdit.Properties.PasswordChar = '*';
            this._pwEdit.Properties.UseSystemPasswordChar = true;
            this._pwEdit.Size = new System.Drawing.Size(180, 20);
            this._pwEdit.TabIndex = 3;
            //
            // _btnOk
            //
            this._btnOk.Location = new System.Drawing.Point(90, 100);
            this._btnOk.Name = "_btnOk";
            this._btnOk.Size = new System.Drawing.Size(85, 28);
            this._btnOk.TabIndex = 4;
            this._btnOk.Text = "로그인";
            //
            // _btnCancel
            //
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(185, 100);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(85, 28);
            this._btnCancel.TabIndex = 5;
            this._btnCancel.Text = "취소";
            //
            // LoginForm
            //
            this.AcceptButton = this._btnOk;
            this.CancelButton = this._btnCancel;
            this.ClientSize = new System.Drawing.Size(294, 154);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._pwEdit);
            this.Controls.Add(this._idEdit);
            this.Controls.Add(this._pwLabel);
            this.Controls.Add(this._idLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "로그인";
            ((System.ComponentModel.ISupportInitialize)(this._idEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._pwEdit.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
