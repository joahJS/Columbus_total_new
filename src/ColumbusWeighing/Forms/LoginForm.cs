using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;

namespace ColumbusWeighing.Forms
{
    public partial class LoginForm : XtraForm
    {
        private readonly IAuthenticationService _authService;

        /// <summary>로그인 성공 시 화면에 표시할 사용자명.</summary>
        public string UserId { get; private set; }

        /// <summary>
        /// 시스템 설정의 "자동 로그인 사용"이 켜져 있으면(호출하는 쪽에서 미리 확인), 로그인창을
        /// 띄우지 않고 바로 인증을 시도한다. "접속정보 기억하기" 체크와는 무관하게, 자동 로그인이
        /// 켜진 상태에서 마지막으로 성공한 로그인 정보(IniKeyAutoLoginId/Pw)를 쓴다 — 그래야
        /// 사용자가 "기억하기"를 체크하지 않아도 자동 로그인이 동작한다.
        /// 성공하면 LoginForm의 BtnOk_Click과 동일하게 LoginUser를 채운다.
        /// </summary>
        public static bool TryAutoLogin(IAuthenticationService authService, out string displayName)
        {
            displayName = null;

            var userId = IniHelper.GetValue(ComnString.IniSectionLogin, ComnString.IniKeyAutoLoginId);
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var password = CredentialProtector.Unprotect(IniHelper.GetValue(ComnString.IniSectionLogin, ComnString.IniKeyAutoLoginPw));
            if (!authService.TryLogin(userId, password, out displayName))
            {
                return false;
            }

            LoginUser.UserId = userId;
            LoginUser.UserName = displayName;
            return true;
        }

        public LoginForm(IAuthenticationService authService)
        {
            InitializeComponent();

            _authService = authService;
            _btnOk.Click += BtnOk_Click;
            Load += LoginForm_Load;
        }

        /// <summary>이전에 "접속정보 기억하기"로 저장해 둔 아이디/비밀번호가 있으면 미리 채워 넣는다.</summary>
        private void LoginForm_Load(object sender, System.EventArgs e)
        {
            var remembered = IniHelper.GetValue(ComnString.IniSectionLogin, ComnString.IniKeyLoginRemember);
            if (remembered != "True")
            {
                return;
            }

            _idEdit.Text = IniHelper.GetValue(ComnString.IniSectionLogin, ComnString.IniKeyLoginId);
            _pwEdit.Text = CredentialProtector.Unprotect(IniHelper.GetValue(ComnString.IniSectionLogin, ComnString.IniKeyLoginPw));
            _chkRemember.Checked = true;
        }

        private void BtnOk_Click(object sender, System.EventArgs e)
        {
            var userId = _idEdit.Text.Trim();
            var password = _pwEdit.Text;

            if (string.IsNullOrWhiteSpace(userId))
            {
                ComnFunc.gp_PrintMessage("사용자ID를 입력하세요.", "로그인", MessageType.경고);
                DialogResult = DialogResult.None;
                return;
            }

            if (!_authService.TryLogin(userId, password, out var displayName))
            {
                ComnFunc.gp_PrintMessage("아이디 또는 비밀번호가 올바르지 않습니다.", "로그인", MessageType.경고);
                _pwEdit.Text = string.Empty;
                _pwEdit.Focus();
                DialogResult = DialogResult.None;
                return;
            }

            UserId = displayName;
            LoginUser.UserId = userId;
            LoginUser.UserName = displayName;

            IniHelper.SetValue(ComnString.IniSectionLogin, ComnString.IniKeyLoginRemember, _chkRemember.Checked.ToString());
            if (_chkRemember.Checked)
            {
                IniHelper.SetValue(ComnString.IniSectionLogin, ComnString.IniKeyLoginId, userId);
                IniHelper.SetValue(ComnString.IniSectionLogin, ComnString.IniKeyLoginPw, CredentialProtector.Protect(password));
            }
            else
            {
                IniHelper.SetValue(ComnString.IniSectionLogin, ComnString.IniKeyLoginId, string.Empty);
                IniHelper.SetValue(ComnString.IniSectionLogin, ComnString.IniKeyLoginPw, string.Empty);
            }

            // "자동 로그인 사용"은 "접속정보 기억하기" 체크와 무관하게 동작해야 하므로, 그 설정이
            // 켜져 있으면 이번에 성공한 로그인 정보를 별도 키에 저장해둔다. 설정이 꺼져 있으면
            // 굳이 저장하지 않는다(사용자가 요청하지 않은 자격증명을 디스크에 남기지 않기 위함).
            if (new IniAppSettingsRepository().Load().UseAutoLogin)
            {
                IniHelper.SetValue(ComnString.IniSectionLogin, ComnString.IniKeyAutoLoginId, userId);
                IniHelper.SetValue(ComnString.IniSectionLogin, ComnString.IniKeyAutoLoginPw, CredentialProtector.Protect(password));
            }

            DialogResult = DialogResult.OK;
        }
    }
}
