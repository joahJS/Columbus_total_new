using System;
using System.Windows.Forms;
using ColumbusWeighing.Models;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;

namespace ColumbusWeighing.Controls
{
    /// <summary>
    /// "1차 계량" 패널. 아직 2차 계량이 완료되지 않은(대기 중) 계근 건 목록을 보여주고,
    /// F5(1차계량 등록) / F6(선택 건 2차계량 완료) 를 처리한다.
    /// </summary>
    public partial class FirstWeighingControl : XtraUserControl
    {
        private IWeighingRepository _repository;
        private IScaleIndicatorService _scaleService;
        private AppLogService _logService;

        public FirstWeighingControl()
        {
            InitializeComponent();
            BuildColumns();

            _gridView.CustomColumnDisplayText += GridView_CustomColumnDisplayText;
            _btnFirstWeighing.Click += (s, e) => RegisterFirstWeighing();
            _btnSecondWeighing.Click += (s, e) => CompleteSecondWeighingForSelected();
            _btnFirstSlip.Click += (s, e) => PrintFirstSlip();
        }

        /// <summary>2차 계량이 완료되어 해당 건이 2차계량 화면으로 이동했을 때 발생.</summary>
        public event EventHandler SecondWeighingCompleted;

        /// <summary>선택된 행의 계근 기록(없으면 null).</summary>
        public WeighingRecord SelectedRecord
        {
            get { return _gridView.GetFocusedRow() as WeighingRecord; }
        }

        public void Initialize(IWeighingRepository repository, IScaleIndicatorService scaleService, AppLogService logService)
        {
            _repository = repository;
            _scaleService = scaleService;
            _logService = logService;

            _gridControl.DataSource = _repository.Records;
            _gridView.ActiveFilterString = "[IsCompleted] = False";
        }

        private void BuildColumns()
        {
            _gridView.Columns.Clear();

            AddColumn("FirstDateTime", "1차 계량일", 90, "yyyy-MM-dd");
            AddColumn("WeighSeq", "계량순번", 60);
            AddColumn("FirstDateTime", "1차시간", 60, "HH:mm");
            AddColumn("VehicleNo", "차량번호", 70);
            AddColumn("CustomerName", "거래처명", 110);
            AddColumn("ProductName", "제품명", 100);
            AddColumn("FirstWeight", "1차중량", 80, "N0");
            AddInOutColumn();
            AddColumn("WeigherName", "계량자", 130);
            AddColumn("Remark", "비고", 100);
        }

        private GridColumn AddColumn(string fieldName, string caption, int width, string format = null)
        {
            var column = _gridView.Columns.AddVisible(fieldName, caption);
            column.Width = width;
            column.OptionsColumn.AllowEdit = false;

            if (!string.IsNullOrEmpty(format))
            {
                var isDateTimeFormat = format == "yyyy-MM-dd" || format == "HH:mm";
                column.DisplayFormat.FormatType = isDateTimeFormat
                    ? DevExpress.Utils.FormatType.DateTime
                    : DevExpress.Utils.FormatType.Numeric;
                column.DisplayFormat.FormatString = format;
            }

            return column;
        }

        private void AddInOutColumn()
        {
            var column = _gridView.Columns.AddVisible("InOutType", "입/출고");
            column.Width = 60;
            column.OptionsColumn.AllowEdit = false;
        }

        private void GridView_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "InOutType" && e.Value is InOutType inOut)
            {
                e.DisplayText = inOut.ToDisplayString();
            }
        }

        /// <summary>MainForm 의 F5 단축키에서 호출.</summary>
        public void RegisterFirstWeighing()
        {
            if (_repository == null || _scaleService == null)
            {
                return;
            }

            var vehicleNo = XtraInputBox.Show("차량번호를 입력하세요.", "1차 계량 등록", string.Empty);
            if (string.IsNullOrWhiteSpace(vehicleNo))
            {
                return;
            }

            var customerName = XtraInputBox.Show("거래처명을 입력하세요.", "1차 계량 등록", string.Empty);
            var productName = XtraInputBox.Show("제품명을 입력하세요.", "1차 계량 등록", string.Empty);

            var weight = _scaleService.CurrentWeight;
            var record = _repository.AddFirstWeighing(
                vehicleNo.Trim(),
                customerName?.Trim() ?? string.Empty,
                productName?.Trim() ?? string.Empty,
                InOutType.Out,
                weight,
                "콜럼버스 주식회사");

            _logService?.Info("콜럼버스 주식회사", string.Format("{0} : 1차 계량 완료 ({1:N0}kg)", record.VehicleNo, weight));
        }

        /// <summary>MainForm 의 F6 단축키에서 호출.</summary>
        public void CompleteSecondWeighingForSelected()
        {
            if (_repository == null || _scaleService == null)
            {
                return;
            }

            var record = SelectedRecord;
            if (record == null)
            {
                XtraMessageBox.Show("2차 계량을 완료할 건을 목록에서 먼저 선택하세요.", "안내",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var weight = _scaleService.CurrentWeight;
            _repository.CompleteSecondWeighing(record, weight, "콜럼버스 주식회사");

            _logService?.Info("콜럼버스 주식회사", string.Format("{0} : 2차 계량 완료 ({1:N0}kg)", record.VehicleNo, weight));
            SecondWeighingCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void PrintFirstSlip()
        {
            var record = SelectedRecord;
            if (record == null)
            {
                XtraMessageBox.Show("전표를 출력할 건을 목록에서 먼저 선택하세요.", "안내",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // TODO: XtraReports 로 작성된 1차 전표(.repx) 연결.
            XtraMessageBox.Show(
                string.Format("[{0}] {1} 차량 1차 전표 인쇄는 준비 중입니다.", record.WeighSeq, record.VehicleNo),
                "1차 전표", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
