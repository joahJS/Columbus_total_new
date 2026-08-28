using System;
using ColumbusSync.BranchBC.Config;
using ColumbusSync.BranchBC.Hub;
using ColumbusSync.BranchBC.Logging;
using ColumbusSync.BranchBC.Source;

namespace ColumbusSync.BranchBC
{
    /// <summary>
    /// 한 번의 동기화 배치(= 10분마다 한 번)를 수행한다.
    /// "지점별 데이터 차이점 정리" 문서의 규칙을 여기서 실제로 적용한다:
    ///   - 완료 여부는 SecondWeight 존재 여부로 재계산 (원본 WEIGH_STS를 그대로 믿지 않음)
    ///   - 입/출고 구분은 1차/2차 중량 크기 비교로 재계산 (원본 INOUT을 그대로 믿지 않음)
    ///   - 원본 키(SENO)는 SOURCE_KEY 컬럼에 그대로 보존
    /// A지점(ColumbusSync.BranchA)과 같은 규칙을 그대로 적용해 지점 간 데이터를 일관되게 만든다.
    /// </summary>
    public class SyncOrchestrator
    {
        private readonly MdbSourceReader _source;
        private readonly HubWriter _hub;

        public SyncOrchestrator(MdbSourceReader source, HubWriter hub)
        {
            _source = source;
            _hub = hub;
        }

        public void RunOnce()
        {
            var startedAt = DateTime.Now;
            int upserted = 0;

            try
            {
                FileLogger.Info("동기화 시작");

                foreach (var customer in _source.GetCustomers())
                {
                    _hub.UpsertCustomer(customer);
                    upserted++;
                }

                foreach (var vehicle in _source.GetVehicles())
                {
                    _hub.UpsertVehicle(vehicle);
                    upserted++;
                }

                foreach (var product in _source.GetProducts())
                {
                    _hub.UpsertProduct(product);
                    upserted++;
                }

                // 계근 데이터는 최근 N일치를 매번 다시 훑는다. 검수/수정이 늦게 들어온 건도
                // 놓치지 않으려면 그 지연 가능성만큼 기간을 넉넉히 잡아야 한다.
                // App.config의 WeighSyncLookbackDays로 조정한다(기본 30일).
                var toDate = DateTime.Today;
                var fromDate = toDate.AddDays(-SyncSettings.WeighSyncLookbackDays);

                foreach (var raw in _source.GetWeighRecords(fromDate, toDate))
                {
                    _hub.UpsertWeighRecord(Transform(raw));
                    upserted++;
                }

                var finishedAt = DateTime.Now;
                _hub.WriteSyncLog(startedAt, finishedAt, success: true, inserted: upserted, updated: 0, errorMessage: null);
                FileLogger.Info(string.Format("동기화 완료: {0}건 처리, {1:F1}초 소요", upserted, (finishedAt - startedAt).TotalSeconds));
            }
            catch (Exception ex)
            {
                FileLogger.Error("동기화 실패", ex);

                try
                {
                    _hub.WriteSyncLog(startedAt, DateTime.Now, success: false, inserted: upserted, updated: 0, errorMessage: ex.ToString());
                }
                catch (Exception logEx)
                {
                    // 허브 DB 자체가 응답이 없는 상황일 수 있으므로 로그 기록 실패는 파일 로그로만 남긴다.
                    FileLogger.Error("SYNC_LOG 기록 실패", logEx);
                }
            }
        }

        /// <summary>mdb 원본 1건을 통합 스키마 규칙에 맞게 변환한다.</summary>
        private static WeighRecordForHub Transform(RawWeighRow raw)
        {
            var isCompleted = raw.SecondWeight.HasValue && raw.SecondWeight.Value != 0;

            string inOutType = null;
            if (raw.FirstWeight.HasValue && raw.SecondWeight.HasValue && raw.FirstWeight.Value != 0 && raw.SecondWeight.Value != 0)
            {
                // A지점(SA015F00.cs CalculateWeight())과 동일한 규칙: 1차 >= 2차면 입고, 아니면 출고.
                inOutType = raw.FirstWeight.Value >= raw.SecondWeight.Value ? "I" : "O";
            }

            decimal? netWeight = null;
            if (raw.FirstWeight.HasValue && raw.SecondWeight.HasValue)
            {
                netWeight = Math.Abs(raw.FirstWeight.Value - raw.SecondWeight.Value);
            }

            int? weighSeq;
            int seqParsed;
            weighSeq = int.TryParse(raw.Bunho, out seqParsed) ? seqParsed : (int?)null;

            return new WeighRecordForHub
            {
                SourceKey = raw.Seno,
                WeighDate = raw.WeighDate.Date,
                WeighSeq = weighSeq,
                VehicleNo = raw.CarNo,
                CustomerSourceCode = raw.CustoNo,
                CustomerName = raw.CustoNm,
                ProductSourceCode = raw.PumNo,
                ProductName = raw.PumNm,
                FirstDateTime = raw.FirstDateTime,
                FirstWeight = raw.FirstWeight,
                SecondDateTime = raw.SecondDateTime,
                SecondWeight = raw.SecondWeight,
                NetWeight = netWeight,
                LossWeight = raw.LossWeight,
                UnitPrice = raw.UnitPrice,
                InOutType = inOutType,
                IsCompleted = isCompleted,
                WeigherName = raw.WeigherName,
                Remark = raw.Remark,
                SourceRawStatus = raw.WeighStatus,
            };
        }
    }
}
