using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;

namespace ColumbusWeighing.Forms
{
    /// <summary>
    /// 거래처 마스터(dbo.CUSTOMER) 조회 화면. 참고 화면(TS2020 거래처 관리)과 같은 구성(조회내역
    /// 검색 + 목록)이지만, 이 프로그램은 조회 전용이라 추가/수정/삭제 기능은 두지 않는다
    /// (제품 관리와 마찬가지로 실제 사용 여부가 정해지지 않아 보류 - 필요해지면 그때 추가한다).
    /// </summary>
    public partial class CustomerManagementForm : XtraForm
    {
        private readonly ICustomerRepository _repository;

        public CustomerManagementForm(ICustomerRepository repository)
        {
            InitializeComponent();

            _repository = repository;

            BuildColumns();
            ComnGridFunc.GridStyleBasicSetting(_gridView);
            _gridView.OptionsView.ShowGroupPanel = false;
            _gridView.OptionsBehavior.Editable = false;
            _gridView.CustomColumnDisplayText += GridView_CustomColumnDisplayText;

            _gridControl.DataSource = _repository.Records;

            _btnRetrieve.Click += (s, e) => Retrieve();
            _btnPrint.Click += (s, e) => PrintList();
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

            AddColumn("BranchCode", "지점", 60);
            AddColumn("CustomerCode", "거래처코드", 90);
            AddColumn("CustomerName", "거래처명", 180);
            AddColumn("CeoName", "대표", 90);
            AddColumn("ManagerName", "담당자", 90);
            AddColumn("Tel", "전화", 120);
            AddColumn("Fax", "팩스", 120);
            AddColumn("Remark", "비고", 180);
            AddColumn("SyncedAt", "동기화일시", 140, "yyyy-MM-dd HH:mm");
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

        private void GridView_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "BranchCode" && e.Value is string branchCode)
            {
                e.DisplayText = branchCode.ToDisplayString();
            }
        }

        private void PrintList()
        {
            // TODO: XtraReports 로 작성된 거래처 목록 출력 연결.
            ComnFunc.gp_PrintMessage("거래처 목록 인쇄는 준비 중입니다.", "거래처 관리", MessageType.알림);
        }
    }
}
