using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;

namespace ColumbusWeighing.Forms
{
    /// <summary>
    /// 계량 조회/집계 프로그램 메인 화면 (A/B/C 지점 공용, 조회 전용).
    /// 상단: 로그/로그인, 중단: 1차 계량 대기 조회, 하단: 2차계량 완료 조회.
    /// 실제 계량 입력(1차/2차/1회 계량)은 각 지점이 지금 쓰는 프로그램(TS2020/MES)에서
    /// 그대로 처리하며, 이 프로그램은 통합 허브 DB에서 그 결과를 읽어와 보여주기만 한다.
    /// </summary>
    public partial class MainForm : XtraForm
    {
        private const string CompanyName = "콜럼버스 주식회사";

        /// <summary>사용자 입력으로 취급할 윈도우 메시지(마우스 이동/클릭/휠, 키 입력).
        /// 관리자 자동오프 유휴시간 판정에 쓰인다.</summary>
        private static readonly HashSet<int> ActivityMessages = new HashSet<int>
        {
            0x0100, // WM_KEYDOWN
            0x0201, // WM_LBUTTONDOWN
            0x0204, // WM_RBUTTONDOWN
            0x0207, // WM_MBUTTONDOWN
            0x0200, // WM_MOUSEMOVE
            0x020A  // WM_MOUSEWHEEL
        };

        private readonly IWeighingRepository _repository;
        private readonly AppLogService _logService;
        private readonly IAuthenticationService _authService;
        private readonly IVersionRepository _versionRepository;
        private readonly IAppSettingsRepository _appSettingsRepository;
        private readonly IWeighingColumnSettingsRepository _weighingColumnSettingsRepository;
        private readonly string _loggedInUserName;
        private readonly ActivityMessageFilter _activityFilter;
        private readonly Timer _idleTimer = new Timer { Interval = 15000 };

        private int _adminAutoOffMinutes;
        private DateTime _lastActivityUtc = DateTime.UtcNow;

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

            _repository = new SqlWeighingRepository();
            _logService = new AppLogService();
            _authService = authService;
            _versionRepository = new InMemoryVersionRepository();
            _appSettingsRepository = new IniAppSettingsRepository();
            _weighingColumnSettingsRepository = new IniWeighingColumnSettingsRepository();
            _loggedInUserName = loggedInUserName;

            _btnLogin.Text = loggedInUserName;

            var settings = _appSettingsRepository.Load();
            _firstWeighingControl.Initialize(_repository, settings);
            _secondWeighingControl.Initialize(_repository, settings);
            ApplyRuntimeSettings(settings);
            ApplyColumnSettings(_weighingColumnSettingsRepository.Load());

            _activityFilter = new ActivityMessageFilter(() => _lastActivityUtc = DateTime.UtcNow);
            Application.AddMessageFilter(_activityFilter);
            _idleTimer.Tick += IdleTimer_Tick;

            _logService.LogAdded += LogService_LogAdded;

            _btnLogin.Click += BtnLogin_Click;
            _menuFileExit.Click += (s, e) => Close();
            _menuBaseDataCustomer.Click += (s, e) => ShowNotReady("거래처 관리");
            _menuBaseDataVehicle.Click += (s, e) => ShowNotReady("차량 관리");
            _menuBaseDataProduct.Click += (s, e) => ShowProductManagement();
            _menuBaseDataSystemSettings.Click += (s, e) => ShowSystemSettings();
            _menuBaseDataWeighingColumns.Click += (s, e) => ShowWeighingColumnSettings();
            _menuStatusDaily.Click += (s, e) => ShowNotReady("일일 계량현황");
            _menuStatusPeriod.Click += (s, e) => ShowNotReady("기간별 집계");
            _menuSystemVersion.Click += (s, e) => ShowVersionManagement();
            _menuSystemUser.Click += (s, e) => ShowUserManagement();

            Load += (s, e) =>
            {
                _logService.Info(CompanyName, string.Format("{0} 님으로 로그인된 상태로 프로그램이 시작되었습니다.", loggedInUserName));
            };

            FormClosed += (s, e) =>
            {
                Application.RemoveMessageFilter(_activityFilter);
                _idleTimer.Dispose();
            };
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

            // 저장 여부와 무관하게 현재 저장된 값을 다시 읽어 화면(그리드 폰트/중량 단위/로그 저장/
            // 관리자 자동오프)에 즉시 반영한다. 조회 중인 날짜 필터 등은 건드리지 않는다.
            ApplyRuntimeSettings(_appSettingsRepository.Load());
        }

        private void ShowWeighingColumnSettings()
        {
            using (var form = new WeighingColumnSettingsForm(_weighingColumnSettingsRepository))
            {
                form.ShowDialog(this);
            }

            ApplyColumnSettings(_weighingColumnSettingsRepository.Load());
        }

        private void ShowProductManagement()
        {
            using (var form = new ProductManagementForm(new SqlProductRepository()))
            {
                form.ShowDialog(this);
            }
        }

        private void ShowUserManagement()
        {
            using (var form = new UserManagementForm(new SqlUserRepository(), _loggedInUserName))
            {
                form.ShowDialog(this);
            }
        }

        /// <summary>"계량 화면 설정"에서 고른 부가 컬럼 표시 여부를 두 그리드에 반영한다.</summary>
        private void ApplyColumnSettings(WeighingColumnSettings settings)
        {
            _firstWeighingControl.ApplyColumnSettings(settings);
            _secondWeighingControl.ApplyColumnSettings(settings);
        }

        /// <summary>시스템 설정 값을 실제 동작(그리드 표시/로그 저장/관리자 자동오프)에 반영한다.</summary>
        private void ApplyRuntimeSettings(AppSettings settings)
        {
            _logService.SaveToFile = settings.SaveLogData;
            _firstWeighingControl.ApplyDisplaySettings(settings);
            _secondWeighingControl.ApplyDisplaySettings(settings);

            _adminAutoOffMinutes = settings.AdminAutoOffMinutes;
            if (_adminAutoOffMinutes > 0)
            {
                _lastActivityUtc = DateTime.UtcNow;
                _idleTimer.Start();
            }
            else
            {
                _idleTimer.Stop();
            }
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            if (_adminAutoOffMinutes <= 0)
            {
                return;
            }

            if ((DateTime.UtcNow - _lastActivityUtc).TotalMinutes < _adminAutoOffMinutes)
            {
                return;
            }

            LockForInactivity();
        }

        /// <summary>관리자 자동오프(분) 초과 시 재로그인을 요구한다. 취소하면 프로그램을 종료한다
        /// (계근 데이터가 보이는 화면을 인증 없이 열어둘 수는 없기 때문).</summary>
        private void LockForInactivity()
        {
            _idleTimer.Stop();
            _logService.Info(CompanyName, string.Format("{0}분 이상 조작이 없어 자동으로 로그아웃되었습니다.", _adminAutoOffMinutes));

            using (var form = new LoginForm(_authService))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _btnLogin.Text = form.UserId;
                    _logService.Info(CompanyName, string.Format("{0} 님이 다시 로그인했습니다.", form.UserId));
                }
                else
                {
                    Close();
                    return;
                }
            }

            _lastActivityUtc = DateTime.UtcNow;
            _idleTimer.Start();
        }

        private static void ShowNotReady(string menuName)
        {
            ComnFunc.gp_PrintMessage(menuName + " 화면은 준비 중입니다.", "안내", MessageType.알림);
        }

        /// <summary>애플리케이션 전역 마우스/키보드 입력을 감지해 관리자 자동오프 유휴시간을 갱신한다.
        /// 자식 그리드 컨트롤 위에서 발생한 입력은 Form 이벤트로 버블링되지 않으므로 메시지 필터로 잡는다.</summary>
        private sealed class ActivityMessageFilter : IMessageFilter
        {
            private readonly Action _onActivity;

            public ActivityMessageFilter(Action onActivity)
            {
                _onActivity = onActivity;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (ActivityMessages.Contains(m.Msg))
                {
                    _onActivity();
                }

                return false;
            }
        }
    }
}
