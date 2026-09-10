using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>시스템 설정 값의 조회/저장. 실제 배포 시에는 DB 연동 구현체로 교체한다.</summary>
    public interface IAppSettingsRepository
    {
        AppSettings Load();

        void Save(AppSettings settings);
    }
}
