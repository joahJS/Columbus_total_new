using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>"계량 화면 설정" 값의 조회/저장.</summary>
    public interface IWeighingColumnSettingsRepository
    {
        WeighingColumnSettings Load();

        void Save(WeighingColumnSettings settings);
    }
}
