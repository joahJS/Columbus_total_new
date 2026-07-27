using System;
using System.Windows.Forms;
using ColumbusWeighing.Forms;
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

            Application.Run(new MainForm());
        }
    }
}
