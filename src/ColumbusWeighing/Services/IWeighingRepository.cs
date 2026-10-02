using System;
using System.ComponentModel;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 계근 기록 저장소. 계량 자체(1차/2차 중량 등)는 각 지점이 지금 쓰는 프로그램에서 입력하고
    /// 동기화로만 채워지므로 이 프로그램은 기본적으로 조회 전용이다. 유일한 예외가 업체중량
    /// (VendorWeight)으로, 동기화 대상이 아니라 이 프로그램에서 직접 입력/저장한다.
    /// 실제 배포 시에는 통합 허브 DB 연동 구현체(예: SqlWeighingRepository)로 교체한다.
    /// </summary>
    public interface IWeighingRepository
    {
        BindingList<WeighingRecord> Records { get; }

        /// <summary>지정한 기간의 계근 기록을 다시 조회해 Records를 갱신한다.</summary>
        void Refresh(DateTime fromDate, DateTime toDate);

        /// <summary>업체중량(수동 입력값)만 저장한다. 동기화가 채우는 값이 아니라 이 프로그램에서
        /// 직접 관리하는 예외적인 쓰기 동작이다(2차계량 화면의 "업체중량" 컬럼 편집).</summary>
        void UpdateVendorWeight(int weighId, decimal? vendorWeight);
    }
}
