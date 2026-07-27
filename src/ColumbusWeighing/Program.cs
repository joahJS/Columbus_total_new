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

            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Colorful");

            Application.Run(new MainForm());
        }
    }
}
