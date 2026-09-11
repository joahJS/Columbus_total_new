using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;

namespace ColumbusWeighing.Forms
{
    /// <summary>
    /// 참고 화면(TS2020 "계량 데이터 관리")과 같은 취지의, 여러 조건으로 계근 기록을 검색하고
    /// 합계를 보여주는 화면. 이 프로그램은 조회/집계 전용이므로 추가/수정/삭제는 두지 않았다
    /// (계량 입력은 각 지점이 지금 쓰는 프로그램에서 그대로 처리 - 여기서 새로 입력하게 하면
    /// 실제 계근 데이터와 이중 입력/불일치 위험이 생긴다). 참고 화면의 일자별 시(時) 단위
    /// 조회, 전표번호 범위 조회는 단순화해서 날짜 단위 조회만 지원한다.
    /// </summary>
    public partial class WeighDataManagementForm : XtraForm
    {
        private const int ComboAllIndex = 0;

        private readonly IWeighingRepository _repository;
        private string _weightUnit;
        private string _amountUnit;

        private DateEdit _dateEditFrom;
        private DateEdit _dateEditTo;
        private SimpleButton _btnToday;
        private ComboBoxEdit _cboStatus;
        private ComboBoxEdit _cboInOut;
        private TextEdit _productEdit;
        private TextEdit _ownerCompanyEdit;
        private TextEdit _customerEdit;
        private TextEdit _vehicleNoEdit;

        private LabelControl _countValueLabel;
        private LabelControl _netWeightValueLabel;
        private LabelControl _lossValueLabel;
        private LabelControl _receivedValueLabel;
        private LabelControl _amountValueLabel;

        private readonly System.ComponentModel.BindingList<WeighingRecord> _displayRecords =
            new System.ComponentModel.BindingList<WeighingRecord>();

        public WeighDataManagementForm(IWeighingRepository repository, AppSettings settings)
        {
            InitializeComponent();

            _repository = repository;
            _weightUnit = settings.WeightUnit;
            _amountUnit = settings.AmountUnit;

            BuildFilterControls();
            BuildSummaryLabels();
            BuildColumns();
            ComnGridFunc.GridStyleBasicSetting(_gridView);
            _gridView.OptionsView.ShowGroupPanel = false;
            _gridView.OptionsBehavior.Editable = false;
            _gridView.CustomColumnDisplayText += GridView_CustomColumnDisplayText;

            _gridControl.DataSource = _displayRecords;

            _dateEditFrom.DateTime = DateTime.Today;
            _dateEditTo.DateTime = DateTime.Today;

            _btnToday.Click += (s, e) => SetToday();
            _btnExcel.Click += (s, e) => ExportToCsv();
            _btnPrint.Click += (s, e) => PrintList();
            _btnRetrieve.Click += (s, e) => Retrieve();
            _btnClose.Click += (s, e) => Close();

            KeyPreview = true;

            Retrieve();
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

        private void SetToday()
        {
            _dateEditFrom.DateTime = DateTime.Today;
            _dateEditTo.DateTime = DateTime.Today;
            Retrieve();
        }

        private void Retrieve()
        {
            var from = _dateEditFrom.DateTime == DateTime.MinValue ? DateTime.Today : _dateEditFrom.DateTime.Date;
            var to = _dateEditTo.DateTime == DateTime.MinValue ? DateTime.Today : _dateEditTo.DateTime.Date;
            if (to < from)
            {
                to = from;
                _dateEditTo.DateTime = from;
            }

            _repository.Refresh(from, to.AddDays(1));
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var product = _productEdit.Text.Trim();
            var ownerCompany = _ownerCompanyEdit.Text.Trim();
            var customer = _customerEdit.Text.Trim();
            var vehicleNo = _vehicleNoEdit.Text.Trim();
            var statusIndex = _cboStatus.SelectedIndex;
            var inOutIndex = _cboInOut.SelectedIndex;

            var filtered = _repository.Records.Where(r =>
                Contains(r.ProductName, product) &&
                Contains(r.OwnerCompany, ownerCompany) &&
                Contains(r.CustomerName, customer) &&
                Contains(r.VehicleNo, vehicleNo) &&
                MatchesStatus(r, statusIndex) &&
                MatchesInOut(r, inOutIndex));

            _displayRecords.RaiseListChangedEvents = false;
            _displayRecords.Clear();
            foreach (var record in filtered)
            {
                _displayRecords.Add(record);
            }

            _displayRecords.RaiseListChangedEvents = true;
            _displayRecords.ResetBindings();

            UpdateSummary();
        }

        private static bool Contains(string value, string search)
        {
            return string.IsNullOrEmpty(search) || (value ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>계량상태 콤보: 0=전체, 1=1차 대기, 2=2차 완료.</summary>
        private static bool MatchesStatus(WeighingRecord record, int selectedIndex)
        {
            if (selectedIndex == ComboAllIndex)
            {
                return true;
            }

            return selectedIndex == 1 ? !record.IsCompleted : record.IsCompleted;
        }

        /// <summary>입출구분 콤보: 0=전체, 1=입고, 2=출고.</summary>
        private static bool MatchesInOut(WeighingRecord record, int selectedIndex)
        {
            if (selectedIndex == ComboAllIndex)
            {
                return true;
            }

            return selectedIndex == 1 ? record.InOutType == InOutType.In : record.InOutType == InOutType.Out;
        }

        private void UpdateSummary()
        {
            var netWeightSum = _displayRecords.Sum(r => r.NetWeight ?? 0m);
            var lossSum = _displayRecords.Sum(r => r.LossWeight ?? 0m);
            var receivedSum = netWeightSum - lossSum;
            var amountSum = _displayRecords.Sum(r => r.Amount ?? 0m);

            _countValueLabel.Text = _displayRecords.Count.ToString("N0") + "건";
            _netWeightValueLabel.Text = FormatWeight(netWeightSum);
            _lossValueLabel.Text = FormatWeight(lossSum);
            _receivedValueLabel.Text = FormatWeight(receivedSum);
            _amountValueLabel.Text = FormatAmount(amountSum);
        }

        /// <summary>필터/검색 조건 컨트롤을 코드로 만든다. 개수가 많아 계량 화면 설정 팝업의
        /// 체크박스 행처럼 Designer가 아니라 여기서 직접 배치한다.</summary>
        private void BuildFilterControls()
        {
            var labelFont = new Font("맑은 고딕", 9.5F, FontStyle.Bold);

            AddLabel(10, 10, 54, "조회기간", labelFont);
            _dateEditFrom = AddDateEdit(68, 7, 110);
            AddLabel(182, 10, 14, "~", labelFont);
            _dateEditTo = AddDateEdit(200, 7, 110);
            _btnToday = new SimpleButton { Location = new Point(316, 6), Size = new Size(50, 22), Text = "오늘" };
            _filterPanel.Controls.Add(_btnToday);

            AddLabel(380, 10, 54, "계량상태", labelFont);
            _cboStatus = AddComboEdit(438, 7, 100, new[] { "전체", "1차 대기", "2차 완료" });

            AddLabel(548, 10, 54, "입출구분", labelFont);
            _cboInOut = AddComboEdit(606, 7, 90, new[] { "전체", "입고", "출고" });

            AddLabel(10, 43, 30, "제품", labelFont);
            _productEdit = AddTextEdit(44, 40, 130);

            AddLabel(188, 43, 70, "차량소속회사", labelFont);
            _ownerCompanyEdit = AddTextEdit(262, 40, 130);

            AddLabel(406, 43, 40, "거래처", labelFont);
            _customerEdit = AddTextEdit(450, 40, 130);

            AddLabel(594, 43, 50, "차량번호", labelFont);
            _vehicleNoEdit = AddTextEdit(648, 40, 110);
        }

        private void AddLabel(int x, int y, int width, string caption, Font font)
        {
            var label = new LabelControl
            {
                Location = new Point(x, y),
                Size = new Size(width, 16),
                Text = caption
            };
            label.Appearance.Font = font;
            label.Appearance.Options.UseFont = true;
            _filterPanel.Controls.Add(label);
        }

        private DateEdit AddDateEdit(int x, int y, int width)
        {
            var dateEdit = new DateEdit { Location = new Point(x, y), Size = new Size(width, 20) };
            dateEdit.Properties.Mask.EditMask = "yyyy-MM-dd";
            dateEdit.Properties.Buttons.Clear();
            dateEdit.Properties.Buttons.Add(new DevExpress.XtraEditors.Controls.EditorButton(
                DevExpress.XtraEditors.Controls.ButtonPredefines.Combo));
            _filterPanel.Controls.Add(dateEdit);
            return dateEdit;
        }

        private TextEdit AddTextEdit(int x, int y, int width)
        {
            var textEdit = new TextEdit { Location = new Point(x, y), Size = new Size(width, 20) };
            _filterPanel.Controls.Add(textEdit);
            return textEdit;
        }

        private ComboBoxEdit AddComboEdit(int x, int y, int width, string[] items)
        {
            var combo = new ComboBoxEdit { Location = new Point(x, y), Size = new Size(width, 20) };
            combo.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            combo.Properties.Items.AddRange(items);
            combo.SelectedIndex = ComboAllIndex;
            _filterPanel.Controls.Add(combo);
            return combo;
        }

        /// <summary>하단 집계(계량횟수/실중량/감량/인수량/금액) 라벨을 코드로 만든다.</summary>
        private void BuildSummaryLabels()
        {
            var captionFont = new Font("맑은 고딕", 9.5F, FontStyle.Bold);

            AddSummaryCaption(10, "계량횟수", captionFont);
            _countValueLabel = AddSummaryValue(68, 90);

            AddSummaryCaption(168, "실중량", captionFont);
            _netWeightValueLabel = AddSummaryValue(212, 110);

            AddSummaryCaption(332, "감량", captionFont);
            _lossValueLabel = AddSummaryValue(366, 100);

            AddSummaryCaption(476, "인수량", captionFont);
            _receivedValueLabel = AddSummaryValue(520, 110);

            AddSummaryCaption(640, "금액", captionFont);
            _amountValueLabel = AddSummaryValue(674, 130);
        }

        private void AddSummaryCaption(int x, string caption, Font font)
        {
            var label = new LabelControl
            {
                Location = new Point(x, 10),
                Size = new Size(50, 16),
                Text = caption
            };
            label.Appearance.Font = font;
            label.Appearance.Options.UseFont = true;
            _summaryPanel.Controls.Add(label);
        }

        private LabelControl AddSummaryValue(int x, int width)
        {
            var label = new LabelControl
            {
                Location = new Point(x, 10),
                Size = new Size(width, 16),
                Text = string.Empty
            };
            _summaryPanel.Controls.Add(label);
            return label;
        }

        private void BuildColumns()
        {
            _gridView.Columns.Clear();

            AddColumn("Id", "순번", 65);
            AddColumn("FirstDateTime", "1차계량일", 95, "yyyy-MM-dd");
            AddColumn("SecondDateTime", "2차계량일", 95, "yyyy-MM-dd");
            AddColumn("WeighSeq", "계량순번", 80);
            AddColumn("FirstDateTime", "1차시간", 70, "HH:mm");
            AddColumn("SecondDateTime", "2차시간", 70, "HH:mm");
            AddColumn("VehicleNo", "차량번호", 85);
            AddColumn("OwnerCompany", "차량소속회사", 120);
            AddColumn("DriverName", "운전자", 80);
            AddColumn("CustomerName", "거래처명", 120);
            AddColumn("ProductName", "제품명", 110);
            AddColumn("FirstWeight", "1차중량", 90, "N0");
            AddColumn("SecondWeight", "2차중량", 90, "N0");
            AddColumn("NetWeight", "실중량", 90, "N0");
            AddColumn("LossWeight", "감량", 80, "N0");
            AddColumn("UnitPrice", "단가", 85, "N0");
            AddColumn("Amount", "금액", 100, "N0");
            AddInOutColumn();
            AddStatusColumn();
            AddColumn("WeigherName", "계량자", 100);
            AddColumn("Remark", "비고", 150);
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

        /// <summary>IsCompleted는 bool이라 그냥 컬럼을 추가하면 DevExpress가 자동으로 체크박스
        /// 에디터를 붙여 CustomColumnDisplayText의 텍스트("1차 대기"/"2차 완료")가 반영되지
        /// 않는다. 일반 텍스트 에디터로 바꿔 텍스트가 그대로 보이게 한다.</summary>
        private void AddStatusColumn()
        {
            var column = _gridView.Columns.AddVisible("IsCompleted", "계량상태");
            column.Width = 70;
            column.OptionsColumn.AllowEdit = false;

            var textEdit = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            _gridControl.RepositoryItems.Add(textEdit);
            column.ColumnEdit = textEdit;
        }

        private void GridView_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "InOutType" && e.Value is InOutType inOut)
            {
                e.DisplayText = inOut.ToDisplayString();
            }
            else if (e.Column.FieldName == "IsCompleted" && e.Value is bool isCompleted)
            {
                e.DisplayText = isCompleted ? "2차 완료" : "1차 대기";
            }
            else if ((e.Column.FieldName == "FirstWeight" || e.Column.FieldName == "SecondWeight"
                || e.Column.FieldName == "NetWeight" || e.Column.FieldName == "LossWeight") && e.Value is decimal weight)
            {
                e.DisplayText = FormatWeight(weight);
            }
            else if ((e.Column.FieldName == "UnitPrice" || e.Column.FieldName == "Amount") && e.Value is decimal money)
            {
                e.DisplayText = FormatAmount(money);
            }
        }

        private string FormatWeight(decimal value)
        {
            var text = value.ToString("N0");
            return string.IsNullOrEmpty(_weightUnit) ? text : text + " " + _weightUnit;
        }

        private string FormatAmount(decimal value)
        {
            var text = value.ToString("N0");
            return string.IsNullOrEmpty(_amountUnit) ? text : text + " " + _amountUnit;
        }

        private void PrintList()
        {
            // TODO: XtraReports 로 작성된 계량 데이터 목록 출력 연결.
            ComnFunc.gp_PrintMessage("계량 데이터 목록 인쇄는 준비 중입니다.", "계량 데이터 관리", MessageType.알림);
        }

        /// <summary>Excel 전용 라이브러리 참조가 없어, Excel에서 바로 열리는 CSV로 내보낸다
        /// (한글이 깨지지 않도록 BOM 있는 UTF-8로 저장).</summary>
        private void ExportToCsv()
        {
            if (_displayRecords.Count == 0)
            {
                ComnFunc.gp_PrintMessage("내보낼 데이터가 없습니다.", "안내", MessageType.알림);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV 파일 (*.csv)|*.csv";
                dialog.FileName = string.Format("계량데이터_{0:yyyyMMddHHmm}.csv", DateTime.Now);

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    WriteCsv(dialog.FileName);
                    ComnFunc.gp_PrintMessage("저장되었습니다.\r\n" + dialog.FileName, "안내", MessageType.알림);
                }
                catch (Exception ex)
                {
                    ComnFunc.gp_PrintMessage("저장 중 오류가 발생했습니다.\r\n" + ex.Message, "오류", MessageType.오류);
                }
            }
        }

        private void WriteCsv(string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, new UTF8Encoding(true)))
            {
                writer.WriteLine(string.Join(",", new[]
                {
                    "순번", "1차계량일", "2차계량일", "계량순번", "차량번호", "차량소속회사", "운전자",
                    "거래처명", "제품명", "1차중량", "2차중량", "실중량", "감량", "단가", "금액",
                    "입출구분", "계량상태", "계량자", "비고"
                }.Select(CsvField)));

                foreach (var r in _displayRecords)
                {
                    writer.WriteLine(string.Join(",", new[]
                    {
                        CsvField(r.Id.ToString()),
                        CsvField(r.FirstDateTime.ToString("yyyy-MM-dd")),
                        CsvField(r.SecondDateTime.HasValue ? r.SecondDateTime.Value.ToString("yyyy-MM-dd") : string.Empty),
                        CsvField(r.WeighSeq.ToString()),
                        CsvField(r.VehicleNo),
                        CsvField(r.OwnerCompany),
                        CsvField(r.DriverName),
                        CsvField(r.CustomerName),
                        CsvField(r.ProductName),
                        CsvField(r.FirstWeight.ToString("N0")),
                        CsvField(r.SecondWeight.HasValue ? r.SecondWeight.Value.ToString("N0") : string.Empty),
                        CsvField(r.NetWeight.HasValue ? r.NetWeight.Value.ToString("N0") : string.Empty),
                        CsvField(r.LossWeight.HasValue ? r.LossWeight.Value.ToString("N0") : string.Empty),
                        CsvField(r.UnitPrice.HasValue ? r.UnitPrice.Value.ToString("N0") : string.Empty),
                        CsvField(r.Amount.HasValue ? r.Amount.Value.ToString("N0") : string.Empty),
                        CsvField(r.InOutType.ToDisplayString()),
                        CsvField(r.IsCompleted ? "2차 완료" : "1차 대기"),
                        CsvField(r.WeigherName),
                        CsvField(r.Remark),
                    }));
                }
            }
        }

        private static string CsvField(string value)
        {
            value = value ?? string.Empty;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
