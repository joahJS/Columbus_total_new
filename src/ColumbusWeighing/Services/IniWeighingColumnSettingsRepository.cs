using ColumbusWeighing.ComnLib;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// "계량 화면 설정" 값을 사용자 PC의 로컬 설정 파일(%AppData%\ColumbusWeighing\User.ini,
    /// IniAppSettingsRepository와 같은 파일)에 저장/조회한다. 섹션만 분리했다.
    /// </summary>
    public sealed class IniWeighingColumnSettingsRepository : IWeighingColumnSettingsRepository
    {
        private const string Section = "WEIGH_COLUMNS";

        public WeighingColumnSettings Load()
        {
            var defaults = new WeighingColumnSettings();

            try
            {
                return new WeighingColumnSettings
                {
                    ShowWeighSeq = GetBool(nameof(WeighingColumnSettings.ShowWeighSeq), defaults.ShowWeighSeq),
                    ShowFirstDateTime = GetBool(nameof(WeighingColumnSettings.ShowFirstDateTime), defaults.ShowFirstDateTime),
                    ShowSecondDateTime = GetBool(nameof(WeighingColumnSettings.ShowSecondDateTime), defaults.ShowSecondDateTime),
                    ShowOwnerCompany = GetBool(nameof(WeighingColumnSettings.ShowOwnerCompany), defaults.ShowOwnerCompany),
                    ShowProductName = GetBool(nameof(WeighingColumnSettings.ShowProductName), defaults.ShowProductName),
                    ShowCustomerName = GetBool(nameof(WeighingColumnSettings.ShowCustomerName), defaults.ShowCustomerName),
                    ShowDriverName = GetBool(nameof(WeighingColumnSettings.ShowDriverName), defaults.ShowDriverName),
                    ShowLossWeight = GetBool(nameof(WeighingColumnSettings.ShowLossWeight), defaults.ShowLossWeight),
                    ShowPriceInfo = GetBool(nameof(WeighingColumnSettings.ShowPriceInfo), defaults.ShowPriceInfo),
                    // ShowSpecificGravity/ShowPersonInCharge: 항목 자체를 숨겨서(WeighingColumnSettings.cs
                    // 참고) 더 이상 읽지 않는다.
                    ShowInOutType = GetBool(nameof(WeighingColumnSettings.ShowInOutType), defaults.ShowInOutType)
                };
            }
            catch (System.Exception)
            {
                return defaults;
            }
        }

        public void Save(WeighingColumnSettings settings)
        {
            try
            {
                SetValue(nameof(settings.ShowWeighSeq), settings.ShowWeighSeq);
                SetValue(nameof(settings.ShowFirstDateTime), settings.ShowFirstDateTime);
                SetValue(nameof(settings.ShowSecondDateTime), settings.ShowSecondDateTime);
                SetValue(nameof(settings.ShowOwnerCompany), settings.ShowOwnerCompany);
                SetValue(nameof(settings.ShowProductName), settings.ShowProductName);
                SetValue(nameof(settings.ShowCustomerName), settings.ShowCustomerName);
                SetValue(nameof(settings.ShowDriverName), settings.ShowDriverName);
                SetValue(nameof(settings.ShowLossWeight), settings.ShowLossWeight);
                SetValue(nameof(settings.ShowPriceInfo), settings.ShowPriceInfo);
                SetValue(nameof(settings.ShowInOutType), settings.ShowInOutType);
            }
            catch (System.Exception)
            {
                // 저장 실패는 조용히 무시한다(치명적이지 않음).
            }
        }

        private static bool GetBool(string key, bool fallback)
        {
            var value = IniHelper.GetValue(Section, key);
            return string.IsNullOrEmpty(value) ? fallback : value == bool.TrueString;
        }

        private static void SetValue(string key, bool value)
        {
            IniHelper.SetValue(Section, key, value.ToString());
        }
    }
}
