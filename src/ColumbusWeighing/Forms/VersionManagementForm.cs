using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
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
            ComnGridFunc.GridStyleBasicSetting(_gridView);
            _gridView.OptionsView.ShowGroupPanel = false;
            _gridView.OptionsBehavior.Editable = false;

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
    }
}
