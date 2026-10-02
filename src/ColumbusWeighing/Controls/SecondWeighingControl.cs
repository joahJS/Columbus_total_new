using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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
        /// <summary>지점 선택 콤보에 나열할 항목 1개. Code가 null이면 지점 구분 없이 전체를 본다.</summary>
        private sealed class BranchFilterOption
        {
            public string Code;
            public string Display;

            public BranchFilterOption(string code, string display)
            {
                Code = code;
                Display = display;
            }
        }

        private static readonly BranchFilterOption[] BranchFilterOptions =
        {
            new BranchFilterOption(null, "전체"),
            new BranchFilterOption("A", "영천"),
            new BranchFilterOption("B", "생곡"),
            new BranchFilterOption("C", "녹산"),
        };

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

        /// <summary>업체중량/단가를 편집했지만 아직 "저장" 버튼을 누르지 않은 건들. 조회조건을
        /// 바꿔 목록을 다시 불러오면(ApplyDateFilter) 예전 레코드 인스턴스는 더 이상 화면에 없는
        /// 값이 되므로 같이 비운다 - 그렇지 않으면 나중에 저장을 눌렀을 때 이미 버려진 편집값이
        /// 엉뚱하게 다시 저장될 수 있다.</summary>
        private readonly HashSet<WeighingRecord> _dirtyRecords = new HashSet<WeighingRecord>();

        // "업체중량" 헤더의 도움말 아이콘(동그라미 안에 물음표) 관련 상태.
        // GridView에는 헤더 안에 클릭 가능한 버튼을 넣는 기능이 없어서, CustomDrawColumnHeader로
        // 직접 그리고, 그 영역을 MouseMove/MouseDown으로 직접 히트테스트해서 툴팁/안내 메시지를 띄운다.
        private const string VendorWeightHelpText =
            "업체중량 칸에 0을 입력하면 \"값 없음\"으로 처리되어,\r\n실중량/로스/공급가액이 자동으로 당사중량 기준 계산으로 되돌아갑니다.";
        private Rectangle _vendorWeightHelpIconBounds = Rectangle.Empty;
        private bool _vendorWeightHelpHintVisible;
        private readonly ToolTip _vendorWeightHelpToolTip = new ToolTip();

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
        private GridColumn _colVendorWeight;
        private GridColumn _colLoss;
        private GridColumn _colFinalWeight;
        private GridColumn _colAdminUnitPrice;
        private GridColumn _colSupplyAmount;
        private GridColumn _colUnitPrice;
        private GridColumn _colAmount;
        private GridColumn _colInOutType;
        private GridColumn _colWeigherName;
        // 감량률(%)/비중·환산중량/담당자 컬럼은 연결할 실제 데이터가 없어 뺐다
        // (Models/WeighingColumnSettings.cs 참고).

        public SecondWeighingControl()
        {
            InitializeComponent();
            BuildColumns();
            ComnGridFunc.GridStyleBasicSetting(_gridView);
            SetupDateEditCalendarButton();
            SetupBranchCombo();
            SetupInOutCombo();
            SetupSearchTargetCombo();

            _gridControl.DataSource = _completedRecords;
            _gridView.CustomColumnDisplayText += GridView_CustomColumnDisplayText;
            _gridView.CellValueChanged += GridView_CellValueChanged;
            // DevExpress 버전마다 CustomDrawColumnHeader의 EventArgs 타입/네임스페이스가 달라
            // 이름을 직접 쓰면 깨지기 쉬우므로, 람다로 받아 컴파일러가 타입을 추론하게 하고
            // 실제 그리기는 System.Drawing 타입만 쓰는 DrawVendorWeightHelpIcon에 맡긴다.
            _gridView.CustomDrawColumnHeader += (s, e) =>
            {
                if (e.Column == null || e.Column.FieldName != "VendorWeight")
                {
                    return;
                }

                e.Painter.DrawObject(e.Info);
                e.Handled = true;
                _vendorWeightHelpIconBounds = DrawVendorWeightHelpIcon(e.Graphics, e.Bounds);
            };
            _gridView.MouseMove += GridView_MouseMove;
            _gridView.MouseDown += GridView_MouseDown;
            _dateEditFrom.EditValueChanged += (s, e) => ApplyDateFilter();
            _dateEditTo.EditValueChanged += (s, e) => ApplyDateFilter();
            _branchCombo.SelectedIndexChanged += (s, e) => RefreshCompletedList();
            _inOutCombo.SelectedIndexChanged += (s, e) => RefreshCompletedList();
            _searchTargetCombo.SelectedIndexChanged += (s, e) => RefreshCompletedList();
            // 검색어는 입력하는 즉시가 아니라, 조회 버튼(또는 Enter)을 눌렀을 때만 반영한다.
            _searchTextEdit.KeyDown += (s, e) =>
            {
                if (e.KeyCode == System.Windows.Forms.Keys.Enter)
                {
                    ApplyDateFilter();
                }
            };
            _btnQuery.Click += (s, e) => ApplyDateFilter();
            _btnSave.Click += (s, e) => SaveDirtyRecords();
            _btnSecondSlip.Click += (s, e) => PrintSecondSlip();
            _btnShiftWeekBack.Click += (s, e) => ShiftDateRange(-7);
            _btnShiftDayBack.Click += (s, e) => ShiftDateRange(-1);
            _btnShiftDayForward.Click += (s, e) => ShiftDateRange(1);
            _btnShiftWeekForward.Click += (s, e) => ShiftDateRange(7);
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

            // 조회조건을 바꾸면 목록을 DB에서 다시 불러오므로, 저장하지 않은 업체중량/단가
            // 편집값은 사라진다 - 그 전에 한 번 확인한다.
            if (_dirtyRecords.Count > 0
                && !ComnFunc.gp_PrintQuestion(
                    "저장하지 않은 업체중량/단가 변경사항이 있습니다. 계속하면 사라집니다.\r\n계속하시겠습니까?",
                    "저장 확인", MessageType.질문))
            {
                return;
            }

            _dirtyRecords.Clear();
            _repository?.Refresh(from, to.AddDays(1));
            RefreshCompletedList();
        }

        /// <summary>조회기간(시작일~종료일)을 현재 기간 길이는 그대로 유지한 채 통째로 앞/뒤로
        /// 옮긴다(예: -7이면 시작일/종료일 모두 1주일 전으로). 화살표(&lt;&lt;/&lt;/&gt;/&gt;&gt;) 버튼에서 호출.</summary>
        private void ShiftDateRange(int days)
        {
            var from = FromDate.AddDays(days);
            var to = ToDate.AddDays(days);

            _suppressDateChangeEvents = true;
            _dateEditFrom.DateTime = from;
            _dateEditTo.DateTime = to;
            _suppressDateChangeEvents = false;

            ApplyDateFilter();
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
            var branchCode = SelectedBranchCode;
            var searchByVehicleNo = _searchTargetCombo.SelectedIndex == 0;
            var searchText = _searchTextEdit.Text.Trim();

            _completedRecords.RaiseListChangedEvents = false;
            _completedRecords.Clear();
            foreach (var record in _repository.Records
                .Where(r => r.IsCompleted && r.SecondDateTime.Value >= start && r.SecondDateTime.Value < end
                    && (branchCode == null || r.BranchCode == branchCode)
                    && MatchesInOut(r)
                    && Contains(searchByVehicleNo ? r.VehicleNo : r.CustomerName, searchText)))
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
            AddColumn("NetWeight", "당사중량", 80, "N0");
            _colLossWeight = AddColumn("LossWeight", "감량중량", 80, "N0");
            _colVendorWeight = AddColumn("VendorWeight", "업체중량", 95, "N0");
            _colVendorWeight.OptionsColumn.AllowEdit = true;
            _colLoss = AddColumn("Loss", "로스", 80, "N0");
            _colFinalWeight = AddColumn("FinalWeight", "실중량", 80, "N0");
            _colAdminUnitPrice = AddColumn("AdminUnitPrice", "단가", 80, "N0");
            _colAdminUnitPrice.OptionsColumn.AllowEdit = true;
            _colSupplyAmount = AddColumn("SupplyAmount", "공급가액", 90, "N0");
            _colUnitPrice = AddColumn("UnitPrice", "단가(동기화)", 80, "N0");
            _colAmount = AddColumn("Amount", "금액", 90, "N0");
            _colInOutType = AddInOutColumn();
            // 계량자 컬럼은 요청에 따라 임시로 숨김(코드는 남겨둠 - 나중에 다시 보이게 할 수 있음).
            _colWeigherName = AddColumn("WeigherName", "계량자", 130);
            _colWeigherName.Visible = false;
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

        private void SetupBranchCombo()
        {
            foreach (var option in BranchFilterOptions)
            {
                _branchCombo.Properties.Items.Add(option.Display);
            }

            _branchCombo.SelectedIndex = 0;
        }

        /// <summary>입출고 콤보: 0=전체, 1=입고, 2=출고.</summary>
        private void SetupInOutCombo()
        {
            _inOutCombo.Properties.Items.AddRange(new object[] { "전체", "입고", "출고" });
            _inOutCombo.SelectedIndex = 0;
        }

        /// <summary>검색 대상 콤보: 0=차량번호, 1=거래처.</summary>
        private void SetupSearchTargetCombo()
        {
            _searchTargetCombo.Properties.Items.AddRange(new object[] { "차량번호", "거래처" });
            _searchTargetCombo.SelectedIndex = 0;
        }

        private bool MatchesInOut(WeighingRecord record)
        {
            switch (_inOutCombo.SelectedIndex)
            {
                case 1: return record.InOutType == InOutType.In;
                case 2: return record.InOutType == InOutType.Out;
                default: return true;
            }
        }

        private static bool Contains(string value, string search)
        {
            return string.IsNullOrEmpty(search) || (value ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>조회기간 옆 지점 콤보에서 선택한 지점 코드('A'/'B'/'C'). "전체"를 선택했으면 null.</summary>
        private string SelectedBranchCode
        {
            get
            {
                var index = _branchCombo.SelectedIndex;
                return index >= 0 && index < BranchFilterOptions.Length ? BranchFilterOptions[index].Code : null;
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
                || e.Column.FieldName == "LossWeight" || e.Column.FieldName == "VendorWeight" || e.Column.FieldName == "Loss"
                || e.Column.FieldName == "FinalWeight")
                && e.Value is decimal weight)
            {
                e.DisplayText = FormatWeight(weight);
            }
            else if ((e.Column.FieldName == "UnitPrice" || e.Column.FieldName == "Amount"
                || e.Column.FieldName == "AdminUnitPrice" || e.Column.FieldName == "SupplyAmount") && e.Value is decimal money)
            {
                e.DisplayText = FormatAmount(money);
            }
        }

        /// <summary>"업체중량" 헤더 우측에 동그라미+물음표 도움말 아이콘을 그리고, 그 아이콘의
        /// 영역(그리드 좌표 기준)을 반환한다. System.Drawing 타입만 받아서, DevExpress 커스텀
        /// 드로우 이벤트의 EventArgs 타입(버전마다 이름/네임스페이스가 다르다)과 분리해뒀다.</summary>
        private static Rectangle DrawVendorWeightHelpIcon(Graphics graphics, Rectangle headerBounds)
        {
            const int diameter = 14;
            var iconBounds = new Rectangle(
                headerBounds.Right - diameter - 6,
                headerBounds.Top + (headerBounds.Height - diameter) / 2,
                diameter,
                diameter);

            var oldSmoothingMode = graphics.SmoothingMode;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (var circleBrush = new SolidBrush(Color.White))
            using (var circlePen = new Pen(Color.FromArgb(90, 90, 90)))
            {
                graphics.FillEllipse(circleBrush, iconBounds);
                graphics.DrawEllipse(circlePen, iconBounds);
            }

            using (var font = new Font("맑은 고딕", 7.5f, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.FromArgb(90, 90, 90)))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                graphics.DrawString("?", font, textBrush, iconBounds, format);
            }

            graphics.SmoothingMode = oldSmoothingMode;
            return iconBounds;
        }

        /// <summary>도움말 아이콘 위에 마우스를 올리면 툴팁으로 설명을 보여준다.</summary>
        private void GridView_MouseMove(object sender, MouseEventArgs e)
        {
            if (_vendorWeightHelpIconBounds.Contains(e.Location))
            {
                if (!_vendorWeightHelpHintVisible)
                {
                    _vendorWeightHelpHintVisible = true;
                    _vendorWeightHelpToolTip.Show(VendorWeightHelpText, _gridControl, e.X, e.Y - 20, 8000);
                }
            }
            else if (_vendorWeightHelpHintVisible)
            {
                _vendorWeightHelpHintVisible = false;
                _vendorWeightHelpToolTip.Hide(_gridControl);
            }
        }

        /// <summary>도움말 아이콘을 클릭하면 안내 메시지창으로도 같은 설명을 보여준다.</summary>
        private void GridView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _vendorWeightHelpIconBounds.Contains(e.Location))
            {
                ComnFunc.gp_PrintMessage(VendorWeightHelpText, "업체중량 안내", MessageType.알림);
            }
        }

        /// <summary>업체중량/단가 컬럼을 편집하면 바로 DB에 저장하지 않고, "저장" 버튼을 누를 때
        /// 한꺼번에 반영되도록 변경된 건만 표시해둔다. 로스/실중량/공급가액은 거기서 계산되는
        /// 값이라 저장 대상이 아니다.</summary>
        private void GridView_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName != "VendorWeight" && e.Column.FieldName != "AdminUnitPrice")
            {
                return;
            }

            var record = _gridView.GetRow(e.RowHandle) as WeighingRecord;
            if (record == null)
            {
                return;
            }

            _dirtyRecords.Add(record);
        }

        /// <summary>"저장" 버튼 클릭 시 편집된 업체중량/단가를 한꺼번에 허브 DB에 반영한다.</summary>
        private void SaveDirtyRecords()
        {
            if (_repository == null)
            {
                return;
            }

            if (_dirtyRecords.Count == 0)
            {
                ComnFunc.gp_PrintMessage("변경된 내용이 없습니다.", "안내", MessageType.알림);
                return;
            }

            try
            {
                foreach (var record in _dirtyRecords)
                {
                    _repository.UpdateVendorWeight(record.Id, record.VendorWeight);
                    _repository.UpdateAdminUnitPrice(record.Id, record.AdminUnitPrice);
                }

                _dirtyRecords.Clear();
                ComnFunc.gp_PrintMessage("저장되었습니다.", "안내", MessageType.알림);
            }
            catch (Exception ex)
            {
                ComnFunc.gp_PrintMessage("저장에 실패했습니다.\r\n" + ex.Message, "저장 오류", MessageType.오류);
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
