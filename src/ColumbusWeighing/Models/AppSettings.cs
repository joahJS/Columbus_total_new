using System.Collections.Generic;

namespace ColumbusWeighing.Models
{
    /// <summary>
    /// "시스템 설정" 화면(사용자 설정/계량 설정/인쇄 설정/카메라 설정/IP 카메라 설정)에서 다루는 값 전체.
    /// 참고 화면(TS2020)과 동일한 항목 구성이다.
    /// </summary>
    public class AppSettings
    {
        // 사용자 설정
        public string BusinessNo { get; set; }
        public string CompanyName { get; set; }
        public string CeoName { get; set; }
        public string ManagerName { get; set; }
        public string Address { get; set; }
        public string BusinessType { get; set; }
        public string BusinessItem { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }

        // 계량 설정
        // 아래는 실계량 입력/현장 장비 제어용 항목이라 조회 전용인 이 프로그램에는 불필요해
        // 주석 처리했다(SystemSettingsForm.BuildWeighingSection 참고). InOutRule은 각 지점
        // SyncOrchestrator.Transform()에서 이미 고정 규칙으로 계산되고 있어 특히 그렇다.
        // public int VehicleRecognitionThreshold { get; set; }
        // public int WeightJudgmentDeviation { get; set; }
        // public bool UseBroadcast { get; set; }
        // public int WeightStableSeconds { get; set; }
        // public bool CopySecondToFirst { get; set; }
        // public bool MoveSecondToFirst { get; set; }
        // public bool EditFirstOnMainScreen { get; set; }
        // public bool EditSecondOnMainScreen { get; set; }
        // public string InOutRule { get; set; }
        // public bool LoadLastDataOnFirstWeighing { get; set; }
        // public bool UseDispatch { get; set; }
        public bool UseAutoLogin { get; set; }
        public bool SaveLogData { get; set; }
        public int AdminAutoOffMinutes { get; set; }
        public string ClosingTime { get; set; }
        public int MainGridFontSize { get; set; }
        public string WeightUnit { get; set; }
        public string AmountUnit { get; set; }

        // 인쇄 설정
        public string ApprovalTitle1 { get; set; }
        public string ApprovalTitle2 { get; set; }
        public string ApprovalTitle3 { get; set; }
        public string ApprovalTitle4 { get; set; }
        public string ReportPrinter { get; set; }

        // 카메라 설정 / IP 카메라 설정: 계근장 CCTV·차량 사진 캡처 장비 연동용. 이 프로그램은
        // 조회 전용이라 사진을 찍거나 표시하지 않아(WEIGH_RECORD 테이블에도 사진 컬럼이 없음)
        // 주석 처리했다.
        // public int CameraCount { get; set; }
        // public string PhotoSaveFolder { get; set; }
        // public List<IpCameraSetting> IpCameras { get; set; } = new List<IpCameraSetting>();
    }
}
