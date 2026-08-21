using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;

namespace ColumbusWeighing.Forms
{
    /// <summary>
    /// 새 버전 이력을 등록하는 화면.
    /// VisionIns 솔루션의 SY000F01(새 버전 추가) 화면과 동일한 구성(버전ID/파일명/파일크기/비고 + 파일업로드/저장)이다.
    /// </summary>
    public partial class VersionAddForm : XtraForm
    {
        private readonly IVersionRepository _repository;
        private readonly string _loginUserName;
        private byte[] _uploadedFileData;

        /// <summary>저장 완료 후 목록 화면에서 방금 등록한 버전을 포커스하기 위해 사용.</summary>
        public string SavedVersionId { get; private set; }

        public VersionAddForm(IVersionRepository repository, string loginUserName)
        {
            InitializeComponent();

            _repository = repository;
            _loginUserName = loginUserName;

            _tx_Version.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            _btnUpload.Click += BtnUpload_Click;
            _btnSave.Click += BtnSave_Click;
            _btnClose.Click += (s, e) => Close();

            KeyPreview = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F3)
            {
                BtnSave_Click(this, EventArgs.Empty);
                return true;
            }

            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void BtnUpload_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "실행 파일 (*.exe)|*.exe|모든 파일 (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                _uploadedFileData = File.ReadAllBytes(dialog.FileName);
                _tx_FileName.Text = Path.GetFileName(dialog.FileName);
                _tx_FileSize.Text = _uploadedFileData.LongLength.ToString("N0");
            }

            ComnFunc.gp_PrintMessage("저장을 눌러주세요.", "안내", MessageType.알림);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var versionId = _tx_Version.Text.Trim();

            if (_uploadedFileData == null)
            {
                ComnFunc.gp_PrintMessage("파일을 업로드하세요.", "안내", MessageType.경고);
                return;
            }

            if (string.IsNullOrEmpty(versionId))
            {
                ComnFunc.gp_PrintMessage("VersionID를 입력하세요(예 : 1.1.10)", "안내", MessageType.경고);
                _tx_Version.Focus();
                return;
            }

            var record = _repository.AddVersion(
                versionId,
                DateTime.Now,
                _tx_FileName.Text,
                _uploadedFileData,
                _mm_Remark.Text,
                _loginUserName);

            SavedVersionId = record.VersionId;

            ComnFunc.gp_PrintMessage("저장되었습니다.", "안내", MessageType.알림);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
