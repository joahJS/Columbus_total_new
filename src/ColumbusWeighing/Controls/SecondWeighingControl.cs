using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ColumbusWeighing.Models;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;

namespace ColumbusWeighing.Controls
{
    /// <summary>
    /// "2차계량" 패널. 조회일자 기준으로 2차 계량까지 완료된(순중량 확정) 계근 건 목록을 보여준다.
    /// 원본 화면의 팝업(계량 화면 설정)에 가려져 있던 하단 그리드 영역에 해당한다.
    /// </summary>
    public partial class SecondWeighingControl : XtraUserControl
    {
        private IWeighingRepository _repository;
        private IScaleIndicatorService _scaleService;
        private AppLogService _logService;

        /// <summary>조회일자 기준으로 완료된 건만 담는 그리드 전용 목록(그리드는 이 목록에만 바인딩된다).</summary>
        private readonly BindingList<WeighingRecord> _completedRecords = new BindingList<WeighingRecord>();

        public SecondWeighingControl()
        {
            InitializeComponent();
            BuildColumns();
            ApplyGridAppearance();
            SetupDateEditCalendarButton();

            _gridControl.DataSource = _completedRecords;
            _gridView.CustomColumnDisplayText += GridView_CustomColumnDisplayText;
            _dateEdit.EditValueChanged += (s, e) => ApplyDateFilter();
            _btnSingleWeighing.Click += (s, e) => RegisterSingleWeighing();
            _btnSecondSlip.Click += (s, e) => PrintSecondSlip();
        }

        public WeighingRecord SelectedRecord
        {
            get { return _gridView.GetFocusedRow() as WeighingRecord; }
        }

        public DateTime QueryDate
        {
            get { return _dateEdit.DateTime == DateTime.MinValue ? DateTime.Today : _dateEdit.DateTime.Date; }
            set { _dateEdit.DateTime = value.Date; }
        }

        public void Initialize(IWeighingRepository repository, IScaleIndicatorService scaleService, AppLogService logService)
        {
            _repository = repository;
            _scaleService = scaleService;
            _logService = logService;

            _repository.Records.ListChanged += (s, e) => RefreshCompletedList();
            QueryDate = DateTime.Today;
            RefreshCompletedList();
        }

        public void ApplyDateFilter()
        {
            RefreshCompletedList();
        }

        /// <summary>목록을 최신 데이터로 다시 표시(다른 패널에서 2차 계량 완료 처리 시 호출).</summary>
        public void RefreshView()
        {
            RefreshCompletedList();
        }

        /// <summary>저장소 전체 목록에서 조회일자에 2차 계량이 완료된 건만 다시 뽑아 그리드용 목록을 갱신한다.</summary>
        private void RefreshCompletedList()
        {
            if (_repository == null)
            {
                return;
            }

            var start = QueryDate;
            var end = start.AddDays(1);

            _completedRecords.RaiseListChangedEvents = false;
            _completedRecords.Clear();
            foreach (var record in _repository.Records
                .Where(r => r.IsCompleted && r.SecondDateTime.Value >= start && r.SecondDateTime.Value < end))
            {
                _completedRecords.Add(record);
            }

            _completedRecords.RaiseListChangedEvents = true;
            _completedRecords.ResetBindings();
        }

        private void BuildColumns()
        {
            _gridView.Columns.Clear();

            AddColumn("SecondDateTime", "2차계량일", 90, "yyyy-MM-dd");
            AddColumn("WeighSeq", "계량순번", 60);
            AddColumn("FirstDateTime", "1차시간", 55, "HH:mm");
            AddColumn("SecondDateTime", "2차시간", 55, "HH:mm");
            AddColumn("VehicleNo", "차량번호", 70);
            AddColumn("CustomerName", "거래처명", 110);
            AddColumn("ProductName", "제품명", 100);
            AddColumn("FirstWeight", "1차중량", 80, "N0");
            AddColumn("SecondWeight", "2차중량", 80, "N0");
            AddColumn("NetWeight", "순중량", 80, "N0");
            AddInOutColumn();
            AddColumn("WeigherName", "계량자", 130);
            AddColumn("Remark", "비고", 100);
        }

        /// <summary>참고 화면과 동일한 배색(크림색 짝수행/흰색 홀수행, 하늘색 헤더, 파란색 선택행)으로 맞춘다.</summary>
        private void ApplyGridAppearance()
        {
            _gridView.RowHeight = 22;

            _gridView.Appearance.HeaderPanel.BackColor = Color.FromArgb(198, 217, 241);
            _gridView.Appearance.HeaderPanel.ForeColor = Color.Black;
            _gridView.Appearance.HeaderPanel.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            _gridView.Appearance.HeaderPanel.Options.UseBackColor = true;
            _gridView.Appearance.HeaderPanel.Options.UseForeColor = true;
            _gridView.Appearance.HeaderPanel.Options.UseFont = true;
            _gridView.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            _gridView.OptionsView.EnableAppearanceEvenRow = true;
            _gridView.Appearance.EvenRow.BackColor = Color.FromArgb(255, 251, 224);
            _gridView.Appearance.EvenRow.Options.UseBackColor = true;
            _gridView.Appearance.OddRow.BackColor = Color.White;
            _gridView.Appearance.OddRow.Options.UseBackColor = true;
            _gridView.Appearance.Row.Font = new Font("맑은 고딕", 9F);
            _gridView.Appearance.Row.Options.UseFont = true;

            _gridView.Appearance.FocusedRow.BackColor = Color.FromArgb(51, 153, 255);
            _gridView.Appearance.FocusedRow.ForeColor = Color.White;
            _gridView.Appearance.FocusedRow.Options.UseBackColor = true;
            _gridView.Appearance.FocusedRow.Options.UseForeColor = true;
            _gridView.Appearance.SelectedRow.BackColor = Color.FromArgb(51, 153, 255);
            _gridView.Appearance.SelectedRow.ForeColor = Color.White;
            _gridView.Appearance.SelectedRow.Options.UseBackColor = true;
            _gridView.Appearance.SelectedRow.Options.UseForeColor = true;
        }

        /// <summary>조회일자 우측에 클릭하면 달력이 열리는 버튼을 명시적으로 붙인다.</summary>
        private void SetupDateEditCalendarButton()
        {
            _dateEdit.Properties.Buttons.Clear();
            _dateEdit.Properties.Buttons.Add(new DevExpress.XtraEditors.Controls.EditorButton(
                DevExpress.XtraEditors.Controls.ButtonPredefines.Combo));
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

        /// <summary>MainForm 의 F7 단축키에서 호출.</summary>
        public void RegisterSingleWeighing()
        {
            if (_repository == null || _scaleService == null)
            {
                return;
            }

            var vehicleNo = XtraInputBox.Show("차량번호를 입력하세요.", "1회 계량 등록", string.Empty);
            if (string.IsNullOrWhiteSpace(vehicleNo))
            {
                return;
            }

            var customerName = XtraInputBox.Show("거래처명을 입력하세요.", "1회 계량 등록", string.Empty);
            var productName = XtraInputBox.Show("제품명을 입력하세요.", "1회 계량 등록", string.Empty);
            var tareText = XtraInputBox.Show("등록된 공차중량(kg)을 입력하세요.", "1회 계량 등록", "0");

            if (!decimal.TryParse(tareText, out var tareWeight))
            {
                tareWeight = 0m;
            }

            var currentWeight = _scaleService.CurrentWeight;
            var record = _repository.AddSingleWeighing(
                vehicleNo.Trim(),
                customerName?.Trim() ?? string.Empty,
                productName?.Trim() ?? string.Empty,
                InOutType.Out,
                currentWeight,
                tareWeight,
                "콜럼버스 주식회사");

            _logService?.Info("콜럼버스 주식회사", string.Format("{0} : 1회 계량 완료 (순중량 {1:N0}kg)", record.VehicleNo, record.NetWeight));

            RefreshView();
        }

        private void PrintSecondSlip()
        {
            var record = SelectedRecord;
            if (record == null)
            {
                XtraMessageBox.Show("전표를 출력할 건을 목록에서 먼저 선택하세요.", "안내",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // TODO: XtraReports 로 작성된 2차 전표(.repx) 연결.
            XtraMessageBox.Show(
                string.Format("[{0}] {1} 차량 2차 전표 인쇄는 준비 중입니다.", record.WeighSeq, record.VehicleNo),
                "2차 전표", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
