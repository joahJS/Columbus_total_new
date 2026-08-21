using System.Collections.Generic;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 메모리 기반 시스템 설정 저장소. 프로그램 시작 시 참고 화면(TS2020)과 동일한 값으로 적재한다.
    /// </summary>
    public sealed class InMemoryAppSettingsRepository : IAppSettingsRepository
    {
        private AppSettings _current;

        public InMemoryAppSettingsRepository()
        {
            _current = CreateDefault();
        }

        public AppSettings Load()
        {
            return _current;
        }

        public void Save(AppSettings settings)
        {
            _current = settings;
        }

        private static AppSettings CreateDefault()
        {
            return new AppSettings
            {
                BusinessNo = "428-81-01467",
                CompanyName = "콜럼버스 주식회사",
                CeoName = "이재현",
                ManagerName = "박재욱",
                Address = "부산광역시 강서구 녹산산업중로 426",
                BusinessType = "상기와 같이 계량하였음을 증명함.",
                BusinessItem = "계 량 증 명 서",
                Phone = "051-966-1472",
                Fax = "051-966-1473",

                VehicleRecognitionThreshold = 500,
                WeightJudgmentDeviation = 0,
                UseBroadcast = false,
                WeightStableSeconds = 2,
                CopySecondToFirst = false,
                MoveSecondToFirst = false,
                EditFirstOnMainScreen = false,
                EditSecondOnMainScreen = false,
                InOutRule = "1차>2차 [입고], 2차>1차 [출고]",
                LoadLastDataOnFirstWeighing = false,
                UseDispatch = false,
                UseAutoLogin = false,
                SaveLogData = false,
                AdminAutoOffMinutes = 10,
                ClosingTime = "00:00",
                MainGridFontSize = 10,
                WeightUnit = "kg",
                AmountUnit = "원",

                ApprovalColumnCount = 1,
                ReportPrinter = string.Empty,

                CameraCount = 0,
                PhotoSaveFolder = @"C:\ColumbusWeighing\IMAGE\",

                IpCameras = new List<IpCameraSetting>
                {
                    new IpCameraSetting { No = 1, Ip = "192.168.0.150", VnpPort = 4520, HttpPort = 80, UserId = "admin", Password = "4321", Model = "SNB-5000A" },
                    new IpCameraSetting { No = 2, Ip = "192.168.0.151", VnpPort = 4520, HttpPort = 80, UserId = "admin", Password = "4321", Model = "SNB-5000A" },
                    new IpCameraSetting { No = 3, Ip = "192.168.0.152", VnpPort = 4520, HttpPort = 80, UserId = "admin", Password = "4321", Model = "SNB-5000A" },
                    new IpCameraSetting { No = 4, Ip = "192.168.0.153", VnpPort = 4520, HttpPort = 80, UserId = "admin", Password = "4321", Model = "SNB-5000A" }
                }
            };
        }
    }
}
