using System;
using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;

namespace ColumbusWeighing.Forms
{
    /// <summary>
    /// 사용자 계정 추가/수정 화면. editingAccount가 null이면 추가, 아니면 수정 모드다.
    /// 수정 모드에서 비밀번호 칸을 비워두면 비밀번호는 바꾸지 않는다.
    /// </summary>
    public partial class UserEditForm : XtraForm
    {
        private readonly IUserRepository _repository;
        private readonly UserAccount _editingAccount;
        private readonly string _modifiedBy;

        public UserEditForm(IUserRepository repository, UserAccount editingAccount, string modifiedBy)
        {
            InitializeComponent();

            _repository = repository;
            _editingAccount = editingAccount;
            _modifiedBy = modifiedBy;

            var isEditMode = _editingAccount != null;
            Text = isEditMode ? "사용자 수정" : "사용자 추가";
            _passwordHintLabel.Visible = isEditMode;

            if (isEditMode)
            {
                _loginIdEdit.Text = _editingAccount.LoginId ?? string.Empty;
                _displayNameEdit.Text = _editingAccount.DisplayName ?? string.Empty;
                _phoneEdit.Text = _editingAccount.Phone ?? string.Empty;
                _remarkEdit.Text = _editingAccount.Remark ?? string.Empty;
                _chkCanPrint.Checked = _editingAccount.CanPrint;
                _chkCanEdit.Checked = _editingAccount.CanEdit;
                _chkCanDelete.Checked = _editingAccount.CanDelete;
                _chkIsAdmin.Checked = _editingAccount.IsAdmin;
            }

            _btnSave.Click += (s, e) => Save();
            _btnCancel.Click += (s, e) => Close();

            KeyPreview = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Save()
        {
            var isEditMode = _editingAccount != null;
            var loginId = _loginIdEdit.Text.Trim();
            var displayName = _displayNameEdit.Text.Trim();
            var password = _passwordEdit.Text;
            var passwordConfirm = _passwordConfirmEdit.Text;

            if (string.IsNullOrEmpty(loginId))
            {
                ComnFunc.gp_PrintMessage("아이디를 입력하세요.", "안내", MessageType.경고);
                _loginIdEdit.Focus();
                return;
            }

            if (string.IsNullOrEmpty(displayName))
            {
                ComnFunc.gp_PrintMessage("사용자명을 입력하세요.", "안내", MessageType.경고);
                _displayNameEdit.Focus();
                return;
            }

            if (!isEditMode && string.IsNullOrEmpty(password))
            {
                ComnFunc.gp_PrintMessage("비밀번호를 입력하세요.", "안내", MessageType.경고);
                _passwordEdit.Focus();
                return;
            }

            if (password != passwordConfirm)
            {
                ComnFunc.gp_PrintMessage("비밀번호가 서로 일치하지 않습니다.", "안내", MessageType.경고);
                _passwordConfirmEdit.Focus();
                return;
            }

            var account = new UserAccount
            {
                Id = isEditMode ? _editingAccount.Id : 0,
                LoginId = loginId,
                DisplayName = displayName,
                Phone = _phoneEdit.Text.Trim(),
                Remark = _remarkEdit.Text.Trim(),
                CanPrint = _chkCanPrint.Checked,
                CanEdit = _chkCanEdit.Checked,
                CanDelete = _chkCanDelete.Checked,
                IsAdmin = _chkIsAdmin.Checked,
            };

            try
            {
                if (isEditMode)
                {
                    _repository.Update(account, password, _modifiedBy);
                }
                else
                {
                    _repository.Add(account, password, _modifiedBy);
                }
            }
            catch (DuplicateLoginIdException ex)
            {
                ComnFunc.gp_PrintMessage(ex.Message, "안내", MessageType.경고);
                _loginIdEdit.Focus();
                return;
            }
            catch (InvalidOperationException ex)
            {
                ComnFunc.gp_PrintMessage(ex.Message, "안내", MessageType.경고);
                return;
            }
            catch (Exception ex)
            {
                ComnFunc.gp_PrintMessage("저장 중 오류가 발생했습니다.\r\n" + ex.Message, "오류", MessageType.오류);
                return;
            }

            ComnFunc.gp_PrintMessage("저장되었습니다.", "안내", MessageType.알림);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
