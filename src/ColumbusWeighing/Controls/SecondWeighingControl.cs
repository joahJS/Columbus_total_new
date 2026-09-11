using System;
using System.ComponentModel;
using System.Linq;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;

namespace ColumbusWeighing.Controls
{
    /// <summary>
    /// "2차계량 완료" 조회 패널. 조회일자 기준으로 2차 계량까지 완료된(순중량 확정) 계근 건
    /// 목록을 보여준다. 계량 입력(1회계량 버튼)은 각 지점이 지금 쓰는 프로그램에서 그대로
    /// 처리하므로 여기에는 두지 않는다.
    /// </summary>
    public partial class SecondWeighingControl : XtraUserControl
    {
        private IWeighingRepository _repository;

        /// <summary>시스템 설정의 "중량 단위"를 중량 컬럼 표시에 반영하기 위한 값(예: "kg").</summary>
        private string _weightUnit;

        /// <summary>시스템 설정의 "금액 단위"를 단가/금액 컬럼 표시에 반영하기 위한 값(예: "원").</summary>
        private string _amountUnit;

        /// <summary>true인 동안은 날짜 편집기 값이 바뀌어도 ApplyDateFilter를 실행하지 않는다
        /// (시작일/종료일 두 값을 한꺼번에 세팅할 때 중간 상태로 DB를 두 번 조회하는 것을 막는다).</summary>
        private bool _suppressDateChangeEvents;

        /// <summary>조회일자 기준으로 완료된 건만 담는 그리드 전용 목록(그리드는 이 목록에만 바인딩된다).</summary>
        private readonly BindingList<WeighingRecord> _completedRecords = new BindingList<WeighingRecord>();

        // "계량 화면 설정" 팝업에서 켜고 끄는 부가 컬럼들. 차량번호/1·2차중량/순중량/계량자/비고처럼
        // 항상 보여주는 핵심 컬럼은 필드로 따로 들고 있지 않는다.
        private GridColumn _colWeighSeq;
        private GridColumn _colFirstTime;
        private GridColumn _colSecondDate;
        private GridColumn _colSecondTime;
        private GridColumn _colOwnerCompany;
        private GridColumn _colDriverName;
        private GridColumn _colProductName;
        private GridColumn _colCustomerName;
        private GridColumn _colLossWeight;
        private GridColumn _colUnitPrice;
        private GridColumn _colAmount;
        private GridColumn _colInOutType;
        // 감량률(%)/비중·환산중량/담당자 컬럼은 연결할 실제 데이터가 없어 뺐다
        // (Models/WeighingColumnSettings.cs 참고).

        public SecondWeighingControl()
        {
            InitializeComponent();
            BuildColumns();
            ComnGridFunc.GridStyleBasicSetting(_gridView);
            SetupDateEditCalendarButton();

            _gridControl.DataSource = _completedRecords;
            _gridView.CustomColumnDisplayText += GridView_CustomColumnDisplayText;
            _dateEditFrom.EditValueChanged += (s, e) => ApplyDateFilter();
            _dateEditTo.EditValueChanged += (s, e) => ApplyDateFilter();
            _btnSecondSlip.Click += (s, e) => PrintSecondSlip();
        }

        public WeighingRecord SelectedRecord
        {
            get { return _gridView.GetFocusedRow() as WeighingRecord; }
        }

        /// <summary>조회 기간 시작일(포함).</summary>
        public DateTime FromDate
        {
            get { return _dateEditFrom.DateTime == DateTime.MinValue ? DateTime.Today : _dateEditFrom.DateTime.Date; }
            set { _dateEditFrom.DateTime = value.Date; }
        }

        /// <summary>조회 기간 종료일(포함) - 이 날짜까지의 2차 계량 완료 건을 보여준다.</summary>
        public DateTime ToDate
        {
            get { return _dateEditTo.DateTime == DateTime.MinValue ? DateTime.Today : _dateEditTo.DateTime.Date; }
            set { _dateEditTo.DateTime = value.Date; }
        }

        public void Initialize(IWeighingRepository repository, AppSettings settings)
        {
            _repository = repository;
            _repository.Records.ListChanged += (s, e) => RefreshCompletedList();

            // 시스템 설정의 "마감 기준 시간"을 반영한 영업일 기준 오늘 날짜를 초기 조회기간으로 쓴다.
            var businessToday = ComnFunc.GetBusinessToday(settings.ClosingTime);

            // 두 값을 세팅하는 동안은 ApplyDateFilter를 억제해, 중간 상태(시작일만 오늘로
            // 바뀐 상태 등)로 DB를 불필요하게 두 번 조회하지 않게 한다.
            _suppressDateChangeEvents = true;
            _dateEditFrom.DateTime = businessToday;
            _dateEditTo.DateTime = businessToday;
            _suppressDateChangeEvents = false;

            ApplyDisplaySettings(settings);
            ApplyDateFilter();
        }

        /// <summary>그리드 폰트 크기/중량 단위처럼 화면 표시에만 영향을 주는 설정을 다시 적용한다.
        /// 시스템 설정 창을 닫은 직후에도 호출되므로, 현재 선택된 조회기간은 건드리지 않는다.</summary>
        public void ApplyDisplaySettings(AppSettings settings)
        {
            _weightUnit = settings.WeightUnit;
            _amountUnit = settings.AmountUnit;
            ComnGridFunc.SetRowFontSize(_gridView, settings.MainGridFontSize);
            _gridControl.Refresh();
        }

        /// <summary>"계량 화면 설정" 팝업에서 고른 부가 컬럼 표시 여부를 그리드에 반영한다.</summary>
        public void ApplyColumnSettings(WeighingColumnSettings settings)
        {
            _colWeighSeq.Visible = settings.ShowWeighSeq;
            _colFirstTime.Visible = settings.ShowFirstDateTime;
            _colSecondDate.Visible = settings.ShowSecondDateTime;
            _colSecondTime.Visible = settings.ShowSecondDateTime;
            _colOwnerCompany.Visible = settings.ShowOwnerCompany;
            _colDriverName.Visible = settings.ShowDriverName;
            _colProductName.Visible = settings.ShowProductName;
            _colCustomerName.Visible = settings.ShowCustomerName;
            _colLossWeight.Visible = settings.ShowLossWeight;
            _colUnitPrice.Visible = settings.ShowPriceInfo;
            _colAmount.Visible = settings.ShowPriceInfo;
            _colInOutType.Visible = settings.ShowInOutType;
        }

        public void ApplyDateFilter()
        {
            if (_suppressDateChangeEvents)
            {
                return;
            }

            var from = FromDate;
            var to = ToDate;
            if (to < from)
            {
                // 종료일이 시작일보다 빠르면 종료일을 시작일에 맞춰 되돌린다(범위가 뒤집히지 않도록).
                ToDate = from;
                return; // ToDate 세팅이 다시 ApplyDateFilter를 호출하므로 여기서는 끝낸다.
            }

            _repository?.Refresh(from, to.AddDays(1));
            RefreshCompletedList();
        }

        /// <summary>목록을 최신 데이터로 다시 표시(다른 화면에서 데이터가 갱신된 뒤 호출).</summary>
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

            var start = FromDate;
            var end = ToDate.AddDays(1);

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

            _colSecondDate = AddColumn("SecondDateTime", "2차계량일", 90, "yyyy-MM-dd");
            AddColumn("BranchCode", "지점", 60);
            _colWeighSeq = AddColumn("WeighSeq", "계량순번", 60);
            _colFirstTime = AddColumn("FirstDateTime", "1차시간", 55, "HH:mm");
            _colSecondTime = AddColumn("SecondDateTime", "2차시간", 55, "HH:mm");
            AddColumn("VehicleNo", "차량번호", 70);
            _colOwnerCompany = AddColumn("OwnerCompany", "차량소속회사", 100);
            _colDriverName = AddColumn("DriverName", "운전자", 80);
            _colCustomerName = AddColumn("CustomerName", "거래처명", 110);
            _colProductName = AddColumn("ProductName", "제품명", 100);
            AddColumn("FirstWeight", "1차중량", 80, "N0");
            AddColumn("SecondWeight", "2차중량", 80, "N0");
            AddColumn("NetWeight", "순중량", 80, "N0");
            _colLossWeight = AddColumn("LossWeight", "감량중량", 80, "N0");
            _colUnitPrice = AddColumn("UnitPrice", "단가", 80, "N0");
            _colAmount = AddColumn("Amount", "금액", 90, "N0");
            _colInOutType = AddInOutColumn();
            AddColumn("WeigherName", "계량자", 130);
            AddColumn("Remark", "비고", 100);
        }

        /// <summary>조회일자 우측에 클릭하면 달력이 열리는 버튼을 명시적으로 붙인다.</summary>
        private void SetupDateEditCalendarButton()
        {
            foreach (var dateEdit in new[] { _dateEditFrom, _dateEditTo })
            {
                dateEdit.Properties.Buttons.Clear();
                dateEdit.Properties.Buttons.Add(new DevExpress.XtraEditors.Controls.EditorButton(
                    DevExpress.XtraEditors.Controls.ButtonPredefines.Combo));
            }
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

        private GridColumn AddInOutColumn()
        {
            var column = _gridView.Columns.AddVisible("InOutType", "입/출고");
            column.Width = 60;
            column.OptionsColumn.AllowEdit = false;
            return column;
        }

        private void GridView_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "InOutType" && e.Value is InOutType inOut)
            {
                e.DisplayText = inOut.ToDisplayString();
            }
            else if (e.Column.FieldName == "BranchCode" && e.Value is string branchCode)
            {
                e.DisplayText = branchCode.ToDisplayString();
            }
            else if ((e.Column.FieldName == "FirstWeight" || e.Column.FieldName == "SecondWeight" || e.Column.FieldName == "NetWeight"
                || e.Column.FieldName == "LossWeight") && e.Value is decimal weight)
            {
                e.DisplayText = FormatWeight(weight);
            }
            else if ((e.Column.FieldName == "UnitPrice" || e.Column.FieldName == "Amount") && e.Value is decimal money)
            {
                e.DisplayText = FormatAmount(money);
            }
        }

        /// <summary>중량 값에 시스템 설정의 중량 단위(예: "kg")를 붙여서 보여준다.</summary>
        private string FormatWeight(decimal value)
        {
            var text = value.ToString("N0");
            return string.IsNullOrEmpty(_weightUnit) ? text : text + " " + _weightUnit;
        }

        /// <summary>금액 값에 시스템 설정의 금액 단위(예: "원")를 붙여서 보여준다.</summary>
        private string FormatAmount(decimal value)
        {
            var text = value.ToString("N0");
            return string.IsNullOrEmpty(_amountUnit) ? text : text + " " + _amountUnit;
        }

        private void PrintSecondSlip()
        {
            var record = SelectedRecord;
            if (record == null)
            {
                ComnFunc.gp_PrintMessage("전표를 출력할 건을 목록에서 먼저 선택하세요.", "안내", MessageType.알림);
                return;
            }

            // TODO: XtraReports 로 작성된 2차 전표(.repx) 연결.
            ComnFunc.gp_PrintMessage(
                string.Format("[{0}] {1} 차량 2차 전표 인쇄는 준비 중입니다.", record.WeighSeq, record.VehicleNo),
                "2차 전표", MessageType.알림);
        }
    }
}
