using System.Drawing;
using System.Windows.Forms;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;

namespace ColumbusWeighing.Forms
{
    /// <summary>
    /// 프로그램 버전 이력을 조회/등록하는 화면.
    /// VisionIns 솔루션의 버전관리(SY000F00) 화면과 동일한 구성(조회/추가/닫기)이다.
    /// </summary>
    public partial class VersionManagementForm : XtraForm
    {
        private readonly IVersionRepository _repository;
        private readonly string _loginUserName;
        private GridColumn _colVersionId;

        public VersionManagementForm(IVersionRepository repository, string loginUserName)
        {
            InitializeComponent();

            _repository = repository;
            _loginUserName = loginUserName;

            BuildColumns();
            ApplyGridAppearance();

            _gridControl.DataSource = _repository.Records;

            _btnRetrieve.Click += (s, e) => Retrieve();
            _btnAdd.Click += (s, e) => OpenAddForm();
            _btnClose.Click += (s, e) => Close();

            KeyPreview = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F5:
                    Retrieve();
                    return true;
                case Keys.F1:
                    OpenAddForm();
                    return true;
                case Keys.Escape:
                    Close();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Retrieve()
        {
            _gridView.RefreshData();
        }

        private void OpenAddForm()
        {
            using (var form = new VersionAddForm(_repository, _loginUserName))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    Retrieve();
                    FocusVersion(form.SavedVersionId);
                }
            }
        }

        private void FocusVersion(string versionId)
        {
            if (string.IsNullOrEmpty(versionId))
            {
                return;
            }

            var rowHandle = _gridView.LocateByDisplayText(0, _colVersionId, versionId);
            if (rowHandle >= 0)
            {
                _gridView.FocusedRowHandle = rowHandle;
            }
        }

        private void BuildColumns()
        {
            _gridView.Columns.Clear();

            _colVersionId = AddColumn("VersionId", "버전", 100);
            AddColumn("UploadDate", "업로드일자", 100, "yyyy-MM-dd");
            AddColumn("FileName", "파일이름", 160);
            AddColumn("FileSize", "파일크기", 100, "N0");
            AddColumn("Remark", "비고", 1000);
        }

        private GridColumn AddColumn(string fieldName, string caption, int width, string format = null)
        {
            var column = _gridView.Columns.AddVisible(fieldName, caption);
            column.Width = width;
            column.OptionsColumn.AllowEdit = false;

            if (!string.IsNullOrEmpty(format))
            {
                column.DisplayFormat.FormatType = format == "yyyy-MM-dd"
                    ? DevExpress.Utils.FormatType.DateTime
                    : DevExpress.Utils.FormatType.Numeric;
                column.DisplayFormat.FormatString = format;
            }

            return column;
        }

        /// <summary>참고 화면과 동일한 배색(크림색 짝수행/흰색 홀수행, 하늘색 헤더, 파란색 선택행)으로 맞춘다.</summary>
        private void ApplyGridAppearance()
        {
            _gridView.RowHeight = 22;
            _gridView.OptionsView.ShowGroupPanel = false;
            _gridView.OptionsBehavior.Editable = false;

            _gridView.Appearance.HeaderPanel.BackColor = Color.FromArgb(198, 217, 241);
            _gridView.Appearance.HeaderPanel.ForeColor = Color.Black;
            _gridView.Appearance.HeaderPanel.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            _gridView.Appearance.HeaderPanel.Options.UseBackColor = true;
            _gridView.Appearance.HeaderPanel.Options.UseForeColor = true;
            _gridView.Appearance.HeaderPanel.Options.UseFont = true;
            _gridView.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            _gridView.OptionsView.EnableAppearanceEvenRow = true;
            _gridView.Appearance.EvenRow.BackColor = Color.FromArgb(255, 251, 224);
            _gridView.Appearance.EvenRow.Options.UseBackColor = true;
            _gridView.Appearance.OddRow.BackColor = Color.White;
            _gridView.Appearance.OddRow.Options.UseBackColor = true;
            _gridView.Appearance.Row.Font = new Font("맑은 고딕", 9F);
            _gridView.Appearance.Row.Options.UseFont = true;

            _gridView.Appearance.FocusedRow.BackColor = Color.FromArgb(51, 153, 255);
            _gridView.Appearance.FocusedRow.ForeColor = Color.White;
            _gridView.Appearance.FocusedRow.Options.UseBackColor = true;
            _gridView.Appearance.FocusedRow.Options.UseForeColor = true;
            _gridView.Appearance.SelectedRow.BackColor = Color.FromArgb(51, 153, 255);
            _gridView.Appearance.SelectedRow.ForeColor = Color.White;
            _gridView.Appearance.SelectedRow.Options.UseBackColor = true;
            _gridView.Appearance.SelectedRow.Options.UseForeColor = true;
        }
    }
}
