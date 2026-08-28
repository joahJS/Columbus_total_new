using System;
using ColumbusSync.BranchA.Config;
using ColumbusSync.BranchA.Hub;
using ColumbusSync.BranchA.Logging;
using ColumbusSync.BranchA.Source;

namespace ColumbusSync.BranchA
{
    /// <summary>
    /// 한 번의 동기화 배치(= 10분마다 한 번)를 수행한다.
    /// "지점별 데이터 차이점 정리" 문서의 규칙(1~3번)을 여기서 실제로 적용한다:
    ///   - 완료 여부는 SecondWeight 존재 여부로 재계산 (원본 CHKYN을 그대로 믿지 않음)
    ///   - 입/출고 구분은 1차/2차 중량 크기 비교로 재계산 (원본 JOBGU를 그대로 믿지 않음)
    ///   - 원본 키(SLINO)는 SOURCE_KEY 컬럼에 그대로 보존
    /// </summary>
    public class SyncOrchestrator
    {
        private readonly MesSourceReader _source;
        private readonly HubWriter _hub;

        public SyncOrchestrator(MesSourceReader source, HubWriter hub)
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

                // TODO: 품목 마스터는 MesSourceReader.GetProducts() 구현 후 여기에 추가.

                // 계근 데이터는 최근 N일치를 매번 다시 훑는다. MEASURE_RETR이 계량일자(TDATE)로만
                // 조회 조건을 받고 수정일시 기준 필터를 지원하지 않기 때문에, 검수/수정이 늦게
                // 들어온 건도 놓치지 않으려면 그 지연 가능성만큼 기간을 넉넉히 잡아야 한다.
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

        /// <summary>MES 원본 1건을 통합 스키마 규칙에 맞게 변환한다.</summary>
        private static WeighRecordForHub Transform(RawWeighRow raw)
        {
            var isCompleted = raw.SWeit.HasValue && raw.SWeit.Value != 0;

            string inOutType = null;
            if (raw.FWeit.HasValue && raw.SWeit.HasValue && raw.FWeit.Value != 0 && raw.SWeit.Value != 0)
            {
                // SA015F00.cs CalculateWeight()와 동일한 규칙: 1차 >= 2차면 입고, 아니면 출고.
                inOutType = raw.FWeit.Value >= raw.SWeit.Value ? "I" : "O";
            }

            decimal? netWeight = null;
            if (raw.FWeit.HasValue && raw.SWeit.HasValue)
            {
                netWeight = Math.Abs(raw.FWeit.Value - raw.SWeit.Value);
            }

            int? weighSeq;
            int seqParsed;
            weighSeq = int.TryParse(raw.SeqNo, out seqParsed) ? seqParsed : (int?)null;

            return new WeighRecordForHub
            {
                SourceKey = raw.Slino,
                WeighDate = raw.TDate.Date,
                WeighSeq = weighSeq,
                VehicleNo = raw.CarNo,
                CustomerSourceCode = raw.CvCod,
                CustomerName = raw.CvNam,
                ProductSourceCode = raw.ItCod,
                ProductName = raw.ItNam,
                FirstDateTime = raw.FTime,
                FirstWeight = raw.FWeit,
                SecondDateTime = raw.STime,
                SecondWeight = raw.SWeit,
                NetWeight = netWeight,
                LossWeight = raw.LWeit,
                UnitPrice = raw.UCost,
                InOutType = inOutType,
                IsCompleted = isCompleted,
                WeigherName = raw.PlnNm,
                Remark = raw.InsRk,
                SourceRawStatus = raw.ChkYn,
            };
        }
    }
}
