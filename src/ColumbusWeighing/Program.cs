using System;
using System.Windows.Forms;
using ColumbusWeighing.Forms;
using ColumbusWeighing.Services;
using DevExpress.LookAndFeel;
using DevExpress.Skins;

namespace ColumbusWeighing
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            SkinManager.EnableFormSkins();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 참고 화면은 최신 플랫 스킨이 아닌 클래식한 Windows 스타일이므로 기본(Basic) 스킨을 사용한다.
            UserLookAndFeel.Default.SetSkinStyle("Basic");

            var authService = new FixedAuthenticationService();

            using (var loginForm = new LoginForm(authService))
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    // 로그인 취소/실패 시 메인 화면을 띄우지 않고 프로그램을 종료한다.
                    return;
                }

                Application.Run(new MainForm(authService, loginForm.UserId));
            }
        }
    }
}
