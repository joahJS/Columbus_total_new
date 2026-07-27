using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace ColumbusWeighing.Forms
{
    public partial class LoginForm : XtraForm
    {
        public string UserId { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            _btnOk.Click += BtnOk_Click;
        }

        private void BtnOk_Click(object sender, System.EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_idEdit.Text))
            {
                XtraMessageBox.Show("사용자ID를 입력하세요.", "로그인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            // 데모 환경: 비밀번호 검증은 실제 사용자/권한 저장소 연동 시 구현.
            UserId = _idEdit.Text.Trim();
            DialogResult = DialogResult.OK;
        }
    }
}
