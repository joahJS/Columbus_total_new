using System;
using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;

namespace ColumbusWeighing.Forms
{
    /// <summary>
    /// 사용자(로그인 계정) 관리 화면. 조회/추가/수정/삭제를 제공한다. 비밀번호는 여기서 직접
    /// 보여주지 않고, 추가/수정 팝업(UserEditForm)에서만 입력받는다.
    /// </summary>
    public partial class UserManagementForm : XtraForm
    {
        private readonly IUserRepository _repository;
        private readonly string _loginUserName;

        public UserManagementForm(IUserRepository repository, string loginUserName)
        {
            InitializeComponent();

            _repository = repository;
            _loginUserName = loginUserName;

            BuildColumns();
            ComnGridFunc.GridStyleBasicSetting(_gridView);
            _gridView.OptionsView.ShowGroupPanel = false;
            _gridView.OptionsBehavior.Editable = false;

            _gridControl.DataSource = _repository.Records;

            _btnAdd.Click += (s, e) => OpenAddForm();
            _btnEdit.Click += (s, e) => OpenEditForm();
            _btnDelete.Click += (s, e) => DeleteSelected();
            _btnRetrieve.Click += (s, e) => Retrieve();
            _btnClose.Click += (s, e) => Close();
            _searchEdit.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    Retrieve();
                }
            };

            KeyPreview = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F5:
                    Retrieve();
                    return true;
                case Keys.Escape:
                    Close();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private UserAccount SelectedAccount
        {
            get { return _gridView.GetFocusedRow() as UserAccount; }
        }

        private void Retrieve()
        {
            _repository.Refresh(_searchEdit.Text.Trim());
        }

        private void OpenAddForm()
        {
            using (var form = new UserEditForm(_repository, null, _loginUserName))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    Retrieve();
                }
            }
        }

        private void OpenEditForm()
        {
            var account = SelectedAccount;
            if (account == null)
            {
                ComnFunc.gp_PrintMessage("수정할 사용자를 목록에서 먼저 선택하세요.", "안내", MessageType.알림);
                return;
            }

            using (var form = new UserEditForm(_repository, account, _loginUserName))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    Retrieve();
                }
            }
        }

        private void DeleteSelected()
        {
            var account = SelectedAccount;
            if (account == null)
            {
                ComnFunc.gp_PrintMessage("삭제할 사용자를 목록에서 먼저 선택하세요.", "안내", MessageType.알림);
                return;
            }

            var confirmed = ComnFunc.gp_PrintQuestion(
                string.Format("'{0}' 계정을 삭제하시겠습니까?", account.LoginId), "사용자 삭제", MessageType.경고);
            if (!confirmed)
            {
                return;
            }

            try
            {
                _repository.Delete(account.Id);
            }
            catch (InvalidOperationException ex)
            {
                ComnFunc.gp_PrintMessage(ex.Message, "안내", MessageType.경고);
                return;
            }
            catch (Exception ex)
            {
                ComnFunc.gp_PrintMessage("삭제 중 오류가 발생했습니다.\r\n" + ex.Message, "오류", MessageType.오류);
                return;
            }

            ComnFunc.gp_PrintMessage("삭제되었습니다.", "안내", MessageType.알림);
            Retrieve();
        }

        private void BuildColumns()
        {
            _gridView.Columns.Clear();

            AddColumn("LoginId", "ID", 90);
            AddColumn("DisplayName", "사용자", 110);
            AddColumn("Phone", "전화번호", 110);
            AddColumn("Remark", "비고", 200);
            AddColumn("CanPrint", "인쇄", 55);
            AddColumn("CanEdit", "편집", 55);
            AddColumn("CanDelete", "삭제", 55);
            AddColumn("IsAdmin", "관리자", 55);
            AddColumn("ModifiedBy", "수정자", 80);
            AddColumn("ModifiedAt", "수정일", 140, "yyyy-MM-dd HH:mm");
        }

        private GridColumn AddColumn(string fieldName, string caption, int width, string format = null)
        {
            var column = _gridView.Columns.AddVisible(fieldName, caption);
            column.Width = width;
            column.OptionsColumn.AllowEdit = false;

            if (!string.IsNullOrEmpty(format))
            {
                column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                column.DisplayFormat.FormatString = format;
            }

            return column;
        }
    }
}
