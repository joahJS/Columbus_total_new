using System.Windows.Forms;
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
        }

        private void BtnOk_Click(object sender, System.EventArgs e)
        {
            var userId = _idEdit.Text.Trim();
            var password = _pwEdit.Text;

            if (string.IsNullOrWhiteSpace(userId))
            {
                XtraMessageBox.Show("사용자ID를 입력하세요.", "로그인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            if (!_authService.TryLogin(userId, password, out var displayName))
            {
                XtraMessageBox.Show("아이디 또는 비밀번호가 올바르지 않습니다.", "로그인",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _pwEdit.Text = string.Empty;
                _pwEdit.Focus();
                DialogResult = DialogResult.None;
                return;
            }

            UserId = displayName;
            DialogResult = DialogResult.OK;
        }
    }
}
