using System.Linq;
using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;

namespace ColumbusWeighing.Forms
{
    /// <summary>
    /// 제품 마스터(dbo.PRODUCT) 조회 화면. 참고 화면(TS2020 제품 관리)과 같은 구성(조회내역
    /// 검색 + 목록)이지만, 이 프로그램은 조회 전용이라 추가/수정/삭제 기능은 두지 않는다
    /// (실제 사용 여부가 정해지지 않아 보류 - 필요해지면 그때 추가한다).
    /// </summary>
    public partial class ProductManagementForm : XtraForm
    {
        private readonly IProductRepository _repository;

        public ProductManagementForm(IProductRepository repository)
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
            AddColumn("ProductCode", "제품코드", 90);
            AddColumn("ProductName", "제품명", 180);
            AddColumn("Unit", "단위", 60);
            AddColumn("UnitPrice", "단가", 90, "N0");
            AddColumn("LossWeight", "감량(kg)", 90, "N0");
            AddColumn("LossRate", "감량률(%)", 80, "N1");
            AddColumn("Remark", "비고", 220);
            AddColumn("SyncedAt", "동기화일시", 140, "yyyy-MM-dd HH:mm");
        }

        private GridColumn AddColumn(string fieldName, string caption, int width, string format = null)
        {
            var column = _gridView.Columns.AddVisible(fieldName, caption);
            column.Width = width;
            column.OptionsColumn.AllowEdit = false;

            if (!string.IsNullOrEmpty(format))
            {
                column.DisplayFormat.FormatType = format == "yyyy-MM-dd HH:mm"
                    ? DevExpress.Utils.FormatType.DateTime
                    : DevExpress.Utils.FormatType.Numeric;
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
            else if (e.Column.FieldName == "LossRate" && e.Value is decimal lossRate)
            {
                e.DisplayText = lossRate.ToString("N1") + "%";
            }
        }

        private static readonly string[] PrintHeaders = { "지점", "제품코드", "제품명", "단위", "단가", "감량(kg)", "감량률(%)", "비고" };
        private static readonly int[] PrintColumnWidths = { 6, 10, 18, 6, 9, 9, 9, 22 };

        private void PrintList()
        {
            var rows = _repository.Records.Select(r => new[]
            {
                r.BranchCode.ToDisplayString(),
                r.ProductCode,
                r.ProductName,
                r.Unit,
                r.UnitPrice.HasValue ? r.UnitPrice.Value.ToString("N0") : string.Empty,
                r.LossWeight.HasValue ? r.LossWeight.Value.ToString("N0") : string.Empty,
                r.LossRate.HasValue ? r.LossRate.Value.ToString("N1") : string.Empty,
                r.Remark,
            }).ToArray();

            GridPrinter.ShowPrintPreview(this, PrintHeaders, PrintColumnWidths, rows);
        }
    }
}
