namespace ColumbusWeighing.Forms
{
    partial class UserEditForm
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

        private DevExpress.XtraEditors.LabelControl _loginIdLabel;
        private DevExpress.XtraEditors.TextEdit _loginIdEdit;
        private DevExpress.XtraEditors.LabelControl _displayNameLabel;
        private DevExpress.XtraEditors.TextEdit _displayNameEdit;
        private DevExpress.XtraEditors.LabelControl _phoneLabel;
        private DevExpress.XtraEditors.TextEdit _phoneEdit;
        private DevExpress.XtraEditors.LabelControl _remarkLabel;
        private DevExpress.XtraEditors.TextEdit _remarkEdit;
        private DevExpress.XtraEditors.CheckEdit _chkCanPrint;
        private DevExpress.XtraEditors.CheckEdit _chkCanEdit;
        private DevExpress.XtraEditors.CheckEdit _chkCanDelete;
        private DevExpress.XtraEditors.CheckEdit _chkIsAdmin;
        private DevExpress.XtraEditors.LabelControl _passwordLabel;
        private DevExpress.XtraEditors.TextEdit _passwordEdit;
        private DevExpress.XtraEditors.LabelControl _passwordConfirmLabel;
        private DevExpress.XtraEditors.TextEdit _passwordConfirmEdit;
        private DevExpress.XtraEditors.LabelControl _passwordHintLabel;
        private DevExpress.XtraEditors.SimpleButton _btnSave;
        private DevExpress.XtraEditors.SimpleButton _btnCancel;

        private void InitializeComponent()
        {
            this._loginIdLabel = new DevExpress.XtraEditors.LabelControl();
            this._loginIdEdit = new DevExpress.XtraEditors.TextEdit();
            this._displayNameLabel = new DevExpress.XtraEditors.LabelControl();
            this._displayNameEdit = new DevExpress.XtraEditors.TextEdit();
            this._phoneLabel = new DevExpress.XtraEditors.LabelControl();
            this._phoneEdit = new DevExpress.XtraEditors.TextEdit();
            this._remarkLabel = new DevExpress.XtraEditors.LabelControl();
            this._remarkEdit = new DevExpress.XtraEditors.TextEdit();
            this._chkCanPrint = new DevExpress.XtraEditors.CheckEdit();
            this._chkCanEdit = new DevExpress.XtraEditors.CheckEdit();
            this._chkCanDelete = new DevExpress.XtraEditors.CheckEdit();
            this._chkIsAdmin = new DevExpress.XtraEditors.CheckEdit();
            this._passwordLabel = new DevExpress.XtraEditors.LabelControl();
            this._passwordEdit = new DevExpress.XtraEditors.TextEdit();
            this._passwordConfirmLabel = new DevExpress.XtraEditors.LabelControl();
            this._passwordConfirmEdit = new DevExpress.XtraEditors.TextEdit();
            this._passwordHintLabel = new DevExpress.XtraEditors.LabelControl();
            this._btnSave = new DevExpress.XtraEditors.SimpleButton();
            this._btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this._loginIdEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._displayNameEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._phoneEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._remarkEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._chkCanPrint.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._chkCanEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._chkCanDelete.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._chkIsAdmin.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._passwordEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._passwordConfirmEdit.Properties)).BeginInit();
            this.SuspendLayout();
            //
            // _loginIdLabel
            //
            this._loginIdLabel.Location = new System.Drawing.Point(24, 28);
            this._loginIdLabel.Name = "_loginIdLabel";
            this._loginIdLabel.Size = new System.Drawing.Size(12, 13);
            this._loginIdLabel.TabIndex = 0;
            this._loginIdLabel.Text = "ID";
            //
            // _loginIdEdit
            //
            this._loginIdEdit.Location = new System.Drawing.Point(120, 25);
            this._loginIdEdit.Name = "_loginIdEdit";
            this._loginIdEdit.Size = new System.Drawing.Size(200, 20);
            this._loginIdEdit.TabIndex = 1;
            //
            // _displayNameLabel
            //
            this._displayNameLabel.Location = new System.Drawing.Point(24, 64);
            this._displayNameLabel.Name = "_displayNameLabel";
            this._displayNameLabel.Size = new System.Drawing.Size(30, 13);
            this._displayNameLabel.TabIndex = 2;
            this._displayNameLabel.Text = "사용자명";
            //
            // _displayNameEdit
            //
            this._displayNameEdit.Location = new System.Drawing.Point(120, 61);
            this._displayNameEdit.Name = "_displayNameEdit";
            this._displayNameEdit.Size = new System.Drawing.Size(200, 20);
            this._displayNameEdit.TabIndex = 3;
            //
            // _phoneLabel
            //
            this._phoneLabel.Location = new System.Drawing.Point(24, 100);
            this._phoneLabel.Name = "_phoneLabel";
            this._phoneLabel.Size = new System.Drawing.Size(42, 13);
            this._phoneLabel.TabIndex = 4;
            this._phoneLabel.Text = "전화번호";
            //
            // _phoneEdit
            //
            this._phoneEdit.Location = new System.Drawing.Point(120, 97);
            this._phoneEdit.Name = "_phoneEdit";
            this._phoneEdit.Size = new System.Drawing.Size(200, 20);
            this._phoneEdit.TabIndex = 5;
            //
            // _remarkLabel
            //
            this._remarkLabel.Location = new System.Drawing.Point(24, 136);
            this._remarkLabel.Name = "_remarkLabel";
            this._remarkLabel.Size = new System.Drawing.Size(18, 13);
            this._remarkLabel.TabIndex = 6;
            this._remarkLabel.Text = "비고";
            //
            // _remarkEdit
            //
            this._remarkEdit.Location = new System.Drawing.Point(120, 133);
            this._remarkEdit.Name = "_remarkEdit";
            this._remarkEdit.Size = new System.Drawing.Size(200, 20);
            this._remarkEdit.TabIndex = 7;
            //
            // _chkCanPrint
            //
            this._chkCanPrint.Location = new System.Drawing.Point(24, 172);
            this._chkCanPrint.Name = "_chkCanPrint";
            this._chkCanPrint.Properties.Caption = "인쇄";
            this._chkCanPrint.Size = new System.Drawing.Size(75, 20);
            this._chkCanPrint.TabIndex = 8;
            //
            // _chkCanEdit
            //
            this._chkCanEdit.Location = new System.Drawing.Point(190, 172);
            this._chkCanEdit.Name = "_chkCanEdit";
            this._chkCanEdit.Properties.Caption = "편집";
            this._chkCanEdit.Size = new System.Drawing.Size(75, 20);
            this._chkCanEdit.TabIndex = 9;
            //
            // _chkCanDelete
            //
            this._chkCanDelete.Location = new System.Drawing.Point(24, 204);
            this._chkCanDelete.Name = "_chkCanDelete";
            this._chkCanDelete.Properties.Caption = "삭제";
            this._chkCanDelete.Size = new System.Drawing.Size(75, 20);
            this._chkCanDelete.TabIndex = 10;
            //
            // _chkIsAdmin
            //
            this._chkIsAdmin.Location = new System.Drawing.Point(190, 204);
            this._chkIsAdmin.Name = "_chkIsAdmin";
            this._chkIsAdmin.Properties.Caption = "관리자";
            this._chkIsAdmin.Size = new System.Drawing.Size(75, 20);
            this._chkIsAdmin.TabIndex = 11;
            //
            // _passwordLabel
            //
            this._passwordLabel.Location = new System.Drawing.Point(24, 244);
            this._passwordLabel.Name = "_passwordLabel";
            this._passwordLabel.Size = new System.Drawing.Size(48, 13);
            this._passwordLabel.TabIndex = 12;
            this._passwordLabel.Text = "비밀번호";
            //
            // _passwordEdit
            //
            this._passwordEdit.Location = new System.Drawing.Point(120, 241);
            this._passwordEdit.Name = "_passwordEdit";
            this._passwordEdit.Properties.UseSystemPasswordChar = true;
            this._passwordEdit.Size = new System.Drawing.Size(200, 20);
            this._passwordEdit.TabIndex = 13;
            //
            // _passwordConfirmLabel
            //
            this._passwordConfirmLabel.Location = new System.Drawing.Point(24, 280);
            this._passwordConfirmLabel.Name = "_passwordConfirmLabel";
            this._passwordConfirmLabel.Size = new System.Drawing.Size(72, 13);
            this._passwordConfirmLabel.TabIndex = 14;
            this._passwordConfirmLabel.Text = "비밀번호 확인";
            //
            // _passwordConfirmEdit
            //
            this._passwordConfirmEdit.Location = new System.Drawing.Point(120, 277);
            this._passwordConfirmEdit.Name = "_passwordConfirmEdit";
            this._passwordConfirmEdit.Properties.UseSystemPasswordChar = true;
            this._passwordConfirmEdit.Size = new System.Drawing.Size(200, 20);
            this._passwordConfirmEdit.TabIndex = 15;
            //
            // _passwordHintLabel
            //
            this._passwordHintLabel.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this._passwordHintLabel.Appearance.Options.UseForeColor = true;
            this._passwordHintLabel.Location = new System.Drawing.Point(120, 304);
            this._passwordHintLabel.Name = "_passwordHintLabel";
            this._passwordHintLabel.Size = new System.Drawing.Size(200, 26);
            this._passwordHintLabel.TabIndex = 16;
            this._passwordHintLabel.Text = "비워두면 비밀번호를 바꾸지 않습니다.";
            //
            // _btnSave
            //
            this._btnSave.Location = new System.Drawing.Point(120, 340);
            this._btnSave.Name = "_btnSave";
            this._btnSave.Size = new System.Drawing.Size(95, 32);
            this._btnSave.TabIndex = 17;
            this._btnSave.Text = "저장(F3)";
            //
            // _btnCancel
            //
            this._btnCancel.Location = new System.Drawing.Point(225, 340);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(95, 32);
            this._btnCancel.TabIndex = 18;
            this._btnCancel.Text = "취소(ESC)";
            //
            // UserEditForm
            //
            this.AcceptButton = this._btnSave;
            this.CancelButton = this._btnCancel;
            this.ClientSize = new System.Drawing.Size(360, 396);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnSave);
            this.Controls.Add(this._passwordHintLabel);
            this.Controls.Add(this._passwordConfirmEdit);
            this.Controls.Add(this._passwordConfirmLabel);
            this.Controls.Add(this._passwordEdit);
            this.Controls.Add(this._passwordLabel);
            this.Controls.Add(this._chkIsAdmin);
            this.Controls.Add(this._chkCanDelete);
            this.Controls.Add(this._chkCanEdit);
            this.Controls.Add(this._chkCanPrint);
            this.Controls.Add(this._remarkEdit);
            this.Controls.Add(this._remarkLabel);
            this.Controls.Add(this._phoneEdit);
            this.Controls.Add(this._phoneLabel);
            this.Controls.Add(this._displayNameEdit);
            this.Controls.Add(this._displayNameLabel);
            this.Controls.Add(this._loginIdEdit);
            this.Controls.Add(this._loginIdLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "사용자 추가";
            ((System.ComponentModel.ISupportInitialize)(this._loginIdEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._displayNameEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._phoneEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._remarkEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._chkCanPrint.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._chkCanEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._chkCanDelete.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._chkIsAdmin.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._passwordEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._passwordConfirmEdit.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
