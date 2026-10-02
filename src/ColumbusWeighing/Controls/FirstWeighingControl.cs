using System.Collections.Generic;
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
    /// "1차 계량 대기" 조회 패널. 아직 2차 계량이 완료되지 않은 건 목록을 보여준다.
    /// 이 프로그램은 조회/집계 전용이며, 계량 입력(1차/2차 계량 버튼)은 각 지점이 지금
    /// 쓰는 프로그램(TS2020/MES)에서 그대로 처리하므로 여기에는 두지 않는다.
    /// </summary>
    public partial class FirstWeighingControl : XtraUserControl
    {
        private IWeighingRepository _repository;

        /// <summary>시스템 설정의 "중량 단위"를 중량 컬럼 표시에 반영하기 위한 값(예: "kg").</summary>
        private string _weightUnit;

        /// <summary>시스템 설정의 "금액 단위"를 단가/금액 컬럼 표시에 반영하기 위한 값(예: "원").</summary>
        private string _amountUnit;

        /// <summary>2차 계량 대기 중인 건만 담는 그리드 전용 목록(그리드는 이 목록에만 바인딩된다).</summary>
        private readonly BindingList<WeighingRecord> _pendingRecords = new BindingList<WeighingRecord>();

        /// <summary>중량 컬럼의 캡션에 시스템 설정의 중량 단위를 붙이기 위해, 컬럼별 기본 캡션을
        /// 따로 기억해둔다(예: "1차중량" + "(kg)"). 값 자체에는 더 이상 단위를 붙이지 않는다.</summary>
        private readonly Dictionary<GridColumn, string> _weightColumnBaseCaptions = new Dictionary<GridColumn, string>();

        // "계량 화면 설정" 팝업에서 켜고 끄는 부가 컬럼들. 차량번호/1차중량/계량자/비고처럼
        // 항상 보여주는 핵심 컬럼은 필드로 따로 들고 있지 않는다.
        private GridColumn _colWeighSeq;
        private GridColumn _colFirstDate;
        private GridColumn _colFirstTime;
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

        public FirstWeighingControl()
        {
            InitializeComponent();
            BuildColumns();
            ComnGridFunc.GridStyleBasicSetting(_gridView);

            _gridControl.DataSource = _pendingRecords;
            _gridView.CustomColumnDisplayText += GridView_CustomColumnDisplayText;
            _btnFirstSlip.Click += (s, e) => PrintFirstSlip();
        }

        /// <summary>선택된 행의 계근 기록(없으면 null).</summary>
        public WeighingRecord SelectedRecord
        {
            get { return _gridView.GetFocusedRow() as WeighingRecord; }
        }

        public void Initialize(IWeighingRepository repository, AppSettings settings)
        {
            _repository = repository;
            _repository.Records.ListChanged += (s, e) => RefreshPendingList();
            ApplyDisplaySettings(settings);
            RefreshPendingList();
        }

        /// <summary>그리드 폰트 크기/중량 단위처럼 화면 표시에만 영향을 주는 설정을 다시 적용한다.
        /// 시스템 설정 창을 닫은 직후에도 호출되므로, 조회 조건(날짜 등)은 건드리지 않는다.</summary>
        public void ApplyDisplaySettings(AppSettings settings)
        {
            _weightUnit = settings.WeightUnit;
            _amountUnit = settings.AmountUnit;
            ApplyWeightUnitToCaptions();
            ComnGridFunc.SetRowFontSize(_gridView, settings.MainGridFontSize);
            _gridControl.Refresh();
        }

        /// <summary>중량 컬럼 캡션을 "기본캡션(단위)" 형태로 갱신한다(예: "1차중량(kg)").
        /// 값 칸에는 더 이상 단위를 붙이지 않고 헤더에만 표기한다.</summary>
        private void ApplyWeightUnitToCaptions()
        {
            var suffix = string.IsNullOrEmpty(_weightUnit) ? string.Empty : "(" + _weightUnit + ")";
            foreach (var pair in _weightColumnBaseCaptions)
            {
                pair.Key.Caption = pair.Value + suffix;
            }
        }

        /// <summary>"계량 화면 설정" 팝업에서 고른 부가 컬럼 표시 여부를 그리드에 반영한다.</summary>
        public void ApplyColumnSettings(WeighingColumnSettings settings)
        {
            _colWeighSeq.Visible = settings.ShowWeighSeq;
            _colFirstDate.Visible = settings.ShowFirstDateTime;
            _colFirstTime.Visible = settings.ShowFirstDateTime;
            _colOwnerCompany.Visible = settings.ShowOwnerCompany;
            _colDriverName.Visible = settings.ShowDriverName;
            _colProductName.Visible = settings.ShowProductName;
            _colCustomerName.Visible = settings.ShowCustomerName;
            _colLossWeight.Visible = settings.ShowLossWeight;
            _colUnitPrice.Visible = settings.ShowPriceInfo;
            _colAmount.Visible = settings.ShowPriceInfo;
            _colInOutType.Visible = settings.ShowInOutType;
        }

        /// <summary>저장소 전체 목록에서 2차 계량 대기 중인 건만 다시 뽑아 그리드용 목록을 갱신한다.</summary>
        private void RefreshPendingList()
        {
            _pendingRecords.RaiseListChangedEvents = false;
            _pendingRecords.Clear();
            foreach (var record in _repository.Records.Where(r => !r.IsCompleted))
            {
                _pendingRecords.Add(record);
            }

            _pendingRecords.RaiseListChangedEvents = true;
            _pendingRecords.ResetBindings();
        }

        private void BuildColumns()
        {
            _gridView.Columns.Clear();
            _weightColumnBaseCaptions.Clear();

            _colFirstDate = AddCenteredColumn("FirstDateTime", "1차 계량일", 90, "yyyy-MM-dd");
            AddCenteredColumn("BranchCode", "지점", 60);
            _colWeighSeq = AddCenteredColumn("WeighSeq", "계량순번", 60);
            _colFirstTime = AddCenteredColumn("FirstDateTime", "1차시간", 60, "HH:mm");
            AddCenteredColumn("VehicleNo", "차량번호", 70);
            _colOwnerCompany = AddColumn("OwnerCompany", "차량소속회사", 100);
            _colDriverName = AddColumn("DriverName", "운전자", 80);
            _colCustomerName = AddColumn("CustomerName", "거래처명", 110);
            _colProductName = AddCenteredColumn("ProductName", "제품명", 100);
            AddWeightColumn("FirstWeight", "1차중량", 80);
            _colAmount = AddMoneyColumn("Amount", "금액", 90);
            _colUnitPrice = AddMoneyColumn("UnitPrice", "단가", 80);
            _colLossWeight = AddWeightColumn("LossWeight", "감량중량", 80);
            _colInOutType = AddCenteredColumn("InOutType", "입/출고", 60);
            AddColumn("WeigherName", "계량자", 130);
            AddLeftAlignedColumn("Remark", "비고", 100);
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

        /// <summary>가운데 정렬 컬럼(일자/지점/순번/차량번호/제품명/시간/입출고 등).</summary>
        private GridColumn AddCenteredColumn(string fieldName, string caption, int width, string format = null)
        {
            var column = AddColumn(fieldName, caption, width, format);
            column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            column.AppearanceCell.Options.UseTextOptions = true;
            return column;
        }

        /// <summary>우측 정렬 + "N0" 숫자 포맷 중량 컬럼. 캡션에 중량 단위를 붙이기 위해
        /// 기본 캡션을 _weightColumnBaseCaptions에 등록해둔다(ApplyWeightUnitToCaptions 참고).</summary>
        private GridColumn AddWeightColumn(string fieldName, string caption, int width)
        {
            var column = AddColumn(fieldName, caption, width, "N0");
            column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            column.AppearanceCell.Options.UseTextOptions = true;
            _weightColumnBaseCaptions[column] = caption;
            return column;
        }

        /// <summary>우측 정렬 + "N0" 숫자 포맷 금액 컬럼(단가/금액).</summary>
        private GridColumn AddMoneyColumn(string fieldName, string caption, int width)
        {
            var column = AddColumn(fieldName, caption, width, "N0");
            column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            column.AppearanceCell.Options.UseTextOptions = true;
            return column;
        }

        /// <summary>좌측 정렬 컬럼(비고). 문자열 컬럼의 기본 정렬과 같지만 명시적으로 지정해둔다.</summary>
        private GridColumn AddLeftAlignedColumn(string fieldName, string caption, int width)
        {
            var column = AddColumn(fieldName, caption, width);
            column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            column.AppearanceCell.Options.UseTextOptions = true;
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
            // 중량 값 자체에는 더 이상 단위를 붙이지 않는다(헤더 캡션에 "(kg)"처럼 표기 -
            // ApplyWeightUnitToCaptions 참고). DisplayFormat의 "N0"만으로 숫자 서식은 충분하다.
            else if ((e.Column.FieldName == "UnitPrice" || e.Column.FieldName == "Amount") && e.Value is decimal money)
            {
                e.DisplayText = FormatAmount(money);
            }
        }

        /// <summary>금액 값에 시스템 설정의 금액 단위(예: "원")를 붙여서 보여준다.</summary>
        private string FormatAmount(decimal value)
        {
            var text = value.ToString("N0");
            return string.IsNullOrEmpty(_amountUnit) ? text : text + " " + _amountUnit;
        }

        private void PrintFirstSlip()
        {
            var record = SelectedRecord;
            if (record == null)
            {
                ComnFunc.gp_PrintMessage("전표를 출력할 건을 목록에서 먼저 선택하세요.", "안내", MessageType.알림);
                return;
            }

            // TODO: XtraReports 로 작성된 1차 전표(.repx) 연결.
            ComnFunc.gp_PrintMessage(
                string.Format("[{0}] {1} 차량 1차 전표 인쇄는 준비 중입니다.", record.WeighSeq, record.VehicleNo),
                "1차 전표", MessageType.알림);
        }
    }
}
