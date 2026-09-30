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

            var authService = new SqlAuthenticationService();
            var versionRepository = new SqlVersionRepository();

            // 시스템 설정의 "자동 로그인 사용"이 켜져 있고 "접속정보 기억하기"로 저장된 계정이
            // 있으면 로그인창을 건너뛴다.
            var settings = new IniAppSettingsRepository().Load();
            if (settings.UseAutoLogin && LoginForm.TryAutoLogin(authService, out var autoLoginDisplayName))
            {
                if (AppUpdateService.CheckAndApply(versionRepository))
                {
                    // 새 버전으로 교체하고 재시작하는 배치 스크립트를 이미 띄웠으므로,
                    // MainForm을 열지 않고 그냥 종료해 exe 파일 잠금을 풀어준다.
                    return;
                }

                Application.Run(new MainForm(authService, autoLoginDisplayName));
                return;
            }

            using (var loginForm = new LoginForm(authService))
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    // 로그인 취소/실패 시 메인 화면을 띄우지 않고 프로그램을 종료한다.
                    return;
                }

                if (AppUpdateService.CheckAndApply(versionRepository))
                {
                    return;
                }

                Application.Run(new MainForm(authService, loginForm.UserId));
            }
        }
    }
}
