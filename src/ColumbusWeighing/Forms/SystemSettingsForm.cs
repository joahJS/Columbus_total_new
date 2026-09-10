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

        private const int UserLabelX = 10;
        private const int UserLabelWidth = 75;
        private const int UserFieldX = UserLabelX + UserLabelWidth + 4;
        private const int UserFullFieldWidth = 420 - UserFieldX;
        private const int UserColBLabelX = 199;
        private const int UserColBLabelWidth = 50;
        private const int UserColBFieldX = UserColBLabelX + UserColBLabelWidth + 4;
        private const int UserColBFieldWidth = 420 - UserColBFieldX;

        private void BuildUserSection()
        {
            AddLabel(_grpUser, UserLabelX, 30, UserLabelWidth, "사업자등록번호");
            _txtBizNo = AddTextEdit(_grpUser, UserFieldX, 27, UserFullFieldWidth);

            AddLabel(_grpUser, UserLabelX, 58, UserLabelWidth, "회사명");
            _txtCompanyName = AddTextEdit(_grpUser, UserFieldX, 55, UserFullFieldWidth);

            AddLabel(_grpUser, UserLabelX, 86, UserLabelWidth, "대표자");
            _txtCeoName = AddTextEdit(_grpUser, UserFieldX, 83, 100);
            AddLabel(_grpUser, UserColBLabelX, 86, UserColBLabelWidth, "담당자");
            _txtManagerName = AddTextEdit(_grpUser, UserColBFieldX, 83, UserColBFieldWidth);

            AddLabel(_grpUser, UserLabelX, 114, UserLabelWidth, "주소");
            _txtAddress = AddTextEdit(_grpUser, UserFieldX, 111, UserFullFieldWidth);

            AddLabel(_grpUser, UserLabelX, 142, UserLabelWidth, "업태");
            _txtBizType = AddTextEdit(_grpUser, UserFieldX, 139, UserFullFieldWidth);

            AddLabel(_grpUser, UserLabelX, 170, UserLabelWidth, "종목");
            _txtBizItem = AddTextEdit(_grpUser, UserFieldX, 167, UserFullFieldWidth);

            AddLabel(_grpUser, UserLabelX, 198, UserLabelWidth, "전화번호");
            _txtPhone = AddTextEdit(_grpUser, UserFieldX, 195, 100);
            AddLabel(_grpUser, UserColBLabelX, 198, UserColBLabelWidth, "Fax");
            _txtFax = AddTextEdit(_grpUser, UserColBFieldX, 195, UserColBFieldWidth);
        }

        #endregion

        #region [계량 설정]

        // 계량 설정의 라벨+입력란 줄을 모두 이 격자에 맞춰, 입력란 너비/가로 위치가 줄마다 어긋나지 않게 한다.
        // A열은 체크박스들과 좌측 줄을 맞추기 위해 좌측정렬(AddLabelLeft), B열은 우측정렬(AddLabel)을 쓴다.
        private const int WeighingColALabelX = 20;
        private const int WeighingColALabelWidth = 120;
        private const int WeighingColAFieldX = WeighingColALabelX + WeighingColALabelWidth + 4;
        private const int WeighingColBLabelX = 209;
        private const int WeighingColBLabelWidth = 150;
        private const int WeighingColBFieldX = WeighingColBLabelX + WeighingColBLabelWidth + 4;
        private const int WeighingFieldWidth = 55;
        private const int WeighingCheckWidth = 185;
        private const int WeighingContentRight = 420;

        private void BuildWeighingSection()
        {
            // 참고 화면(TS2020)과 동일한 순서: 차량인식기준/편차 → PC안내방송+안정판정시간 →
            // 2차자료 복사/이동 → 메인화면 수정권한 → 입출고구분 → 최종자료읽기 → 배차사용 →
            // 자동로그인/로그저장 → 관리자자동오프 → 마감시간/그리드폰트 → 중량/금액 단위.
            AddLabelLeft(_grpWeighing, WeighingColALabelX, 30, WeighingColALabelWidth, "차량인식기준");
            _numVehicleThreshold = AddSpinEdit(_grpWeighing, WeighingColAFieldX, 27, WeighingFieldWidth, 0, 100000);
            AddLabel(_grpWeighing, WeighingColBLabelX, 30, WeighingColBLabelWidth, "중량 판정 편차");
            _numWeightDeviation = AddSpinEdit(_grpWeighing, WeighingColBFieldX, 27, WeighingFieldWidth, 0, 100000);

            _chkUseBroadcast = AddCheckEdit(_grpWeighing, WeighingColALabelX, 60, WeighingCheckWidth, "PC 안내방송 사용");
            AddLabel(_grpWeighing, WeighingColBLabelX, 58, WeighingColBLabelWidth, "중량안정판정시간(1~10초)");
            _numStableSeconds = AddSpinEdit(_grpWeighing, WeighingColBFieldX, 55, WeighingFieldWidth, 1, 10);

            _chkCopySecondToFirst = AddCheckEdit(_grpWeighing, WeighingColALabelX, 84, WeighingCheckWidth, "2차 계량 자료 1차로 복사");
            _chkMoveSecondToFirst = AddCheckEdit(_grpWeighing, WeighingColBLabelX, 84, WeighingCheckWidth, "2차 계량 자료 1차로 이동");

            _chkEditFirstOnMain = AddCheckEdit(_grpWeighing, WeighingColALabelX, 108, WeighingCheckWidth, "메인화면 1차 계량자료 수정");
            _chkEditSecondOnMain = AddCheckEdit(_grpWeighing, WeighingColBLabelX, 108, WeighingCheckWidth, "메인화면 2차계량자료 수정");

            // 입출고 구분은 라벨을 짧게 두고 선택란을 줄 끝까지 가득 채운다.
            const int inOutLabelWidth = 70;
            const int inOutFieldX = WeighingColALabelX + inOutLabelWidth + 4;
            AddLabel(_grpWeighing, WeighingColALabelX, 138, inOutLabelWidth, "입출고 구분");
            _cboInOutRule = AddComboEdit(_grpWeighing, inOutFieldX, 135, WeighingContentRight - inOutFieldX);
            _cboInOutRule.Properties.Items.AddRange(new object[]
            {
                "1차>2차 [입고], 2차>1차 [출고]",
                "1차>2차 [출고], 2차>1차 [입고]"
            });

            _chkLoadLastOnFirst = AddCheckEdit(_grpWeighing, WeighingColALabelX, 162, WeighingContentRight - WeighingColALabelX, "1차, 1회 계량시 최종 자료 읽어오기");

            _chkUseDispatch = AddCheckEdit(_grpWeighing, WeighingColALabelX, 186, WeighingCheckWidth, "배차 사용");

            _chkAutoLogin = AddCheckEdit(_grpWeighing, WeighingColALabelX, 210, WeighingCheckWidth, "자동 로그인 사용");
            _chkSaveLog = AddCheckEdit(_grpWeighing, WeighingColBLabelX, 210, WeighingCheckWidth, "로그 데이터 저장");

            AddLabelLeft(_grpWeighing, WeighingColALabelX, 240, WeighingColALabelWidth, "관리자 자동오프(분)");
            _numAdminAutoOffMinutes = AddSpinEdit(_grpWeighing, WeighingColAFieldX, 237, WeighingFieldWidth, 0, 999);

            AddLabelLeft(_grpWeighing, WeighingColALabelX, 268, WeighingColALabelWidth, "마감 기준 시간");
            _txtClosingTime = AddTextEdit(_grpWeighing, WeighingColAFieldX, 265, WeighingFieldWidth);
            AddLabel(_grpWeighing, WeighingColBLabelX, 268, WeighingColBLabelWidth, "메인화면 그리드 폰트");
            _numGridFontSize = AddSpinEdit(_grpWeighing, WeighingColBFieldX, 265, WeighingFieldWidth, 6, 24);

            AddLabelLeft(_grpWeighing, WeighingColALabelX, 296, WeighingColALabelWidth, "중량 단위");
            _txtWeightUnit = AddTextEdit(_grpWeighing, WeighingColAFieldX, 293, WeighingFieldWidth);
            AddLabel(_grpWeighing, WeighingColBLabelX, 296, WeighingColBLabelWidth, "금액 단위");
            _txtAmountUnit = AddTextEdit(_grpWeighing, WeighingColBFieldX, 293, WeighingFieldWidth);
        }

        #endregion

        #region [인쇄 설정]

        private void BuildPrintSection()
        {
            var subLabel = new LabelControl
            {
                Location = new Point(10, 30),
                Size = new Size(150, 16),
                Text = "보고서 인쇄 설정",
                AutoSizeMode = LabelAutoSizeMode.None
            };
            subLabel.Appearance.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            subLabel.Appearance.Options.UseFont = true;
            _grpPrint.Controls.Add(subLabel);

            // 결재란 입력란 4개의 좌측 끝(PrintFieldStartX)과 마지막 칸의 우측 끝(PrintFieldEndX)을
            // 보고서프린터 콤보와 정확히 맞춘다.
            const int printFieldStartX = 100;
            const int printFieldEndX = 484;
            const int approvalGap = 8;
            const int approvalWidth = (printFieldEndX - printFieldStartX - approvalGap * 3) / 4;

            AddLabel(_grpPrint, 10, 56, 85, "결재란 설정");
            var approvalX = printFieldStartX;
            _txtApproval1 = AddTextEdit(_grpPrint, approvalX, 53, approvalWidth);
            approvalX += approvalWidth + approvalGap;
            _txtApproval2 = AddTextEdit(_grpPrint, approvalX, 53, approvalWidth);
            approvalX += approvalWidth + approvalGap;
            _txtApproval3 = AddTextEdit(_grpPrint, approvalX, 53, approvalWidth);
            approvalX += approvalWidth + approvalGap;
            _txtApproval4 = AddTextEdit(_grpPrint, approvalX, 53, printFieldEndX - approvalX);

            AddLabel(_grpPrint, 10, 88, 85, "보고서프린터");
            _cboReportPrinter = AddComboEdit(_grpPrint, printFieldStartX, 85, printFieldEndX - printFieldStartX);
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
            AddLabel(_grpCamera, 10, 32, 70, "카메라수량");
            _cboCameraCount = AddComboEdit(_grpCamera, 84, 29, 110);
            _cboCameraCount.Properties.Items.AddRange(new object[] { "NONE", "1", "2", "3", "4" });

            var btnFolder = new SimpleButton
            {
                Location = new Point(10, 62),
                Size = new Size(110, 28),
                Text = "사진 저장 폴더"
            };
            btnFolder.Click += (s, e) => ChoosePhotoFolder();
            _grpCamera.Controls.Add(btnFolder);

            _txtPhotoFolder = AddTextEdit(_grpCamera, 130, 65, 385);
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

        // 좌측 여백을 줄이고(8) VNP/HTTP PORT 칸을 넓혀 값이 잘리지 않게 한 열 배치.
        private const int IpCamRowLabelX = 8;
        private const int IpCamIpX = 36;
        private const int IpCamIpWidth = 95;
        private const int IpCamVnpX = 135;
        private const int IpCamVnpWidth = 85;
        private const int IpCamHttpX = 224;
        private const int IpCamHttpWidth = 70;
        private const int IpCamIdX = 298;
        private const int IpCamIdWidth = 55;
        private const int IpCamPasswordX = 357;
        private const int IpCamPasswordWidth = 55;
        private const int IpCamModelX = 416;
        private const int IpCamModelWidth = 90;

        private void BuildIpCameraSection()
        {
            AddColumnHeader(_grpIpCamera, IpCamIpX, IpCamIpWidth, "IP");
            AddColumnHeader(_grpIpCamera, IpCamVnpX, IpCamVnpWidth, "VNP PORT");
            AddColumnHeader(_grpIpCamera, IpCamHttpX, IpCamHttpWidth, "HTTP PORT");
            AddColumnHeader(_grpIpCamera, IpCamIdX, IpCamIdWidth, "ID");
            AddColumnHeader(_grpIpCamera, IpCamPasswordX, IpCamPasswordWidth, "암호");
            AddColumnHeader(_grpIpCamera, IpCamModelX, IpCamModelWidth, "MODEL");

            for (var i = 0; i < IpCameraCount; i++)
            {
                var y = 52 + i * 28;

                var rowLabel = new LabelControl
                {
                    Location = new Point(IpCamRowLabelX, y + 2),
                    Size = new Size(26, 16),
                    Text = string.Format("#{0}", i + 1),
                    AutoSizeMode = LabelAutoSizeMode.None
                };
                _grpIpCamera.Controls.Add(rowLabel);

                _txtCameraIp[i] = new TextEdit { Location = new Point(IpCamIpX, y), Size = new Size(IpCamIpWidth, 20) };
                _grpIpCamera.Controls.Add(_txtCameraIp[i]);

                _numCameraVnpPort[i] = new SpinEdit { Location = new Point(IpCamVnpX, y), Size = new Size(IpCamVnpWidth, 20) };
                SetPortRange(_numCameraVnpPort[i]);
                _grpIpCamera.Controls.Add(_numCameraVnpPort[i]);

                _numCameraHttpPort[i] = new SpinEdit { Location = new Point(IpCamHttpX, y), Size = new Size(IpCamHttpWidth, 20) };
                SetPortRange(_numCameraHttpPort[i]);
                _grpIpCamera.Controls.Add(_numCameraHttpPort[i]);

                _txtCameraId[i] = new TextEdit { Location = new Point(IpCamIdX, y), Size = new Size(IpCamIdWidth, 20) };
                _grpIpCamera.Controls.Add(_txtCameraId[i]);

                // 암호도 다른 입력란과 동일하게 평문으로 보이도록 마스킹하지 않는다.
                _txtCameraPassword[i] = new TextEdit { Location = new Point(IpCamPasswordX, y), Size = new Size(IpCamPasswordWidth, 20) };
                _grpIpCamera.Controls.Add(_txtCameraPassword[i]);

                _cboCameraModel[i] = AddComboEdit(_grpIpCamera, IpCamModelX, y, IpCamModelWidth);
                _cboCameraModel[i].Properties.Items.AddRange(IpCameraModels);
            }
        }

        private static void SetPortRange(SpinEdit edit)
        {
            edit.Properties.MinValue = 0;
            edit.Properties.MaxValue = 65535;
            // "N0"는 천단위 구분 콤마가 붙어 좁은 칸에서 잘려 보이므로, 콤마 없는 고정소수점 마스크를 쓴다.
            edit.Properties.Mask.EditMask = "f0";
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

        /// <summary>PC안내방송 사용 등 체크박스들과 좌측 세로줄을 맞추기 위한 좌측정렬 라벨.</summary>
        private static LabelControl AddLabelLeft(GroupControl group, int x, int y, int width, string text)
        {
            var label = new LabelControl
            {
                Location = new Point(x, y),
                Size = new Size(width, 16),
                Text = text,
                AutoSizeMode = LabelAutoSizeMode.None
            };
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
