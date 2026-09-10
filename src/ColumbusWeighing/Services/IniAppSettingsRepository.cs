using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 시스템 설정 값을 사용자 PC의 로컬 설정 파일(%AppData%\ColumbusWeighing\User.ini, IniHelper와
    /// 동일 파일)에 저장/조회한다. 로그인 화면의 "접속정보 기억하기"와 같은 파일을 쓰되 섹션을
    /// 분리했다. 프로세스를 껐다 켜도 값이 유지되어야 자동 로그인/그리드 폰트/관리자 자동오프 같은
    /// 설정이 실제로 의미가 있기 때문에, 이전의 메모리 전용(InMemoryAppSettingsRepository) 구현을
    /// 대체했다. 실제 DB 연동 시에는 이 구현체를 그대로 교체하면 된다.
    /// </summary>
    public sealed class IniAppSettingsRepository : IAppSettingsRepository
    {
        private const string Section = "SETTINGS";

        public AppSettings Load()
        {
            var defaults = CreateDefault();

            try
            {
                return new AppSettings
                {
                    BusinessNo = GetString("BusinessNo", defaults.BusinessNo),
                    CompanyName = GetString("CompanyName", defaults.CompanyName),
                    CeoName = GetString("CeoName", defaults.CeoName),
                    ManagerName = GetString("ManagerName", defaults.ManagerName),
                    Address = GetString("Address", defaults.Address),
                    BusinessType = GetString("BusinessType", defaults.BusinessType),
                    BusinessItem = GetString("BusinessItem", defaults.BusinessItem),
                    Phone = GetString("Phone", defaults.Phone),
                    Fax = GetString("Fax", defaults.Fax),

                    UseAutoLogin = GetBool("UseAutoLogin", defaults.UseAutoLogin),
                    SaveLogData = GetBool("SaveLogData", defaults.SaveLogData),
                    AdminAutoOffMinutes = GetInt("AdminAutoOffMinutes", defaults.AdminAutoOffMinutes),
                    ClosingTime = GetString("ClosingTime", defaults.ClosingTime),
                    MainGridFontSize = GetInt("MainGridFontSize", defaults.MainGridFontSize),
                    WeightUnit = GetString("WeightUnit", defaults.WeightUnit),
                    AmountUnit = GetString("AmountUnit", defaults.AmountUnit),

                    ApprovalTitle1 = GetString("ApprovalTitle1", defaults.ApprovalTitle1),
                    ApprovalTitle2 = GetString("ApprovalTitle2", defaults.ApprovalTitle2),
                    ApprovalTitle3 = GetString("ApprovalTitle3", defaults.ApprovalTitle3),
                    ApprovalTitle4 = GetString("ApprovalTitle4", defaults.ApprovalTitle4),
                    ReportPrinter = GetString("ReportPrinter", defaults.ReportPrinter)
                };
            }
            catch (System.Exception)
            {
                // 로컬 설정 파일을 읽지 못해도(권한 문제 등) 프로그램 시작 자체가 막히면 안 되므로
                // 기본값으로 대체한다.
                return defaults;
            }
        }

        public void Save(AppSettings settings)
        {
            try
            {
                SetValue("BusinessNo", settings.BusinessNo);
                SetValue("CompanyName", settings.CompanyName);
                SetValue("CeoName", settings.CeoName);
                SetValue("ManagerName", settings.ManagerName);
                SetValue("Address", settings.Address);
                SetValue("BusinessType", settings.BusinessType);
                SetValue("BusinessItem", settings.BusinessItem);
                SetValue("Phone", settings.Phone);
                SetValue("Fax", settings.Fax);

                SetValue("UseAutoLogin", settings.UseAutoLogin);
                SetValue("SaveLogData", settings.SaveLogData);
                SetValue("AdminAutoOffMinutes", settings.AdminAutoOffMinutes);
                SetValue("ClosingTime", settings.ClosingTime);
                SetValue("MainGridFontSize", settings.MainGridFontSize);
                SetValue("WeightUnit", settings.WeightUnit);
                SetValue("AmountUnit", settings.AmountUnit);

                SetValue("ApprovalTitle1", settings.ApprovalTitle1);
                SetValue("ApprovalTitle2", settings.ApprovalTitle2);
                SetValue("ApprovalTitle3", settings.ApprovalTitle3);
                SetValue("ApprovalTitle4", settings.ApprovalTitle4);
                SetValue("ReportPrinter", settings.ReportPrinter);
            }
            catch (System.Exception)
            {
                // 저장 실패는 화면에서 "저장되었습니다" 메세지 이후 조용히 무시한다(치명적이지 않음).
            }
        }

        private static string GetString(string key, string fallback)
        {
            var value = IniHelper.GetValue(Section, key);
            return string.IsNullOrEmpty(value) ? fallback : value;
        }

        private static bool GetBool(string key, bool fallback)
        {
            var value = IniHelper.GetValue(Section, key);
            return string.IsNullOrEmpty(value) ? fallback : value == bool.TrueString;
        }

        private static int GetInt(string key, int fallback)
        {
            var value = IniHelper.GetValue(Section, key);
            return !string.IsNullOrEmpty(value) && int.TryParse(value, out var parsed) ? parsed : fallback;
        }

        private static void SetValue(string key, string value)
        {
            IniHelper.SetValue(Section, key, value ?? string.Empty);
        }

        private static void SetValue(string key, bool value)
        {
            IniHelper.SetValue(Section, key, value.ToString());
        }

        private static void SetValue(string key, int value)
        {
            IniHelper.SetValue(Section, key, value.ToString());
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

                UseAutoLogin = false,
                SaveLogData = false,
                AdminAutoOffMinutes = 10,
                ClosingTime = "00:00",
                MainGridFontSize = 9,
                WeightUnit = "kg",
                AmountUnit = "원",

                ApprovalTitle1 = "결재4",
                ApprovalTitle2 = "결재3",
                ApprovalTitle3 = "결재2",
                ApprovalTitle4 = "결재1",
                ReportPrinter = string.Empty
            };
        }
    }
}
