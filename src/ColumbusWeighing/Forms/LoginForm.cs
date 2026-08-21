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

            DialogResult = DialogResult.OK;
        }
    }
}
