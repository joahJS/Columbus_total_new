using System;
using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;

namespace ColumbusWeighing.Forms
{
    /// <summary>
    /// 무인계근관리 프로그램 메인 화면.
    /// 상단: 현재 중량 표시 + 통신/이벤트 로그 + 로그인,
    /// 중단: 1차 계량(대기 목록), 하단: 2차계량(당일 완료 목록).
    /// </summary>
    public partial class MainForm : XtraForm
    {
        private const string CompanyName = "콜럼버스 주식회사";

        private readonly IWeighingRepository _repository;
        private readonly IScaleIndicatorService _scaleService;
        private readonly AppLogService _logService;
        private readonly IAuthenticationService _authService;
        private readonly IVersionRepository _versionRepository;
        private readonly IAppSettingsRepository _appSettingsRepository;
        private readonly string _loggedInUserName;

        /// <summary>VS 디자이너 전용(디자인 타임 로드를 위해 필요). 실행 시에는 사용하지 않는다.</summary>
        public MainForm() : this(new FixedAuthenticationService(), "게스트")
        {
        }

        /// <summary>
        /// Program.cs 에서 로그인창을 통과한 뒤에 호출하는 실제 생성자.
        /// </summary>
        public MainForm(IAuthenticationService authService, string loggedInUserName)
        {
            InitializeComponent();

            _repository = new InMemoryWeighingRepository();
            _scaleService = new SimulatedScaleIndicatorService();
            _logService = new AppLogService();
            _authService = authService;
            _versionRepository = new InMemoryVersionRepository();
            _appSettingsRepository = new InMemoryAppSettingsRepository();
            _loggedInUserName = loggedInUserName;

            _btnLogin.Text = loggedInUserName;

            _firstWeighingControl.Initialize(_repository, _scaleService, _logService);
            _secondWeighingControl.Initialize(_repository, _scaleService, _logService);
            _firstWeighingControl.SecondWeighingCompleted += (s, e) => _secondWeighingControl.RefreshView();

            _scaleService.WeightChanged += ScaleService_WeightChanged;
            _logService.LogAdded += LogService_LogAdded;

            _btnLogin.Click += BtnLogin_Click;
            _menuFileExit.Click += (s, e) => Close();
            _menuBaseDataCustomer.Click += (s, e) => ShowNotReady("거래처 관리");
            _menuBaseDataVehicle.Click += (s, e) => ShowNotReady("차량 관리");
            _menuBaseDataProduct.Click += (s, e) => ShowNotReady("제품 관리");
            _menuBaseDataSystemSettings.Click += (s, e) => ShowSystemSettings();
            _menuStatusDaily.Click += (s, e) => ShowNotReady("일일 계량현황");
            _menuStatusPeriod.Click += (s, e) => ShowNotReady("기간별 집계");
            _menuSystemVersion.Click += (s, e) => ShowVersionManagement();

            KeyPreview = true;

            Load += (s, e) =>
            {
                _scaleService.Start();
                _logService.Info(CompanyName, string.Format("{0} 님으로 로그인된 상태로 프로그램이 시작되었습니다.", loggedInUserName));
            };

            FormClosed += (s, e) => _scaleService.Dispose();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F5:
                    _firstWeighingControl.RegisterFirstWeighing();
                    return true;
                case Keys.F6:
                    _firstWeighingControl.CompleteSecondWeighingForSelected();
                    return true;
                case Keys.F7:
                    _secondWeighingControl.RegisterSingleWeighing();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ScaleService_WeightChanged(object sender, WeightChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => ScaleService_WeightChanged(sender, e)));
                return;
            }

            _weightDisplay.Value = e.Weight;
            _weightDisplay.SetStable(e.IsStable);
            _logService.Raw(e.RawPacket);
        }

        private void LogService_LogAdded(object sender, LogEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => LogService_LogAdded(sender, e)));
                return;
            }

            _logMemo.Text += (_logMemo.Text.Length == 0 ? string.Empty : Environment.NewLine) + e.Text;
            _logMemo.SelectionStart = _logMemo.Text.Length;
            _logMemo.ScrollToCaret();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            using (var form = new LoginForm(_authService))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    // 회사명 타이틀은 항상 표시하고, 로그인 버튼에 사용자명을 반영한다.
                    _btnLogin.Text = form.UserId;
                    _logService.Info(CompanyName, string.Format("{0} 님이 로그인했습니다.", form.UserId));
                }
            }
        }

        private void ShowVersionManagement()
        {
            using (var form = new VersionManagementForm(_versionRepository, _loggedInUserName))
            {
                form.ShowDialog(this);
            }
        }

        private void ShowSystemSettings()
        {
            using (var form = new SystemSettingsForm(_appSettingsRepository))
            {
                form.ShowDialog(this);
            }
        }

        private static void ShowNotReady(string menuName)
        {
            ComnFunc.gp_PrintMessage(menuName + " 화면은 준비 중입니다.", "안내", MessageType.알림);
        }
    }
}
