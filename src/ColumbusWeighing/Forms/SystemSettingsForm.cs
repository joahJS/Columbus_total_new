using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;
using ColumbusWeighing.Services;
using DevExpress.XtraEditors;

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

        private TextEdit _txtApproval1;
        private TextEdit _txtApproval2;
        private TextEdit _txtApproval3;
        private TextEdit _txtApproval4;
        private ComboBoxEdit _cboReportPrinter;

        private ComboBoxEdit _cboCameraCount;
        private TextEdit _txtPhotoFolder;

        private const int IpCameraCount = 4;
        private static readonly string[] IpCameraModels = { "SNB-5000A", "SNB-6004", "XNP-6320" };
        private readonly TextEdit[] _txtCameraIp = new TextEdit[IpCameraCount];
        private readonly SpinEdit[] _numCameraVnpPort = new SpinEdit[IpCameraCount];
        private readonly SpinEdit[] _numCameraHttpPort = new SpinEdit[IpCameraCount];
        private readonly TextEdit[] _txtCameraId = new TextEdit[IpCameraCount];
        private readonly TextEdit[] _txtCameraPassword = new TextEdit[IpCameraCount];
        private readonly ComboBoxEdit[] _cboCameraModel = new ComboBoxEdit[IpCameraCount];

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

        // 계량 설정의 라벨+입력란 줄을 모두 이 격자에 맞춰, 입력란 너비/가로 위치가 줄마다 어긋나지 않게 한다.
        private const int WeighingColALabelX = 20;
        private const int WeighingColBLabelX = 250;
        private const int WeighingLabelWidth = 150;
        private const int WeighingFieldWidth = 60;
        private const int WeighingColAFieldX = WeighingColALabelX + WeighingLabelWidth + 4;
        private const int WeighingColBFieldX = WeighingColBLabelX + WeighingLabelWidth + 4;

        private void BuildWeighingSection()
        {
            AddLabel(_grpWeighing, WeighingColALabelX, 30, WeighingLabelWidth, "차량인식기준");
            _numVehicleThreshold = AddSpinEdit(_grpWeighing, WeighingColAFieldX, 27, WeighingFieldWidth, 0, 100000);
            AddLabel(_grpWeighing, WeighingColBLabelX, 30, WeighingLabelWidth, "중량 판정 편차");
            _numWeightDeviation = AddSpinEdit(_grpWeighing, WeighingColBFieldX, 27, WeighingFieldWidth, 0, 100000);

            AddLabel(_grpWeighing, WeighingColALabelX, 58, WeighingLabelWidth, "중량안정판정시간(1~10초)");
            _numStableSeconds = AddSpinEdit(_grpWeighing, WeighingColAFieldX, 55, WeighingFieldWidth, 1, 10);
            AddLabel(_grpWeighing, WeighingColBLabelX, 58, WeighingLabelWidth, "관리자 자동오프(분)");
            _numAdminAutoOffMinutes = AddSpinEdit(_grpWeighing, WeighingColBFieldX, 55, WeighingFieldWidth, 0, 999);

            AddLabel(_grpWeighing, WeighingColALabelX, 86, WeighingLabelWidth, "마감 기준 시간");
            _txtClosingTime = AddTextEdit(_grpWeighing, WeighingColAFieldX, 83, WeighingFieldWidth);
            AddLabel(_grpWeighing, WeighingColBLabelX, 86, WeighingLabelWidth, "메인화면 그리드 폰트");
            _numGridFontSize = AddSpinEdit(_grpWeighing, WeighingColBFieldX, 83, WeighingFieldWidth, 6, 24);

            AddLabel(_grpWeighing, WeighingColALabelX, 114, WeighingLabelWidth, "중량 단위");
            _txtWeightUnit = AddTextEdit(_grpWeighing, WeighingColAFieldX, 111, WeighingFieldWidth);
            AddLabel(_grpWeighing, WeighingColBLabelX, 114, WeighingLabelWidth, "금액 단위");
            _txtAmountUnit = AddTextEdit(_grpWeighing, WeighingColBFieldX, 111, WeighingFieldWidth);

            AddLabel(_grpWeighing, WeighingColALabelX, 144, WeighingLabelWidth, "입출고 구분");
            _cboInOutRule = AddComboEdit(_grpWeighing, WeighingColAFieldX, 141, 464 - WeighingColAFieldX);
            _cboInOutRule.Properties.Items.AddRange(new object[]
            {
                "1차>2차 [입고], 2차>1차 [출고]",
                "1차>2차 [출고], 2차>1차 [입고]"
            });

            _chkUseBroadcast = AddCheckEdit(_grpWeighing, 20, 176, 210, "PC 안내방송 사용");
            _chkAutoLogin = AddCheckEdit(_grpWeighing, 250, 176, 200, "자동 로그인 사용");

            _chkSaveLog = AddCheckEdit(_grpWeighing, 20, 200, 210, "로그 데이터 저장");
            _chkUseDispatch = AddCheckEdit(_grpWeighing, 250, 200, 200, "배차 사용");

            _chkCopySecondToFirst = AddCheckEdit(_grpWeighing, 20, 224, 220, "2차 계량 자료 1차로 복사");
            _chkMoveSecondToFirst = AddCheckEdit(_grpWeighing, 250, 224, 210, "2차 계량 자료 1차로 이동");

            _chkEditFirstOnMain = AddCheckEdit(_grpWeighing, 20, 248, 220, "메인화면 1차 계량자료 수정");
            _chkEditSecondOnMain = AddCheckEdit(_grpWeighing, 250, 248, 210, "메인화면 2차계량자료 수정");

            _chkLoadLastOnFirst = AddCheckEdit(_grpWeighing, 20, 272, 430, "1차, 1회 계량시 최종 자료 읽어오기");
        }

        #endregion

        #region [인쇄 설정]

        private void BuildPrintSection()
        {
            var subLabel = new LabelControl
            {
                Location = new Point(20, 30),
                Size = new Size(150, 16),
                Text = "보고서 인쇄 설정",
                AutoSizeMode = LabelAutoSizeMode.None
            };
            subLabel.Appearance.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            subLabel.Appearance.Options.UseFont = true;
            _grpPrint.Controls.Add(subLabel);

            AddLabel(_grpPrint, 20, 56, 70, "결재란 설정");
            const int approvalWidth = 85;
            const int approvalGap = 8;
            var approvalX = 95;
            _txtApproval1 = AddTextEdit(_grpPrint, approvalX, 53, approvalWidth);
            approvalX += approvalWidth + approvalGap;
            _txtApproval2 = AddTextEdit(_grpPrint, approvalX, 53, approvalWidth);
            approvalX += approvalWidth + approvalGap;
            _txtApproval3 = AddTextEdit(_grpPrint, approvalX, 53, approvalWidth);
            approvalX += approvalWidth + approvalGap;
            _txtApproval4 = AddTextEdit(_grpPrint, approvalX, 53, approvalWidth);

            AddLabel(_grpPrint, 20, 88, 80, "보고서프린터");
            _cboReportPrinter = AddComboEdit(_grpPrint, 105, 85, 345);
            _cboReportPrinter.Properties.Items.Clear();
            foreach (string printerName in PrinterSettings.InstalledPrinters)
            {
                _cboReportPrinter.Properties.Items.Add(printerName);
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
            AddColumnHeader(_grpIpCamera, 48, 90, "IP");
            AddColumnHeader(_grpIpCamera, 142, 50, "VNP PORT");
            AddColumnHeader(_grpIpCamera, 196, 50, "HTTP PORT");
            AddColumnHeader(_grpIpCamera, 250, 50, "ID");
            AddColumnHeader(_grpIpCamera, 304, 50, "암호");
            AddColumnHeader(_grpIpCamera, 358, 90, "MODEL");

            for (var i = 0; i < IpCameraCount; i++)
            {
                var y = 52 + i * 28;

                var rowLabel = new LabelControl
                {
                    Location = new Point(12, y + 2),
                    Size = new Size(30, 16),
                    Text = string.Format("#{0}", i + 1),
                    AutoSizeMode = LabelAutoSizeMode.None
                };
                _grpIpCamera.Controls.Add(rowLabel);

                _txtCameraIp[i] = new TextEdit { Location = new Point(48, y), Size = new Size(90, 20) };
                _grpIpCamera.Controls.Add(_txtCameraIp[i]);

                _numCameraVnpPort[i] = new SpinEdit { Location = new Point(142, y), Size = new Size(50, 20) };
                SetPortRange(_numCameraVnpPort[i]);
                _grpIpCamera.Controls.Add(_numCameraVnpPort[i]);

                _numCameraHttpPort[i] = new SpinEdit { Location = new Point(196, y), Size = new Size(50, 20) };
                SetPortRange(_numCameraHttpPort[i]);
                _grpIpCamera.Controls.Add(_numCameraHttpPort[i]);

                _txtCameraId[i] = new TextEdit { Location = new Point(250, y), Size = new Size(50, 20) };
                _grpIpCamera.Controls.Add(_txtCameraId[i]);

                _txtCameraPassword[i] = new TextEdit { Location = new Point(304, y), Size = new Size(50, 20) };
                _txtCameraPassword[i].Properties.PasswordChar = '*';
                _grpIpCamera.Controls.Add(_txtCameraPassword[i]);

                _cboCameraModel[i] = AddComboEdit(_grpIpCamera, 358, y, 90);
                _cboCameraModel[i].Properties.Items.AddRange(IpCameraModels);
            }
        }

        private static void SetPortRange(SpinEdit edit)
        {
            edit.Properties.MinValue = 0;
            edit.Properties.MaxValue = 65535;
            edit.Properties.Mask.EditMask = "N0";
            edit.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            edit.Properties.Mask.UseMaskAsDisplayFormat = true;
        }

        private static void AddColumnHeader(GroupControl group, int x, int width, string text)
        {
            var label = new LabelControl
            {
                Location = new Point(x, 30),
                Size = new Size(width, 16),
                Text = text,
                AutoSizeMode = LabelAutoSizeMode.None
            };
            label.Appearance.Font = new Font("맑은 고딕", 8F, FontStyle.Bold);
            label.Appearance.Options.UseFont = true;
            label.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            group.Controls.Add(label);
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

            _txtApproval1.Text = settings.ApprovalTitle1;
            _txtApproval2.Text = settings.ApprovalTitle2;
            _txtApproval3.Text = settings.ApprovalTitle3;
            _txtApproval4.Text = settings.ApprovalTitle4;

            if (!string.IsNullOrEmpty(settings.ReportPrinter) && !_cboReportPrinter.Properties.Items.Contains(settings.ReportPrinter))
            {
                _cboReportPrinter.Properties.Items.Add(settings.ReportPrinter);
            }
            _cboReportPrinter.Text = settings.ReportPrinter;

            _cboCameraCount.Text = settings.CameraCount <= 0 ? "NONE" : settings.CameraCount.ToString();
            _txtPhotoFolder.Text = settings.PhotoSaveFolder;

            for (var i = 0; i < IpCameraCount; i++)
            {
                var camera = i < settings.IpCameras.Count ? settings.IpCameras[i] : new IpCameraSetting();

                _txtCameraIp[i].Text = camera.Ip;
                _numCameraVnpPort[i].Value = camera.VnpPort;
                _numCameraHttpPort[i].Value = camera.HttpPort;
                _txtCameraId[i].Text = camera.UserId;
                _txtCameraPassword[i].Text = camera.Password;

                if (!string.IsNullOrEmpty(camera.Model) && !_cboCameraModel[i].Properties.Items.Contains(camera.Model))
                {
                    _cboCameraModel[i].Properties.Items.Add(camera.Model);
                }
                _cboCameraModel[i].Text = camera.Model;
            }
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

                ApprovalTitle1 = _txtApproval1.Text,
                ApprovalTitle2 = _txtApproval2.Text,
                ApprovalTitle3 = _txtApproval3.Text,
                ApprovalTitle4 = _txtApproval4.Text,
                ReportPrinter = _cboReportPrinter.Text,

                CameraCount = _cboCameraCount.Text == "NONE" ? 0 : ParseIntOrZero(_cboCameraCount.Text),
                PhotoSaveFolder = _txtPhotoFolder.Text,

                IpCameras = BuildIpCameraList()
            };

            _repository.Save(settings);
            ComnFunc.gp_PrintMessage("저장되었습니다.", "시스템 설정", MessageType.알림);
        }

        private static int ParseIntOrZero(string text)
        {
            return int.TryParse(text, out var value) ? value : 0;
        }

        private List<IpCameraSetting> BuildIpCameraList()
        {
            var cameras = new List<IpCameraSetting>();
            for (var i = 0; i < IpCameraCount; i++)
            {
                cameras.Add(new IpCameraSetting
                {
                    No = i + 1,
                    Ip = _txtCameraIp[i].Text,
                    VnpPort = (int)_numCameraVnpPort[i].Value,
                    HttpPort = (int)_numCameraHttpPort[i].Value,
                    UserId = _txtCameraId[i].Text,
                    Password = _txtCameraPassword[i].Text,
                    Model = _cboCameraModel[i].Text
                });
            }

            return cameras;
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
            // LabelControl은 AutoSizeMode가 기본값(Default)이면 텍스트 크기에 맞춰 스스로 줄어들어
            // HAlignment.Far를 줘도 정렬할 여유 공간이 없어 항상 좌측처럼 보인다. None으로 고정해야
            // 위에서 지정한 Size(너비)를 실제로 채우고, 그 안에서 우측정렬이 눈에 보이게 적용된다.
            label.AutoSizeMode = LabelAutoSizeMode.None;
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
