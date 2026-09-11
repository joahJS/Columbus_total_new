using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;

namespace ColumbusWeighing.Forms
{
    /// <summary>
    /// 사용자(로그인 계정) 조회 화면. 참고 화면(TS2020 사용자 관리)과 같은 구성(조회 + 목록)이지만,
    /// 이 프로그램은 조회 전용이라 추가/수정/삭제(계정 생성, 비밀번호 변경 등)는 두지 않는다
    /// (제품 관리와 마찬가지로 필요해지면 그때 추가한다).
    /// </summary>
    public partial class UserManagementForm : XtraForm
    {
        private readonly IUserRepository _repository;

        public UserManagementForm(IUserRepository repository)
        {
            InitializeComponent();

            _repository = repository;

            BuildColumns();
            ComnGridFunc.GridStyleBasicSetting(_gridView);
            _gridView.OptionsView.ShowGroupPanel = false;
            _gridView.OptionsBehavior.Editable = false;

            _gridControl.DataSource = _repository.Records;

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

        private void Retrieve()
        {
            _repository.Refresh(_searchEdit.Text.Trim());
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
