using System.Drawing;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;

namespace ColumbusWeighing.Forms
{
    /// <summary>
    /// 1차/2차 계량 그리드에서 항상 보여주는 핵심 컬럼(차량번호/중량/계량자/비고 등) 외에
    /// 선택적으로 켜고 끌 수 있는 부가 컬럼을 고르는 팝업. 참고 화면(TS2020)의
    /// "계량 화면 설정"과 동일한 구성이다.
    /// </summary>
    public partial class WeighingColumnSettingsForm : XtraForm
    {
        private readonly IWeighingColumnSettingsRepository _repository;

        private CheckEdit _chkWeighSeq;
        private CheckEdit _chkFirstDateTime;
        private CheckEdit _chkSecondDateTime;
        private CheckEdit _chkOwnerCompany;
        private CheckEdit _chkProductName;
        private CheckEdit _chkCustomerName;
        private CheckEdit _chkDriverName;
        private CheckEdit _chkLossWeight;
        private CheckEdit _chkPriceInfo;
        private CheckEdit _chkInOutType;
        // 감량률(%)/비중·환산중량/담당자는 연결할 실제 데이터가 없어 항목 자체를 숨겼다
        // (WeighingColumnSettings.cs 참고).

        public WeighingColumnSettingsForm(IWeighingColumnSettingsRepository repository)
        {
            InitializeComponent();

            _repository = repository;

            BuildCheckboxRows();
            LoadFromSettings(_repository.Load());

            _btnReset.Click += (s, e) => LoadFromSettings(new WeighingColumnSettings());
            _btnSave.Click += (s, e) => Save();
            _btnClose.Click += (s, e) => Close();
            KeyPreview = true;
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            if (keyData == System.Windows.Forms.Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void BuildCheckboxRows()
        {
            const int rowHeight = 34;
            const int rowGap = 0;
            var y = 0;

            _chkWeighSeq = AddRow(ref y, rowHeight, rowGap, "계량 순번");
            _chkFirstDateTime = AddRow(ref y, rowHeight, rowGap, "1차 일자 / 1차 시간");
            _chkSecondDateTime = AddRow(ref y, rowHeight, rowGap, "2차 일자 / 2차 시간");
            _chkOwnerCompany = AddRow(ref y, rowHeight, rowGap, "차량 소속 회사");
            _chkProductName = AddRow(ref y, rowHeight, rowGap, "제품");
            _chkCustomerName = AddRow(ref y, rowHeight, rowGap, "거래처");
            _chkDriverName = AddRow(ref y, rowHeight, rowGap, "운전자");
            _chkLossWeight = AddRow(ref y, rowHeight, rowGap, "감량(kg)");
            _chkPriceInfo = AddRow(ref y, rowHeight, rowGap, "단가 / 금액");
            _chkInOutType = AddRow(ref y, rowHeight, rowGap, "입·출고 구분");
            // 감량률(%)/비중·환산중량/담당자 행은 연결할 실제 데이터가 없어 뺐다.
        }

        private CheckEdit AddRow(ref int y, int height, int gap, string caption)
        {
            var check = new CheckEdit
            {
                Location = new Point(0, y),
                Size = new Size(400, height),
                Padding = new System.Windows.Forms.Padding(12, 4, 12, 4)
            };
            check.Properties.Caption = caption;
            check.Properties.Appearance.BackColor = Color.FromArgb(163, 204, 235);
            check.Properties.Appearance.Options.UseBackColor = true;
            check.Properties.Appearance.Font = new Font("맑은 고딕", 12F);
            check.Properties.Appearance.Options.UseFont = true;
            _bodyPanel.Controls.Add(check);

            y += height + gap;
            return check;
        }

        private void LoadFromSettings(WeighingColumnSettings settings)
        {
            _chkWeighSeq.Checked = settings.ShowWeighSeq;
            _chkFirstDateTime.Checked = settings.ShowFirstDateTime;
            _chkSecondDateTime.Checked = settings.ShowSecondDateTime;
            _chkOwnerCompany.Checked = settings.ShowOwnerCompany;
            _chkProductName.Checked = settings.ShowProductName;
            _chkCustomerName.Checked = settings.ShowCustomerName;
            _chkDriverName.Checked = settings.ShowDriverName;
            _chkLossWeight.Checked = settings.ShowLossWeight;
            _chkPriceInfo.Checked = settings.ShowPriceInfo;
            _chkInOutType.Checked = settings.ShowInOutType;
        }

        private void Save()
        {
            var settings = new WeighingColumnSettings
            {
                ShowWeighSeq = _chkWeighSeq.Checked,
                ShowFirstDateTime = _chkFirstDateTime.Checked,
                ShowSecondDateTime = _chkSecondDateTime.Checked,
                ShowOwnerCompany = _chkOwnerCompany.Checked,
                ShowProductName = _chkProductName.Checked,
                ShowCustomerName = _chkCustomerName.Checked,
                ShowDriverName = _chkDriverName.Checked,
                ShowLossWeight = _chkLossWeight.Checked,
                ShowPriceInfo = _chkPriceInfo.Checked,
                ShowInOutType = _chkInOutType.Checked
            };

            _repository.Save(settings);
            ComnFunc.gp_PrintMessage("저장되었습니다.", "계량 화면 설정", MessageType.알림);
        }
    }
}
