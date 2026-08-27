using System;
using System.ComponentModel;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 계근 기록 저장소. 이 프로그램은 조회/집계 전용이며(계량 입력은 각 지점이 지금 쓰는
    /// 프로그램에서 그대로 처리), 통합 허브 DB(COLUMBUS_WEIGH_HUB)에서 값을 읽어오기만 한다.
    /// 실제 배포 시에는 통합 허브 DB 연동 구현체(예: SqlWeighingRepository)로 교체한다.
    /// </summary>
    public interface IWeighingRepository
    {
        BindingList<WeighingRecord> Records { get; }

        /// <summary>지정한 기간의 계근 기록을 다시 조회해 Records를 갱신한다.</summary>
        void Refresh(DateTime fromDate, DateTime toDate);
    }
}
