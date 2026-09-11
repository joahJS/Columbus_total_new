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

            var contentHeight = BuildCheckboxRows();
            ResizeToContent(contentHeight);
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

        /// <summary>체크박스 행을 모두 만들고, 실제로 쌓인 총 높이(px)를 돌려준다. 이 값으로
        /// 본문 패널/팝업 높이를 정확히 맞춰서(ResizeToContent) 스크롤이 생기지 않게 한다.</summary>
        private int BuildCheckboxRows()
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

            return y;
        }

        /// <summary>팝업 전체 높이를 실제 항목 총 높이에 맞춘다. 디자이너에 적어둔 고정 수치와
        /// 항목 수가 어긋나 스크롤이 생기거나 마지막 항목이 가려지는 것을 막기 위해, 하드코딩된
        /// 값 대신 BuildCheckboxRows가 실제로 쌓은 높이를 그대로 쓴다. _bodyPanel은 Dock=Fill이라
        /// 직접 크기를 지정해도 소용없으므로, 폼의 ClientSize를 바꿔 그 결과로 맞춘다.</summary>
        private void ResizeToContent(int contentHeight)
        {
            ClientSize = new Size(ClientSize.Width, _topBar.Height + contentHeight);
        }

        /// <summary>체크박스 자체(CheckEdit)의 WinForms Padding은 owner-drawn 컨트롤이라 무시되므로,
        /// 배경색 있는 PanelControl로 감싸고 그 Padding으로 상하좌우 여백을 실제로 반영한다.</summary>
        private CheckEdit AddRow(ref int y, int height, int gap, string caption)
        {
            var rowColor = Color.FromArgb(163, 204, 235);

            var row = new PanelControl
            {
                Location = new Point(0, y),
                Size = new Size(_bodyPanel.ClientSize.Width, height),
                Padding = new System.Windows.Forms.Padding(12, 4, 12, 4),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            row.Appearance.BackColor = rowColor;
            row.Appearance.Options.UseBackColor = true;

            var check = new CheckEdit { Dock = System.Windows.Forms.DockStyle.Fill };
            check.Properties.Caption = caption;
            check.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            check.Properties.Appearance.BackColor = rowColor;
            check.Properties.Appearance.Options.UseBackColor = true;
            check.Properties.Appearance.Font = new Font("맑은 고딕", 12F);
            check.Properties.Appearance.Options.UseFont = true;

            // 항목 사이 구분을 위해 각 항목 아래쪽에 1px 경계선을 직접 그린다(테두리 있는
            // 컨트롤을 감싸는 대신 Paint에서 그리면 Padding 영역까지 선이 꽉 차게 나온다).
            var borderColor = Color.FromArgb(110, 150, 185);
            row.Paint += (s, e) =>
            {
                using (var pen = new Pen(borderColor))
                {
                    e.Graphics.DrawLine(pen, 0, row.Height - 1, row.Width, row.Height - 1);
                }
            };

            row.Controls.Add(check);
            _bodyPanel.Controls.Add(row);

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
