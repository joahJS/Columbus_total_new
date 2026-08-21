using System.Drawing;
using DevExpress.XtraGrid.Views.Grid;

namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// GridView 공통 스타일 적용 헬퍼.
    /// VisionIns 솔루션의 ComnGridFunc.GridStyleBasicSetting과 동일한 배색(크림색 짝수행/흰색
    /// 홀수행, 하늘색 헤더, 파란색 선택행)을 적용한다. 각 화면마다 복붙되어 있던 코드를 여기 하나로 모았다.
    /// </summary>
    public static class ComnGridFunc
    {
        public static void GridStyleBasicSetting(GridView view)
        {
            view.RowHeight = 22;

            view.Appearance.HeaderPanel.BackColor = Color.FromArgb(198, 217, 241);
            view.Appearance.HeaderPanel.ForeColor = Color.Black;
            view.Appearance.HeaderPanel.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            view.Appearance.HeaderPanel.Options.UseBackColor = true;
            view.Appearance.HeaderPanel.Options.UseForeColor = true;
            view.Appearance.HeaderPanel.Options.UseFont = true;
            view.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            view.OptionsView.EnableAppearanceEvenRow = true;
            view.Appearance.EvenRow.BackColor = Color.FromArgb(255, 251, 224);
            view.Appearance.EvenRow.Options.UseBackColor = true;
            view.Appearance.OddRow.BackColor = Color.White;
            view.Appearance.OddRow.Options.UseBackColor = true;
            view.Appearance.Row.Font = new Font("맑은 고딕", 9F);
            view.Appearance.Row.Options.UseFont = true;

            view.Appearance.FocusedRow.BackColor = Color.FromArgb(51, 153, 255);
            view.Appearance.FocusedRow.ForeColor = Color.White;
            view.Appearance.FocusedRow.Options.UseBackColor = true;
            view.Appearance.FocusedRow.Options.UseForeColor = true;
            view.Appearance.SelectedRow.BackColor = Color.FromArgb(51, 153, 255);
            view.Appearance.SelectedRow.ForeColor = Color.White;
            view.Appearance.SelectedRow.Options.UseBackColor = true;
            view.Appearance.SelectedRow.Options.UseForeColor = true;
        }
    }
}
