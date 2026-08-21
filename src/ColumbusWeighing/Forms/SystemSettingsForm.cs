using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;

namespace ColumbusWeighing.Forms
{
    /// <summary>
    /// 회사 정보/계량 옵션/인쇄/카메라 설정을 한 화면에서 관리하는 시스템 설정 팝업.
    /// 참고 화면(TS2020)의 "시스템 설정" 창과 동일한 구성(사용자 설정/계량 설정/인쇄 설정/카메라 설정/IP 카메라 설정)이다.
    /// </summary>
    public partial class SystemSettingsForm : XtraForm
    {
        private readonly IAppSettingsRepository _repository;

        private TextEdit _txtBizNo;
        private TextEdit _txtCompanyName;
        private TextEdit _txtCeoName;
        private TextEdit _txtManagerName;
        private TextEdit _txtAddress;
        private TextEdit _txtBizType;
        private TextEdit _txtBizItem;
        private TextEdit _txtPhone;
        private TextEdit _txtFax;

        private SpinEdit _numVehicleThreshold;
        private SpinEdit _numWeightDeviation;
        private CheckEdit _chkUseBroadcast;
        private SpinEdit _numStableSeconds;
        private CheckEdit _chkCopySecondToFirst;
        private CheckEdit _chkMoveSecondToFirst;
        private CheckEdit _chkEditFirstOnMain;
        private CheckEdit _chkEditSecondOnMain;
        private ComboBoxEdit _cboInOutRule;
        private CheckEdit _chkLoadLastOnFirst;
        private CheckEdit _chkUseDispatch;
        private CheckEdit _chkAutoLogin;
        private CheckEdit _chkSaveLog;
        private SpinEdit _numAdminAutoOffMinutes;
        private TextEdit _txtClosingTime;
        private SpinEdit _numGridFontSize;
        private TextEdit _txtWeightUnit;
        private TextEdit _txtAmountUnit;

        private readonly SimpleButton[] _approvalButtons = new SimpleButton[4];
        private int _approvalColumnCount;
        private ComboBoxEdit _cboReportPrinter;

        private ComboBoxEdit _cboCameraCount;
        private TextEdit _txtPhotoFolder;

        private DevExpress.XtraGrid.GridControl _gridIpCamera;
        private DevExpress.XtraGrid.Views.Grid.GridView _gridViewIpCamera;
        private readonly BindingList<IpCameraSetting> _ipCameras = new BindingList<IpCameraSetting>();

        public SystemSettingsForm(IAppSettingsRepository repository)
        {
            InitializeComponent();

            _repository = repository;

            BuildUserSection();
            BuildWeighingSection();
            BuildPrintSection();
            BuildCameraSection();
            BuildIpCameraSection();

            LoadFromSettings(_repository.Load());

            _btnSave.Click += (s, e) => Save();
            _btnClose.Click += (s, e) => Close();
            KeyPreview = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region [사용자 설정]

        private void BuildUserSection()
        {
            AddLabel(_grpUser, 20, 30, 90, "사업자등록번호");
            _txtBizNo = AddTextEdit(_grpUser, 118, 27, 200);

            AddLabel(_grpUser, 20, 58, 90, "회사명");
            _txtCompanyName = AddTextEdit(_grpUser, 118, 55, 330);

            AddLabel(_grpUser, 20, 86, 90, "대표자");
            _txtCeoName = AddTextEdit(_grpUser, 118, 83, 140);
            AddLabel(_grpUser, 270, 86, 60, "담당자");
            _txtManagerName = AddTextEdit(_grpUser, 336, 83, 112);

            AddLabel(_grpUser, 20, 114, 90, "주소");
            _txtAddress = AddTextEdit(_grpUser, 118, 111, 330);

            AddLabel(_grpUser, 20, 142, 90, "업태");
            _txtBizType = AddTextEdit(_grpUser, 118, 139, 330);

            AddLabel(_grpUser, 20, 170, 90, "종목");
            _txtBizItem = AddTextEdit(_grpUser, 118, 167, 330);

            AddLabel(_grpUser, 20, 198, 90, "전화번호");
            _txtPhone = AddTextEdit(_grpUser, 118, 195, 140);
            AddLabel(_grpUser, 270, 198, 60, "Fax");
            _txtFax = AddTextEdit(_grpUser, 336, 195, 112);
        }

        #endregion

        #region [계량 설정]

        private void BuildWeighingSection()
        {
            AddLabel(_grpWeighing, 20, 30, 100, "차량인식기준");
            _numVehicleThreshold = AddSpinEdit(_grpWeighing, 124, 27, 70, 0, 100000);
            AddLabel(_grpWeighing, 250, 30, 100, "중량 판정 편차");
            _numWeightDeviation = AddSpinEdit(_grpWeighing, 354, 27, 70, 0, 100000);

            _chkUseBroadcast = AddCheckEdit(_grpWeighing, 20, 58, 180, "PC 안내방송 사용");
            AddLabel(_grpWeighing, 230, 60, 150, "중량안정판정시간(1~10초)");
            _numStableSeconds = AddSpinEdit(_grpWeighing, 384, 57, 40, 1, 10);

            _chkCopySecondToFirst = AddCheckEdit(_grpWeighing, 20, 86, 220, "2차 계량 자료 1차로 복사");
            _chkMoveSecondToFirst = AddCheckEdit(_grpWeighing, 250, 86, 210, "2차 계량 자료 1차로 이동");

            _chkEditFirstOnMain = AddCheckEdit(_grpWeighing, 20, 114, 220, "메인화면 1차 계량자료 수정");
            _chkEditSecondOnMain = AddCheckEdit(_grpWeighing, 250, 114, 210, "메인화면 2차계량자료 수정");

            AddLabel(_grpWeighing, 20, 144, 70, "입출고 구분");
            _cboInOutRule = AddComboEdit(_grpWeighing, 95, 141, 355);
            _cboInOutRule.Properties.Items.AddRange(new object[]
            {
                "1차>2차 [입고], 2차>1차 [출고]",
                "1차>2차 [출고], 2차>1차 [입고]"
            });

            _chkLoadLastOnFirst = AddCheckEdit(_grpWeighing, 20, 170, 430, "1차, 1회 계량시 최종 자료 읽어오기");

            _chkUseDispatch = AddCheckEdit(_grpWeighing, 20, 198, 150, "배차 사용");

            _chkAutoLogin = AddCheckEdit(_grpWeighing, 20, 226, 180, "자동 로그인 사용");
            _chkSaveLog = AddCheckEdit(_grpWeighing, 250, 226, 180, "로그 데이터 저장");

            AddLabel(_grpWeighing, 20, 256, 120, "관리자 자동오프(분)");
            _numAdminAutoOffMinutes = AddSpinEdit(_grpWeighing, 145, 253, 60, 0, 999);

            AddLabel(_grpWeighing, 20, 284, 90, "마감 기준 시간");
            _txtClosingTime = AddTextEdit(_grpWeighing, 115, 281, 80);
            AddLabel(_grpWeighing, 230, 284, 120, "메인화면 그리드 폰트");
            _numGridFontSize = AddSpinEdit(_grpWeighing, 355, 281, 60, 6, 24);

            AddLabel(_grpWeighing, 20, 312, 70, "중량 단위");
            _txtWeightUnit = AddTextEdit(_grpWeighing, 95, 309, 80);
            AddLabel(_grpWeighing, 250, 312, 70, "금액 단위");
            _txtAmountUnit = AddTextEdit(_grpWeighing, 325, 309, 80);
        }

        #endregion

        #region [인쇄 설정]

        private void BuildPrintSection()
        {
            var subLabel = new LabelControl
            {
                Location = new Point(20, 30),
                Size = new Size(150, 16),
                Text = "보고서 인쇄 설정"
            };
            subLabel.Appearance.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            subLabel.Appearance.Options.UseFont = true;
            _grpPrint.Controls.Add(subLabel);

            AddLabel(_grpPrint, 20, 56, 70, "결재란 설정");
            var approvalCounts = new[] { 4, 3, 2, 1 };
            var buttonX = 95;
            for (var i = 0; i < approvalCounts.Length; i++)
            {
                var count = approvalCounts[i];
                var button = new SimpleButton
                {
                    Location = new Point(buttonX, 53),
                    Size = new Size(55, 24),
                    Text = string.Format("결재{0}", count),
                    Tag = count
                };
                button.Click += (s, e) => SelectApprovalColumnCount(count);
                _grpPrint.Controls.Add(button);
                _approvalButtons[i] = button;
                buttonX += 58;
            }

            AddLabel(_grpPrint, 20, 88, 80, "보고서프린터");
            _cboReportPrinter = AddComboEdit(_grpPrint, 105, 85, 345);
            _cboReportPrinter.Properties.Items.Clear();
            foreach (string printerName in PrinterSettings.InstalledPrinters)
            {
                _cboReportPrinter.Properties.Items.Add(printerName);
            }
        }

        private void SelectApprovalColumnCount(int count)
        {
            _approvalColumnCount = count;
            foreach (var button in _approvalButtons)
            {
                var isSelected = button.Tag is int tagValue && tagValue == count;
                button.Appearance.BackColor = isSelected ? Color.FromArgb(41, 128, 225) : Color.Empty;
                button.Appearance.ForeColor = isSelected ? Color.White : Color.Empty;
                button.Appearance.Options.UseBackColor = isSelected;
                button.Appearance.Options.UseForeColor = isSelected;
            }
        }

        #endregion

        #region [카메라 설정]

        private void BuildCameraSection()
        {
            AddLabel(_grpCamera, 20, 32, 70, "카메라수량");
            _cboCameraCount = AddComboEdit(_grpCamera, 95, 29, 100);
            _cboCameraCount.Properties.Items.AddRange(new object[] { "NONE", "1", "2", "3", "4" });

            var btnFolder = new SimpleButton
            {
                Location = new Point(20, 62),
                Size = new Size(110, 28),
                Text = "사진 저장 폴더"
            };
            btnFolder.Click += (s, e) => ChoosePhotoFolder();
            _grpCamera.Controls.Add(btnFolder);

            _txtPhotoFolder = AddTextEdit(_grpCamera, 140, 65, 310);
        }

        private void ChoosePhotoFolder()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(_txtPhotoFolder.Text) && System.IO.Directory.Exists(_txtPhotoFolder.Text))
                {
                    dialog.SelectedPath = _txtPhotoFolder.Text;
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _txtPhotoFolder.Text = dialog.SelectedPath.EndsWith("\\") ? dialog.SelectedPath : dialog.SelectedPath + "\\";
                }
            }
        }

        #endregion

        #region [IP 카메라 설정]

        private void BuildIpCameraSection()
        {
            _gridIpCamera = new DevExpress.XtraGrid.GridControl
            {
                Location = new Point(10, 28),
                Size = new Size(450, 260)
            };
            _gridViewIpCamera = new DevExpress.XtraGrid.Views.Grid.GridView { GridControl = _gridIpCamera };
            _gridIpCamera.MainView = _gridViewIpCamera;
            _gridIpCamera.ViewCollection.Add(_gridViewIpCamera);
            _grpIpCamera.Controls.Add(_gridIpCamera);

            ComnGridFunc.GridStyleBasicSetting(_gridViewIpCamera);
            _gridViewIpCamera.OptionsView.ShowGroupPanel = false;

            var modelItems = new RepositoryItemComboBox { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor };
            modelItems.Items.AddRange(new object[] { "SNB-5000A", "SNB-6004", "XNP-6320" });
            _gridIpCamera.RepositoryItems.Add(modelItems);

            var passwordItem = new RepositoryItemTextEdit { PasswordChar = '*' };
            _gridIpCamera.RepositoryItems.Add(passwordItem);

            AddGridColumn("No", "번호", 45).OptionsColumn.AllowEdit = false;
            AddGridColumn("Ip", "IP", 100);
            AddGridColumn("VnpPort", "VNP PORT", 70);
            AddGridColumn("HttpPort", "HTTP PORT", 70);
            AddGridColumn("UserId", "ID", 55);
            var passwordColumn = AddGridColumn("Password", "암호", 55);
            passwordColumn.ColumnEdit = passwordItem;
            var modelColumn = AddGridColumn("Model", "MODEL", 85);
            modelColumn.ColumnEdit = modelItems;

            _gridIpCamera.DataSource = _ipCameras;
        }

        private GridColumn AddGridColumn(string fieldName, string caption, int width)
        {
            var column = _gridViewIpCamera.Columns.AddVisible(fieldName, caption);
            column.Width = width;
            return column;
        }

        #endregion

        #region [Load / Save]

        private void LoadFromSettings(AppSettings settings)
        {
            _txtBizNo.Text = settings.BusinessNo;
            _txtCompanyName.Text = settings.CompanyName;
            _txtCeoName.Text = settings.CeoName;
            _txtManagerName.Text = settings.ManagerName;
            _txtAddress.Text = settings.Address;
            _txtBizType.Text = settings.BusinessType;
            _txtBizItem.Text = settings.BusinessItem;
            _txtPhone.Text = settings.Phone;
            _txtFax.Text = settings.Fax;

            _numVehicleThreshold.Value = settings.VehicleRecognitionThreshold;
            _numWeightDeviation.Value = settings.WeightJudgmentDeviation;
            _chkUseBroadcast.Checked = settings.UseBroadcast;
            _numStableSeconds.Value = settings.WeightStableSeconds;
            _chkCopySecondToFirst.Checked = settings.CopySecondToFirst;
            _chkMoveSecondToFirst.Checked = settings.MoveSecondToFirst;
            _chkEditFirstOnMain.Checked = settings.EditFirstOnMainScreen;
            _chkEditSecondOnMain.Checked = settings.EditSecondOnMainScreen;

            if (!string.IsNullOrEmpty(settings.InOutRule) && !_cboInOutRule.Properties.Items.Contains(settings.InOutRule))
            {
                _cboInOutRule.Properties.Items.Add(settings.InOutRule);
            }
            _cboInOutRule.Text = settings.InOutRule;

            _chkLoadLastOnFirst.Checked = settings.LoadLastDataOnFirstWeighing;
            _chkUseDispatch.Checked = settings.UseDispatch;
            _chkAutoLogin.Checked = settings.UseAutoLogin;
            _chkSaveLog.Checked = settings.SaveLogData;
            _numAdminAutoOffMinutes.Value = settings.AdminAutoOffMinutes;
            _txtClosingTime.Text = settings.ClosingTime;
            _numGridFontSize.Value = settings.MainGridFontSize;
            _txtWeightUnit.Text = settings.WeightUnit;
            _txtAmountUnit.Text = settings.AmountUnit;

            SelectApprovalColumnCount(settings.ApprovalColumnCount);

            if (!string.IsNullOrEmpty(settings.ReportPrinter) && !_cboReportPrinter.Properties.Items.Contains(settings.ReportPrinter))
            {
                _cboReportPrinter.Properties.Items.Add(settings.ReportPrinter);
            }
            _cboReportPrinter.Text = settings.ReportPrinter;

            _cboCameraCount.Text = settings.CameraCount <= 0 ? "NONE" : settings.CameraCount.ToString();
            _txtPhotoFolder.Text = settings.PhotoSaveFolder;

            _ipCameras.RaiseListChangedEvents = false;
            _ipCameras.Clear();
            foreach (var camera in settings.IpCameras)
            {
                _ipCameras.Add(camera);
            }
            _ipCameras.RaiseListChangedEvents = true;
            _ipCameras.ResetBindings();
        }

        private void Save()
        {
            var settings = new AppSettings
            {
                BusinessNo = _txtBizNo.Text,
                CompanyName = _txtCompanyName.Text,
                CeoName = _txtCeoName.Text,
                ManagerName = _txtManagerName.Text,
                Address = _txtAddress.Text,
                BusinessType = _txtBizType.Text,
                BusinessItem = _txtBizItem.Text,
                Phone = _txtPhone.Text,
                Fax = _txtFax.Text,

                VehicleRecognitionThreshold = (int)_numVehicleThreshold.Value,
                WeightJudgmentDeviation = (int)_numWeightDeviation.Value,
                UseBroadcast = _chkUseBroadcast.Checked,
                WeightStableSeconds = (int)_numStableSeconds.Value,
                CopySecondToFirst = _chkCopySecondToFirst.Checked,
                MoveSecondToFirst = _chkMoveSecondToFirst.Checked,
                EditFirstOnMainScreen = _chkEditFirstOnMain.Checked,
                EditSecondOnMainScreen = _chkEditSecondOnMain.Checked,
                InOutRule = _cboInOutRule.Text,
                LoadLastDataOnFirstWeighing = _chkLoadLastOnFirst.Checked,
                UseDispatch = _chkUseDispatch.Checked,
                UseAutoLogin = _chkAutoLogin.Checked,
                SaveLogData = _chkSaveLog.Checked,
                AdminAutoOffMinutes = (int)_numAdminAutoOffMinutes.Value,
                ClosingTime = _txtClosingTime.Text,
                MainGridFontSize = (int)_numGridFontSize.Value,
                WeightUnit = _txtWeightUnit.Text,
                AmountUnit = _txtAmountUnit.Text,

                ApprovalColumnCount = _approvalColumnCount,
                ReportPrinter = _cboReportPrinter.Text,

                CameraCount = _cboCameraCount.Text == "NONE" ? 0 : ParseIntOrZero(_cboCameraCount.Text),
                PhotoSaveFolder = _txtPhotoFolder.Text,

                IpCameras = new System.Collections.Generic.List<IpCameraSetting>(_ipCameras)
            };

            _repository.Save(settings);
            ComnFunc.gp_PrintMessage("저장되었습니다.", "시스템 설정", MessageType.알림);
        }

        private static int ParseIntOrZero(string text)
        {
            return int.TryParse(text, out var value) ? value : 0;
        }

        #endregion

        #region [공통 컨트롤 생성 헬퍼]

        private static LabelControl AddLabel(GroupControl group, int x, int y, int width, string text)
        {
            var label = new LabelControl
            {
                Location = new Point(x, y),
                Size = new Size(width, 16),
                Text = text
            };
            label.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            group.Controls.Add(label);
            return label;
        }

        private static TextEdit AddTextEdit(GroupControl group, int x, int y, int width)
        {
            var edit = new TextEdit { Location = new Point(x, y), Size = new Size(width, 20) };
            group.Controls.Add(edit);
            return edit;
        }

        private static SpinEdit AddSpinEdit(GroupControl group, int x, int y, int width, int min, int max)
        {
            var edit = new SpinEdit { Location = new Point(x, y), Size = new Size(width, 20) };
            edit.Properties.MinValue = min;
            edit.Properties.MaxValue = max;
            edit.Properties.Mask.EditMask = "N0";
            edit.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            edit.Properties.Mask.UseMaskAsDisplayFormat = true;
            group.Controls.Add(edit);
            return edit;
        }

        private static CheckEdit AddCheckEdit(GroupControl group, int x, int y, int width, string caption)
        {
            var check = new CheckEdit { Location = new Point(x, y), Size = new Size(width, 19) };
            check.Properties.Caption = caption;
            group.Controls.Add(check);
            return check;
        }

        private static ComboBoxEdit AddComboEdit(GroupControl group, int x, int y, int width)
        {
            var combo = new ComboBoxEdit { Location = new Point(x, y), Size = new Size(width, 20) };
            combo.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            group.Controls.Add(combo);
            return combo;
        }

        #endregion
    }
}
