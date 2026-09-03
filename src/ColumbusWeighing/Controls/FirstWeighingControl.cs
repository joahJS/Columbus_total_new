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

        /// <summary>2차 계량 대기 중인 건만 담는 그리드 전용 목록(그리드는 이 목록에만 바인딩된다).</summary>
        private readonly BindingList<WeighingRecord> _pendingRecords = new BindingList<WeighingRecord>();

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

        public void Initialize(IWeighingRepository repository)
        {
            _repository = repository;
            _repository.Records.ListChanged += (s, e) => RefreshPendingList();
            RefreshPendingList();
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

            AddColumn("FirstDateTime", "1차 계량일", 90, "yyyy-MM-dd");
            AddColumn("BranchCode", "지점", 60);
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
            else if (e.Column.FieldName == "BranchCode" && e.Value is string branchCode)
            {
                e.DisplayText = branchCode.ToDisplayString();
            }
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
