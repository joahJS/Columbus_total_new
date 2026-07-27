using System;
using System.ComponentModel;
using ColumbusWeighing.Models;

namespace ColumbusWeighing.Services
{
    /// <summary>
    /// 계근 기록 저장소. 실제 배포 시에는 DB(MSSQL 등) 연동 구현체로 교체한다.
    /// 화면(GridControl)은 Records 컬렉션에 직접 바인딩되어 추가/변경이 즉시 반영된다.
    /// </summary>
    public interface IWeighingRepository
    {
        BindingList<WeighingRecord> Records { get; }

        /// <summary>1차 계량 등록. 아직 2차 계량 전 상태(대기)로 생성된다.</summary>
        WeighingRecord AddFirstWeighing(
            string vehicleNo,
            string customerName,
            string productName,
            InOutType inOutType,
            decimal firstWeight,
            string weigherName);

        /// <summary>선택된 1차 계량 건에 대해 2차 계량을 완료 처리한다.</summary>
        void CompleteSecondWeighing(WeighingRecord record, decimal secondWeight, string weigherName);

        /// <summary>1차/2차를 한 번에 처리하는 1회 계량(예: 정미중량이 이미 확정된 경우).</summary>
        WeighingRecord AddSingleWeighing(
            string vehicleNo,
            string customerName,
            string productName,
            InOutType inOutType,
            decimal firstWeight,
            decimal secondWeight,
            string weigherName);

        int GetNextSeq(DateTime date);
    }
}
